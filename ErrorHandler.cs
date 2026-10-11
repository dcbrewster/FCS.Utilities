using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Data.OleDb;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace FCS.Utilities;

/// <summary>
/// A utility class for handling exceptions and generating detailed error reports.
/// The main functionality is to create an HTML report containing comprehensive information about the exception, the environment,
/// and the HTTP request (if available), and then send this report via email to a specified address.
/// </summary>
public class ErrorHandler
{
    #region Private Properties

    /// <summary>
    /// List of substrings to check for in the user agent to determine if the request is likely from a bot/crawler.
    /// This is used to add an email header which can be used to filter out bot requests in email clients that support filtering based on headers.
    /// </summary>
    private static readonly List<string> BotList =
    [
        "BLEXBot/",
        "www.bing.com/bingbot.htm",
        "DomainStatsBot/",
        "facebookexternalhit",
        "Googlebot/",
        "gsa-crawler",
        "OpenSearchServer_Bot"
    ];

    /// <summary>
    /// List of substrings to check for in keys when sanitizing values.
    /// If any of these substrings are found in a key, the value will be redacted in the error report.
    /// </summary>
    private static readonly string[] SensitiveKeyParts =
    [
        "password",
        "pwd",
        "token",
        "secret",
        "authorization",
        "auth",
        "cookie",
        "apikey",
        "api-key",
        "bearer",
        "jwt",
        "creditcard",
        "cardnumber",
        "cvv",
        "ssn",
        "socialsecurity",
        "viewstate",
        "eventvalidation"
    ];

    private readonly StringBuilder builder = new();                          // StringBuilder instance used to construct the HTML error report efficiently.
    private readonly object reportLock = new();                              // Protects the reusable builder if the same handler instance is used concurrently.
    private const string divider = "<tr><td class='divider' colspan='2'></td></tr>";    // A constant string for a divider row in the HTML report, used to visually separate different sections of the report for better readability.
    private const int MaxValueLength = 16_384;
    private const string RedactedValue = "*** REDACTED ***";

    #endregion Private Properties

    #region Public Methods

    /// <summary>
    /// Reports an exception by generating a detailed HTML error report and sending it via email.
    /// The report includes information about the exception, the environment, and the HTTP request (if available).
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    /// <param name="appName">The name of the application where the error occurred.</param>
    /// <param name="httpContext">The HTTP context that caused the error.</param>
    /// <param name="environment">The hosting environment.</param>
    public void ReportError(
        Exception ex,
        string appName,
        HttpContext httpContext,
        IHostEnvironment? environment = null)
    {
        if(ex == null) return;     // If there is no exception to report, exit the method early to avoid unnecessary processing.

        // Resolve the application name to be used in the error report and email subject. This allows for a default application name
        // to be specified in app settings if one is not provided when calling the method.
        appName = ResolveAppName(appName);

        // Build the HTML error report using the provided exception, application name, HTTP context, and hosting environment.
        // This method gathers all relevant information and formats it into an HTML string that can be included in the email.
        string htmlBody = BuildSafeErrorReport(ex, appName, httpContext, environment);

        // Send the error report via email, including the user agent from the HTTP request if available.
        SendErrorEmail(appName, htmlBody, httpContext?.Request.Headers.UserAgent.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Overload of the ReportError method that can be called when there is no HTTP context available (such as in a background task).
    /// This allows for error reporting in a wider range of scenarios, while still providing detailed information about the exception and environment.
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    /// <param name="appName">The name of the application where the error occurred.</param>
    /// <param name="environment">The hosting environment.</param>
    public void ReportError(
        Exception ex,
        string appName,
        IHostEnvironment? environment = null)
    {
        if(ex == null) return;             // If there is no exception to report, exit the method early to avoid unnecessary processing.

        // Resolve the application name to be used in the error report and email subject.
        // This allows for a default application name to be specified in app settings if one is not provided when calling the method.
        appName = ResolveAppName(appName);

        // Build the HTML error report using the provided exception, application name, and hosting environment. Since there is no HTTP context, the report will be generated without any request-specific information. This is useful for scenarios where there is no HTTP context, such as in background tasks or console applications.
        string htmlBody = BuildSafeErrorReport(ex, appName, null, environment);

        SendErrorEmail(appName, htmlBody, "");
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Resolves the application name to be used in the error report and email subject.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <returns>The resolved application name.</returns>
    private static string ResolveAppName(string? appName)
    {
        return string.IsNullOrWhiteSpace(appName)
            ? AppDomain.CurrentDomain.FriendlyName
            : appName.Trim();
    }

    private string BuildSafeErrorReport(
        Exception ex,
        string appName,
        HttpContext? httpContext,
        IHostEnvironment? environment)
    {
        try
        {
            return BuildErrorReport(ex, appName, httpContext, environment);
        }
        catch(Exception reportException)
        {
            Trace.TraceError("ErrorHandler failed to build the full error report: " + reportException);

            return BuildFallbackReport(ex, appName, reportException);
        }
    }

    private static string BuildFallbackReport(Exception originalException, string appName, Exception reportException) =>
        $"<html><body><h1>{Html("An exception has occurred - " + appName)}</h1>" +
        $"<h2>Original exception</h2><pre>{Html(originalException.ToString())}</pre>" +
        $"<h2>Error report generation failure</h2><pre>{Html(reportException.ToString())}</pre></body></html>";

    /// <summary>
    /// Sends an email with the error report. The email settings (from address, to address, SMTP server) are read from app settings.
    /// </summary>
    /// <param name="appName">The name of the application where the error occurred.</param>
    /// <param name="htmlBody">The HTML content of the error report.</param>
    /// <param name="userAgent">The user agent string of the request that caused the error.</param>
    private static void SendErrorEmail(
        string appName,
        string htmlBody,
        string userAgent)
    {
        EmailConfig emailConfig = new();

        EmailHelper email = new()
        {
            FromAddress = emailConfig.FromAddress,              // Read the from address for the email from app settings, which allows for configuration without changing code.
            FromName = emailConfig.FromName,                    // Read the from name for the email from app settings, which allows for configuration without changing code. This is the display name that will appear in the recipient's email client as the sender of the error report.
            Subject = $"AN EXCEPTION HAS OCCURRED - {appName}", // Set the email subject to include the application name, which helps to identify where the error occurred when receiving the email.
            HtmlBody = htmlBody
        };

        string toAddress = emailConfig.ToAddress;               // Read the to address for the email from app settings, which allows for configuration without changing code. This is the address that error reports will be sent to.

        // If a to address is specified in app settings, add it to the email's to address list. This allows for error reports to
        // be sent to a configured email address.
        if(!string.IsNullOrWhiteSpace(toAddress)) email.ToAddressList.Add(new Tuple<string, string>(toAddress.Trim(), string.Empty));

        string ccAddresses = emailConfig.CCAddress;             // Read the CC addresses for the email from app settings, which allows for configuration without changing code. This can be a comma-separated list of email addresses that should receive a copy of the error report.

        if(!string.IsNullOrWhiteSpace(ccAddresses))
        {
            // If CC addresses are specified in app settings, split the string by commas and add each address to the email's CC address list.
            // This allows for error reports to be sent to multiple configured email addresses.
            foreach(string item in ccAddresses.Split(','))
                if(!string.IsNullOrWhiteSpace(item)) email.CCAddressList.Add(new Tuple<string, string>(item.Trim(), string.Empty));
        }

        if(email.ToAddressList.Count == 0 && email.CCAddressList.Count == 0 && email.BCCAddressList.Count == 0)
        {
            Trace.TraceWarning("ErrorHandler email was not sent because no recipients are configured.");
            return;
        }

        // If the user agent indicates that the request is likely from a bot, add a header to the email that can be used to filter out bot
        // requests in email clients that support filtering based on headers. This helps to reduce noise in error reports caused by bots and crawlers.
        if(IsItABot(userAgent)) email.AddHeader("X-Bot", "ItsABot");

        try
        {
            // Attempt to send the email and capture the result, which includes whether the send was successful and any error message if it failed.
            Tuple<bool, string> result = email.Send();

            // Log a warning if the email failed to send, including any error message returned by the Send method.
            // This helps to identify issues with the error reporting mechanism itself, such as misconfigured email settings or SMTP server issues.
            if(result?.Item1 == false) Trace.TraceWarning("ErrorHandler email send failed: " + result.Item2);
        }
        catch(Exception sendException)
        {
            // If there was an exception while trying to send the email, log an error with the exception details.
            // This ensures that issues with the error reporting mechanism itself are captured in logs, even if the email cannot be sent.
            Trace.TraceError("ErrorHandler failed to send error email: " + sendException);
        }
    }

    /// <summary>
    /// Builds the HTML error report by gathering information about the exception, the environment, and the HTTP context (if available).
    /// </summary>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="appName">The name of the application.</param>
    /// <param name="httpContext">The HTTP context, if available.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns></returns>
    private string BuildErrorReport(
        Exception ex,
        string appName,
        HttpContext? httpContext,
        IHostEnvironment? environment)
    {
        lock(reportLock)
        {
            builder.Clear();

        // Start the HTML document and body for the error report, and append the CSS styles for the report, which are defined inline here.
        // This ensures that the report is formatted in a consistent and readable way when viewed in an email client or browser.
            builder.Append("<html><body>")
            .Append(GetStyleHtml())
            .Append("<table class='exception'>");

            AppendHeader($"An exception has occurred - {appName}");
            AppendSystemOutput(httpContext, environment);
            AppendExceptionOutput(ex);
            AppendAggregateExceptionOutput(ex);
            AppendRequestOutput(httpContext);
            AppendActivityOutput();
            AppendSpecificOutput(ex);

            builder.Append("</table>")                               // Close the table element in the HTML report.
            .Append("</body></html>");                           // Close the body and html elements in the HTML report.

            return builder.ToString()
                .Replace("\r\n", "<br />")
                .Replace("\n", "<br />");
        }
    }

    /// <summary>
    /// Appends system and environment information to the error report, including machine name, OS version, .NET version, application domain, timestamps, and hosting environment details.
    /// </summary>
    /// <param name="httpContext">The HTTP context of the current request.</param>
    /// <param name="environment">The hosting environment information.</param>
    private void AppendSystemOutput(
        HttpContext? httpContext,
        IHostEnvironment? environment)
    {
        AppendCommonSystemOutput();                                                 // Append common system and environment information to the report, which includes things like machine name, OS version, .NET version, application domain, and timestamps. This provides important context about the environment in which the error occurred, which can be critical for diagnosing and fixing the issue.

        if(environment != null)
        {
            AppendRow("Environment", environment.EnvironmentName);          // Append the hosting environment name (like Development, Staging, Production) to the report, which can provide important context about the environment in which the error occurred and can help with diagnosing issues that may be specific to certain environments or configurations.
            AppendRow("Application Name", environment.ApplicationName);     // Append the application name from the hosting environment to the report, which can provide important context about which application an error occurred in when multiple applications are running on the same server, and can help with diagnosing issues related to specific applications or configurations.
            AppendRow("Content Root", environment.ContentRootPath);         // Append the content root path of the application to the report, which can provide important context about the file system environment of the application and can help with diagnosing issues related to file paths, permissions, or missing resources.
        }

        if(httpContext == null) return;                                                // If there is no HTTP context available, skip the rest of this method, as the remaining information is all related to the HTTP request and would not be applicable. This allows the error report to still include valuable system and environment information even when there is no HTTP context.

        HttpRequest request = httpContext.Request;                                       // Get the HTTP request from the context to extract information about the request that caused the error. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue.

        AppendRow("Trace Identifier", httpContext.TraceIdentifier);                 // Append the trace identifier of the HTTP context to the report, which can be useful for correlating logs and tracking the flow of a specific request through the system.
        AppendRow("HTTP Method", request.Method);                                   // Append the HTTP method (GET, POST, etc.) of the request to the report, which can provide context about the type of operation that was being performed when the error occurred.
        AppendRow("Scheme", request.Scheme);                                        // Append the scheme (http or https) of the request to the report, which can provide context about the protocol being used for the request.
        AppendRow("Host", request.Host.ToString());                                 // Append the host of the request to the report, which can provide context about the server or domain handling the request.
        AppendRow("Path", request.Path.ToString());                                 // Append the path of the request to the report, which can provide context about the specific endpoint or resource being accessed when the error occurred.
        string redactedQueryString = BuildRedactedQueryString(request.Query);
        AppendRow("Query String", redactedQueryString);
        AppendRow("Full URL", $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{redactedQueryString}");
        AppendRow("Path Base", request.PathBase.ToString());
        AppendRow("Remote IP", httpContext.Connection.RemoteIpAddress?.ToString()!);// Append the remote IP address of the client making the request, which can provide context about the source of the request.
        AppendRow("Remote Port", httpContext.Connection.RemotePort.ToString());
        AppendRow("Local IP", httpContext.Connection.LocalIpAddress?.ToString()!);  // Append the local IP address of the server handling the request, which can provide context about the server's network environment.
        AppendRow("Local Port", httpContext.Connection.LocalPort.ToString());
        AppendRow("Protocol", request.Protocol);
        AppendRow("User Agent", request.Headers["User-Agent"].ToString());      // Append the User-Agent header of the request, which can provide context about the client's browser or application making the request.
        AppendRow("Referrer", request.Headers.Referer.ToString());                  // Append the Referer header of the request, which can provide context about the previous page or resource that led to the request.
        AppendRow("Content Type", request.ContentType ?? string.Empty);
        AppendRow("Content Length", request.ContentLength?.ToString() ?? string.Empty);
        AppendRow("Request ID", request.Headers["Request-Id"].ToString());
        AppendRow("Trace Parent", request.Headers["traceparent"].ToString());
        AppendRow("Forwarded For", SanitizeValue("X-Forwarded-For", request.Headers["X-Forwarded-For"].ToString()));
        AppendRow("Endpoint", httpContext.GetEndpoint()?.DisplayName ?? string.Empty);
        AppendRow("Authenticated", httpContext.User?.Identity?.IsAuthenticated.ToString()!); // Append whether the user is authenticated, which can provide context about the user's authentication status.
        AppendRow("User", httpContext.User?.Identity?.Name ?? "Anonymous");         // Append the name of the authenticated user, or "Anonymous" if not authenticated, which can provide context about the user involved in the request.

        // Append the roles of the authenticated user to the report.
        // This is done by looking for claims that end with "/role" or are of type "role", which is a common convention for representing user roles in claims-based authentication.
        // The roles are then joined into a comma-separated string for display in the report. This can provide important context about the user's permissions and access level,
        // which can be critical for diagnosing authorization issues or understanding the security context of the request.
        string roles = string.Join(
            ", ",
            httpContext.User?.Claims
                .Where(c => c.Type.EndsWith("/role", StringComparison.OrdinalIgnoreCase) || c.Type == "role")
                .Select(c => c.Value)
                .Distinct()
            ?? []);

        // Append the roles to the report, which can provide important context about the user's permissions and access level,
        // and can be critical for diagnosing authorization issues or understanding the security context of the request.
        AppendRow("Roles", roles);
    }

    /// <summary>
    /// Appends details about the HTTP request to the error report, including headers, query string variables, form values, and cookies.
    /// Since this information can often contain sensitive data, the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue, while also taking care to avoid exposing sensitive information in the error report.
    /// </summary>
    /// <param name="httpContext"></param>
    private void AppendRequestOutput(HttpContext? httpContext)
    {
        if(httpContext == null) return;                                          // If there is no HTTP context available, skip this section entirely, as all of the information in this method is related to the HTTP request and would not be applicable. This allows the error report to still include valuable information about the exception and environment even when there is no HTTP context.

        HttpRequest request = httpContext.Request;                                // Get the HTTP request from the context to extract information about the request that caused the error. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue.

        AppendHeaderDictionary("Headers", request.Headers);                 // Append the headers of the HTTP request to the error report. Headers can often contain sensitive information, so the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue, while also taking care to avoid exposing sensitive information in the error report.
        AppendQueryCollection("Query String Variables", request.Query);     // Append the query string variables of the HTTP request to the error report. Query string variables can often contain sensitive information, so the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue, while also taking care to avoid exposing sensitive information in the error report.
        AppendCookieDictionary("Cookies", request.Cookies);                 // Append the cookies of the HTTP request to the error report. Cookies can often contain sensitive information, so the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue, while also taking care to avoid exposing sensitive information in the error report.

        if(request.HasFormContentType)
        {
            try
            {
                // Append the form values of the HTTP request to the error report. Form values can often contain sensitive information,
                // so the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary.
                // This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue,
                // while also taking care to avoid exposing sensitive information in the error report.
                AppendFormCollection("Form Values", request.Form);
            }
            catch(Exception ex)
            {
                // If there was an issue reading the form values (which can happen if the form data is malformed or if there is an issue with the request),
                // append a row to the report indicating that the form values could not be read, along with the exception message. This ensures that we
                // capture the fact that there was an issue with the form data, which can be important context for diagnosing the issue, without causing
                // the entire error reporting process to fail due to an issue with reading the form values.
                AppendRow("Form Values", "Unable to read form: " + ex.Message);
            }
        }

    }

    private static string BuildRedactedQueryString(IQueryCollection query)
    {
        if(query == null || query.Count == 0) return string.Empty;

        List<string> parts = [];

        foreach(KeyValuePair<string, StringValues> item in query.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            if(ShouldSkipKey(item.Key)) continue;

            foreach(string? value in item.Value)
            {
                string sanitizedValue = SanitizeValue(item.Key, value ?? string.Empty);
                parts.Add($"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(sanitizedValue)}");
            }
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private void AppendActivityOutput()
    {
        Activity? activity = Activity.Current;
        if(activity == null) return;

        AppendSection("Diagnostic Activity");
        AppendRow("Activity ID", activity.Id ?? string.Empty);
        AppendRow("Trace ID", activity.TraceId.ToString());
        AppendRow("Span ID", activity.SpanId.ToString());
        AppendRow("Parent Span ID", activity.ParentSpanId.ToString());
        AppendRow("Operation Name", activity.OperationName);
        AppendRow("Display Name", activity.DisplayName);
        AppendRow("Start Time UTC", activity.StartTimeUtc.ToString("O"));
        AppendRow("Duration", activity.Duration.ToString());

        foreach(KeyValuePair<string, string?> tag in activity.Tags)
            AppendRow($"Tag: {tag.Key}", SanitizeValue(tag.Key, tag.Value ?? string.Empty));

        foreach(ActivityEvent activityEvent in activity.Events)
            AppendRow("Event", $"{activityEvent.Name} at {activityEvent.Timestamp:O}");
    }

    /// <summary>
    /// Appends headers from the HTTP request to the error report. Headers can often contain sensitive information, so the keys are checked
    /// against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context
    /// about the request that led to the error, which can be critical for diagnosing and fixing the issue,
    /// while also taking care to avoid exposing sensitive information in the error report.
    /// </summary>
    /// <param name="title">The title of the section to append.</param>
    /// <param name="headers">The header collection to append.</param>
    private void AppendHeaderDictionary(
        string title,
        IHeaderDictionary headers)
    {
        if(headers == null || headers.Count == 0) return;      // If there are no headers, skip this section entirely, as it would not be applicable and would just add unnecessary clutter to the report. This allows the error report to still include valuable information about the exception and environment without including empty sections when there are no headers.

        AppendSection(title);                                   // Add a header for the headers section in the report, which helps to organize the report and make it easier to understand the different types of information being presented.

        // Iterate through each header and append it to the report, sanitizing values as needed. Headers can often contain sensitive information,
        // so the keys are checked against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary.
        // This provides important context about the request that led to the error, which can be critical for diagnosing and fixing the issue,
        // while also taking care to avoid exposing sensitive information in the error report.
        foreach(var header in headers)
            AppendRow(header.Key, SanitizeValue(header.Key, header.Value.ToString()));
    }

    /// <summary>
    /// Appends query string variables from the HTTP request to the error report. Query string variables can often contain sensitive information, so the keys are checked
    /// against a list of sensitive terms in the SanitizeValue method, and values are redacted if necessary. This provides important context about the request that led to the error,
    /// which can be critical for diagnosing and fixing the issue, while also taking care to avoid exposing sensitive information in the error report.
    /// </summary>
    /// <param name="title">The title of the section to append.</param>
    /// <param name="query">The query string collection to append.</param>
    private void AppendQueryCollection(
        string title,
        IQueryCollection query)
    {
        if(query == null || query.Count == 0) return;      // If there are no query string variables, skip this section entirely, as it would not be applicable and would just add unnecessary clutter to the report. This allows the error report to still include valuable information about the exception and environment without including empty sections when there are no query string variables.

        AppendSection(title);                               // Add a header for the query string variables section in the report, which helps to organize the report and make it easier to understand the different types of information being presented.

        // Iterate through each query string variable and append it to the report, sanitizing values as needed. Query string variables can often contain sensitive information,
        foreach(var item in query)
            AppendRow(item.Key, SanitizeValue(item.Key, item.Value.ToString()));
    }

    /// <summary>
    /// Appends form values from the HTTP request to the error report. Form values can sometimes contain sensitive information,
    /// so the keys are checked against a list of sensitive terms,
    /// </summary>
    /// <param name="title">The title of the section to append.</param>
    /// <param name="form">The form collection to append.</param>
    private void AppendFormCollection(
        string title,
        IFormCollection form)
    {
        if(form == null || form.Count == 0) return;                                // If there are no form values, skip this section entirely

        AppendSection(title);                                                       // Add a header for the form section

        foreach(var item in form)                       // Iterate through each form field and append it to the report, sanitizing values as needed
        {
            if(ShouldSkipKey(item.Key)) continue;                                  // Skip keys that are known to contain large amounts of data or sensitive information (like __VIEWSTATE in ASP.NET Web Forms)

            AppendRow(item.Key, SanitizeValue(item.Key, item.Value.ToString()));    // Append the form field to the report, sanitizing the value if the key indicates it may contain sensitive information
        }
    }

    /// <summary>
    /// Appends cookies from the HTTP request to the error report.
    /// Cookies can often contain sensitive information, so the keys are checked against a list of sensitive terms,
    /// </summary>
    /// <param name="title">The title of the section to append.</param>
    /// <param name="cookies">The cookie collection to append.</param>
    private void AppendCookieDictionary(
        string title,
        IRequestCookieCollection cookies)
    {
        if(cookies == null || cookies.Count == 0) return;                          // If there are no cookies, skip this section entirely

        AppendSection(title);                                                       // Add a header for the cookies section

        foreach(var cookie in cookies)                      // Iterate through each cookie and append it to the report, sanitizing values as needed
            AppendRow(cookie.Key, SanitizeValue(cookie.Key, cookie.Value));        // Append each cookie to the report, sanitizing the value if the key indicates it may contain sensitive information
    }

    /// <summary>
    /// Appends common system and environment information to the error report. This includes things like machine name, OS version,
    /// .NET version, application domain, and timestamps.
    /// </summary>
    private void AppendCommonSystemOutput()
    {
        AppendSection("Application / Runtime Info");

        AppendRow("UTC Time", DateTimeOffset.UtcNow.ToString("O"));       // Use ISO 8601 format for timestamps to ensure they are unambiguous and include timezone information
        AppendRow("Local Time", DateTimeOffset.Now.ToString("O"));        // Include both UTC and local time in the report, as this can be helpful for correlating with logs and understanding when the error occurred in the context of the server's timezone
        AppendRow("Machine Name", Environment.MachineName);                     // Include the machine name in the report, as this can be helpful for identifying which server an error occurred on in a load-balanced environment
        AppendRow("OS Version", Environment.OSVersion.ToString());              // Include the operating system version in the report, as this can be helpful for identifying issues that may be specific to certain OS versions or configurations
        AppendRow(".NET Version", Environment.Version.ToString());              // Include the .NET version in the report, as this can be helpful for identifying issues that may be specific to certain versions of the .NET runtime
        AppendRow("App Domain", AppDomain.CurrentDomain.FriendlyName);          // Include the application domain name in the report, as this can be helpful for identifying which application an error occurred in when multiple applications are running on the same server
        AppendRow("Base Directory", AppDomain.CurrentDomain.BaseDirectory);     // Include the base directory of the application in the report, as this can be helpful for understanding the context of the error and for identifying potential issues with file paths or permissions
        AppendRow("64-bit Process", Environment.Is64BitProcess.ToString());     // Include whether the process is 64-bit in the report, as this can be helpful for identifying issues that may be specific to 32-bit vs 64-bit environments
        AppendRow("Process ID", GetProcessId());
        AppendRow("Process Name", Environment.ProcessPath ?? string.Empty);
        AppendRow("Managed Thread ID", Environment.CurrentManagedThreadId.ToString());
        AppendRow("Current Culture", System.Globalization.CultureInfo.CurrentCulture.Name);
        AppendRow("Current UI Culture", System.Globalization.CultureInfo.CurrentUICulture.Name);
        AppendRow("Activity ID", Activity.Current?.Id ?? string.Empty);
    }

    /// <summary>
    /// Gets the current process ID. Wrapped in a try-catch to avoid any potential issues with accessing process information
    /// in certain environments (like sandboxed hosting).
    /// </summary>
    /// <returns>The process ID as a string, or an empty string if it cannot be retrieved.</returns>
    private static string GetProcessId()
    {
        try
        {
            // Attempt to get the current process ID, which can be helpful for correlating with server logs and identifying which
            // process an error occurred in, especially in environments where multiple processes may be running
            return Process.GetCurrentProcess().Id.ToString();
        }
        catch
        {
            // If there is an issue accessing process information (which can happen in certain hosting environments), return an
            // empty string instead of throwing an exception, as we don't want the error reporting itself to fail due to an issue with gathering system information
            return string.Empty;
        }
    }

    /// <summary>
    /// Appends detailed information about the exception and all inner exceptions to the error report.
    /// This includes type, message, source, stack trace, and any custom data.
    /// </summary>
    /// <param name="ex">The exception to extract information from.</param>
    private void AppendExceptionOutput(Exception ex)
    {
        int depth = 0;                                                                      // Initialize a depth counter to keep track of how deep we are in the inner exception chain. This will be used to label inner exceptions appropriately in the report.

        for(Exception current = ex; current != null; current = current.InnerException!)     // Iterate through the exception and all inner exceptions, appending details for each one. This ensures that we capture the full chain of exceptions, which can be critical for diagnosing complex issues where one exception is caused by another.
        {
            AppendSection(depth == 0 ? "Exception" : $"Inner Exception #{depth}");          // Add a section header for each exception in the chain, indicating whether it's the main exception or an inner exception, and if it's an inner exception, which number it is in the chain. This helps to organize the report and make it easier to understand the relationship between exceptions.

            Type type = current.GetType();                                                  // Get the type of the current exception, which is important information for understanding what kind of error occurred and can help with diagnosing the issue and finding relevant documentation or solutions.

            AppendRow("Type", type.FullName!);                                          // Append the full type name of the exception to the report, as this is critical information for understanding what kind of error occurred and can help with diagnosing the issue and finding relevant documentation or solutions.
            AppendRow("Message", current.Message);                                      // Append the exception message to the report, as this often contains important details about what went wrong and can provide clues for diagnosing the issue.
            AppendRow("Source", current.Source!);                                       // Append the source of the exception, which indicates the name of the application or object that caused the error.
            AppendRow("HResult", current.HResult.ToString());                           // Append the HRESULT, which is a coded numerical value assigned to the exception.
            AppendRow("Target Site", current.TargetSite?.ToString()!);                  // Append the target site, which indicates the method that threw the exception.
            AppendRow("Stack Trace", current.StackTrace!);                              // Append the stack trace, which provides a detailed path of method calls leading to the exception.

            AppendExceptionData(current);                                                   // Append any custom data from the exception's Data property, which can include additional information that was added to the exception when it was thrown. This can be very helpful for diagnosing issues, as developers often include key-value pairs in the Data property to provide extra context about the error.

            depth++;                                                                        // Increment the depth counter as we go deeper into the inner exception chain, so that inner exceptions can be labeled appropriately in the report.
        }
    }

    /// <summary>
    /// Appends any custom data from the exception's Data property to the error report.
    /// This can include any additional information that was added to the exception when it was thrown.
    /// </summary>
    /// <param name="ex">The exception to extract custom data from.</param>
    private void AppendExceptionData(Exception ex)                             // Append any custom data from the exception's Data property to the report. The Data property is a collection of key-value pairs that can contain any additional information that was added to the exception when it was thrown. This allows developers to include extra context about the error that may not be captured in the standard exception properties, and can be very helpful for diagnosing issues.
    {
        if(ex.Data == null || ex.Data.Count == 0) return;                       // If there is no custom data in the exception, skip this section entirely to keep the report concise and focused on relevant information.

        AppendSection("Exception Data");                                   // Add a section header for the exception data, which is a collection of key-value pairs that can contain any additional information that was added to the exception when it was thrown. This allows developers to include extra context about the error that may not be captured in the standard exception properties, and can be very helpful for diagnosing issues.

        foreach(DictionaryEntry item in ex.Data)                                // Iterate through each key-value pair in the exception's Data property and append it to the report, sanitizing values as needed. This allows any additional context that was added to the exception to be included in the error report, which can be very helpful for diagnosing issues.
        {
            AppendRow(
                item.Key?.ToString()!,                                          // Append the key of the data item as the label for the row. This allows any additional context that was added to the exception to be included in the error report, which can be very helpful for diagnosing issues.
                SanitizeValue(item.Key?.ToString()!, item.Value?.ToString()!)); // Append each key-value pair from the exception's Data property to the report, sanitizing the value if the key indicates it may contain sensitive information. This allows any additional context that was added to the exception to be included in the error report, which can be very helpful for diagnosing issues.
        }
    }

    /// <summary>
    /// Appends details of any inner exceptions contained within an AggregateException.
    /// This is important for scenarios where multiple exceptions may be thrown together (like in parallel processing).
    /// </summary>
    /// <param name="ex">The exception to extract information from.</param>
    private void AppendAggregateExceptionOutput(Exception ex)
    {
        if(ex is not AggregateException aggregateException) return;                                 // If the exception is not an AggregateException, skip this section entirely, as it is only relevant for exceptions that can contain multiple inner exceptions.

        AppendSection("Aggregate Exception Details");                                          // Add a section header for the aggregate exception details, which will include information about each inner exception contained within the AggregateException. This is important for scenarios where multiple exceptions may be thrown together (like in parallel processing), as it allows us to capture and report on all of the exceptions that occurred, rather than just the top-level AggregateException.

        int index = 1;                                                                              // Initialize an index counter to label each inner exception in the aggregate exception details section. This helps to organize the report and make it easier to understand the relationship between the exceptions contained within the AggregateException.

        foreach(Exception innerException in aggregateException.Flatten().InnerExceptions)           // Iterate through each inner exception contained within the AggregateException (using Flatten to get all inner exceptions at all levels) and append details for each one. This ensures that we capture the full set of exceptions that occurred in scenarios where multiple exceptions may be thrown together (like in parallel processing), which can be critical for diagnosing complex issues.
        {
            AppendRow($"Aggregate Inner #{index} Type", innerException.GetType().FullName!);  // Append the type of each inner exception contained within the AggregateException, which is important information for understanding what kinds of errors occurred and can help with diagnosing the issue and finding relevant documentation or solutions.
            AppendRow($"Aggregate Inner #{index} Message", innerException.Message);           // Append the message of each inner exception, which provides a human-readable description of the error and can help with diagnosing the issue.
            AppendRow($"Aggregate Inner #{index} Stack Trace", innerException.StackTrace!);   // Append the stack trace of each inner exception, which shows the sequence of method calls that led to the exception and can be invaluable for debugging.

            index++;                                                                                // Increment the index counter for each inner exception, so that they can be labeled appropriately in the report and it's clear which details correspond to which inner exception.
        }
    }

    /// <summary>
    /// Appends information about the HTTP request that caused the exception, including form values, query string variables, cookies, and headers.
    /// </summary>
    /// <param name="title">The title of the section to be appended.</param>
    /// <param name="values">The collection of name-value pairs to append.</param>
    private void AppendNameValueCollection(
        string title,
        NameValueCollection values)
    {
        if(values == null || values.Count == 0) return;             // If there are no name-value pairs in the collection, skip this section entirely to keep the report concise and focused on relevant information.

        AppendSection(title);                                       // Add a section header for this collection of name-value pairs, which can include things like form values, query string variables, or server variables from the HTTP request. This helps to organize the report and make it easier to understand the context of the information being presented.

        foreach(string? key in values.AllKeys)                      // Iterate through each name-value pair in the collection and append it to the report, sanitizing values as needed. This allows us to include important details about the HTTP request that caused the exception, while still protecting sensitive information from being exposed in the error report.
        {
            if(ShouldSkipKey(key!)) continue;                       // Skip keys that are known to contain large amounts of data or sensitive information (like __VIEWSTATE in ASP.NET Web Forms), as including them in the report can make it harder to read and may expose sensitive information.

            AppendRow(key!, SanitizeValue(key!, values[key]!));     // Append each name-value pair to the report, sanitizing the value if the key indicates it may contain sensitive information. This allows us to include important details about the HTTP request that caused the exception, while still protecting sensitive information from being exposed in the error report.
        }
    }

    /// <summary>
    /// Determines if a given key should be skipped when appending request data to the error report. This is used to avoid including sensitive information
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static bool ShouldSkipKey(string key)
    {
        if(string.IsNullOrWhiteSpace(key)) return false;                                          // If the key is null, empty, or consists only of white-space characters, it should not be skipped.

        return string.Equals(key, "__VIEWSTATE", StringComparison.OrdinalIgnoreCase)           // Skip keys that are known to contain large amounts of data or sensitive information, such as __VIEWSTATE and __EVENTVALIDATION in ASP.NET Web Forms, as including them in the report can make it harder to read and may expose sensitive information.
            || string.Equals(key, "__VIEWSTATEGENERATOR", StringComparison.OrdinalIgnoreCase)  // __VIEWSTATEGENERATOR is another key used in ASP.NET Web Forms that can contain a large amount of data, so it should also be skipped for the same reasons as __VIEWSTATE.
            || string.Equals(key, "__EVENTVALIDATION", StringComparison.OrdinalIgnoreCase);    // __EVENTVALIDATION is another key used in ASP.NET Web Forms that can contain a large amount of data, so it should also be skipped for the same reasons as __VIEWSTATE.
    }

    /// <summary>
    /// Appends specific details about certain types of exceptions to the error report. For example, if the exception is a SqlException,
    /// it will include details about the SQL error.
    /// </summary>
    /// <param name="ex">The exception to extract specific details from.</param>
    private void AppendSpecificOutput(Exception ex)             // Appends specific details about certain types of exceptions to the error report. For example, if the exception is a SqlException, it will include details about the SQL error, which can be critical for diagnosing database-related issues. This method can be expanded to include additional exception types as needed, allowing for more detailed reporting on specific kinds of errors.
    {
        bool appendedSpecificDetails = false;

        for(Exception? current = ex; current != null; current = current.InnerException)
        {
            if(current is SqlException sqlException)                     // If the exception is a SqlException, append detailed information about the SQL error, including all SQL errors contained within the exception. This can be critical for diagnosing database-related issues, as it provides insights into what went wrong at the database level.
            {
                if(!appendedSpecificDetails) AppendSection("Exception Specifics");
                AppendSqlException(sqlException);                   // Call a helper method to append detailed information about the SqlException, including all SQL errors contained within the exception. This keeps the code organized and allows for more focused handling of the specific details related to SQL exceptions.
                appendedSpecificDetails = true;
            }

            if(current is OleDbException oleDbException)                 // If the exception is an OleDbException, append detailed information about the OleDb error, including all OleDb errors contained within the exception. This can be critical for diagnosing database-related issues that involve OLE DB providers, as it provides insights into what went wrong at the database level.
            {
                if(!appendedSpecificDetails) AppendSection("Exception Specifics");
                AppendOleDbException(oleDbException);               // Call a helper method to append detailed information about the OleDbException, including all OleDb errors contained within the exception. This keeps the code organized and allows for more focused handling of the specific details related to OLE DB exceptions.
                appendedSpecificDetails = true;
            }

            if(current is AggregateException aggregateException)
            {
                foreach(Exception innerException in aggregateException.Flatten().InnerExceptions)
                {
                    if(innerException is SqlException aggregateSqlException)
                    {
                        if(!appendedSpecificDetails) AppendSection("Exception Specifics");
                        AppendSqlException(aggregateSqlException);
                        appendedSpecificDetails = true;
                    }

                    if(innerException is OleDbException aggregateOleDbException)
                    {
                        if(!appendedSpecificDetails) AppendSection("Exception Specifics");
                        AppendOleDbException(aggregateOleDbException);
                        appendedSpecificDetails = true;
                    }
                }
            }
        }

        // If the exception is not one of the specific types we handle, append a generic message indicating that there are no specific details available
        // for this type of exception. This ensures that the report still includes a complete structure, even if we don't have specific information to
        // include for certain types of exceptions.
        if(!appendedSpecificDetails)
        {
            AppendSection("Exception Specifics");
            AppendRow("Specific Details", "No database-specific details available.");
        }
    }

    /// <summary>
    /// Appends detailed information about a SqlException to the error report, including all SQL errors contained within the exception.
    /// </summary>
    /// <param name="sqlException">The SqlException to extract details from.</param>
    private void AppendSqlException(SqlException sqlException)
    {
        AppendRow("Server", sqlException.Server);                       // Append the name of the SQL server that generated the error, which can be helpful for diagnosing issues in environments with multiple database servers or instances.
        AppendRow("Line Number", sqlException.LineNumber.ToString());   // Append the line number where the SQL error occurred, which can be critical for diagnosing issues in SQL queries and understanding exactly where the error happened in the database code.
        AppendRow("Message", sqlException.Message);                     // Append the error message from the SqlException, which provides a human-readable description of the SQL error and can help with diagnosing the issue.
        AppendRow("Error Code", sqlException.ErrorCode.ToString());     // Append the error code from the SqlException, which is a numerical value that can provide additional information about the type of SQL error that occurred and can be useful for looking up specific error details in documentation.
        AppendRow("Procedure", sqlException.Procedure);                 // Append the name of the stored procedure (if any) that was executing when the SQL error occurred, which can be helpful for diagnosing issues in database code and understanding the context of the error.
        AppendRow("Severity Class", sqlException.Class.ToString());     // Append the severity class of the SQL error, which indicates the severity level of the error and can be useful for understanding the potential impact of the error on the database and application.
        AppendRow("State", sqlException.State.ToString());              // Append the state of the SQL error, which provides additional information about the error and can be useful for diagnosing issues in SQL queries and understanding the context of the error.
        AppendRow("Number", sqlException.Number.ToString());            // Append the error number of the SQL error, which is a numerical value that can provide additional information about the type of SQL error that occurred and can be useful for looking up specific error details in documentation.

        AppendSection("SQL Errors");                                     // Add a section header for the individual SQL errors contained within the SqlException. A SqlException can contain multiple SQL errors, so this section will list each one with its details, which can be critical for diagnosing complex database issues where multiple errors may occur in a single operation.

        foreach(SqlError error in sqlException.Errors)                        // Iterate through each individual SQL error contained within the SqlException and append its details to the report. This ensures that we capture all relevant information about the SQL errors that occurred, which can be critical for diagnosing complex database issues where multiple errors may occur in a single operation.
        {
            AppendRow("Number", error.Number.ToString());               // Append the error number of the individual SQL error, which is a numerical value that can provide additional information about the type of SQL error that occurred and can be useful for looking up specific error details in documentation.
            AppendRow("Line Number", error.LineNumber.ToString());      // Append the line number where the individual SQL error occurred, which can be critical for diagnosing issues in SQL queries and understanding exactly where the error happened in the database code.
            AppendRow("Procedure", error.Procedure);                    // Append the name of the stored procedure (if any) that was executing when the individual SQL error occurred, which can be helpful for diagnosing issues in database code and understanding the context of the error.
            AppendRow("Message", error.Message);                        // Append the error message from the individual SQL error, which provides a human-readable description of the SQL error and can help with diagnosing the issue.
            AppendRow("Source", error.Source);                          // Append the source of the individual SQL error, which indicates the name of the application or object that caused the error and can be helpful for diagnosing issues in database code and understanding the context of the error.
            AppendRow("State", error.State.ToString());                 // Append the state of the individual SQL error, which provides additional information about the error and can be useful for diagnosing issues in SQL queries and understanding the context of the error.
            AppendRow("Class", error.Class.ToString());                 // Append the severity class of the individual SQL error, which indicates the severity level of the error and can be useful for understanding the potential impact of the error on the database and application.
        }
    }

    /// <summary>
    /// Appends detailed information about an OleDbException to the error report, including all OleDb errors contained within the exception.
    /// </summary>
    /// <param name="oleDbException">The OleDbException to extract details from.</param>
    private void AppendOleDbException(OleDbException oleDbException)
    {
        AppendRow("Message", oleDbException.Message);                   // Append the error message from the OleDbException, which provides a human-readable description of the OLE DB error and can help with diagnosing the issue.
        AppendRow("Error Code", oleDbException.ErrorCode.ToString());   // Append the error code from the OleDbException, which is a numerical value that can provide additional information about the type of OLE DB error that occurred and can be useful for looking up specific error details in documentation.
        AppendSection("OleDb Errors");                                   // Add a section header for the individual OLE DB errors contained within the OleDbException. An OleDbException can contain multiple OLE DB errors, so this section will list each one with its details, which can be critical for diagnosing complex database issues that involve OLE DB providers where multiple errors may occur in a single operation.

        foreach(OleDbError error in oleDbException.Errors)                    // Iterate through each individual OLE DB error contained within the OleDbException and append its details to the report. This ensures that we capture all relevant information about the OLE DB errors that occurred, which can be critical for diagnosing complex database issues that involve OLE DB providers where multiple errors may occur in a single operation.
        {
            AppendRow("Native Error", error.NativeError.ToString());    // Append the native error code from the individual OLE DB error, which is a numerical value that can provide additional information about the type of OLE DB error that occurred and can be useful for looking up specific error details in documentation.
            AppendRow("Message", error.Message);                        // Append the error message from the individual OLE DB error, which provides a human-readable description of the OLE DB error and can help with diagnosing the issue.
            AppendRow("Source", error.Source);                          // Append the source of the individual OLE DB error, which indicates the name of the application or object that caused the error and can be helpful for diagnosing issues in database code and understanding the context of the error.
            AppendRow("SQL State", error.SQLState);                     // Append the SQL state from the individual OLE DB error, which provides additional information about the error and can be useful for diagnosing issues in SQL queries and understanding the context of the error.
        }
    }

    /// <summary>
    /// Appends a header row to the error report table. This is used to separate different sections of the report and make it easier to read.
    /// </summary>
    /// <param name="text">The text to display as the header.</param>
    private void AppendHeader(string text)
    {
        builder.Append("<tr class='header'><td colspan='2'>")
            .Append(Html(text))
            .Append("</td></tr>");
    }

    /// <summary>
    /// Appends a section label row to the error report table. This is used to group related information together under a common heading.
    /// </summary>
    /// <param name="text">The text to display as the section label.</param>
    private void AppendSection(string text)
    {
        builder.Append("<tr><td colspan='2' class='sectionLabel'>")
            .Append(Html(text))
            .Append("</td></tr>");
    }

    /// <summary>
    /// Appends a row of information to the error report table, consisting of a label and a value. The label is displayed in a bold font on a shaded background,
    /// </summary>
    /// <param name="label">The label to display for the row.</param>
    /// <param name="value">The value to display for the row.</param>
    private void AppendRow(string label, string value)
    {
        string safeValue = Truncate(value);

        builder.Append("<tr><td class='itemLabel'>")
            .Append(Html(label))
            .Append("</td><td>")
            .Append(Html(safeValue))
            .Append("</td></tr>")
            .Append(divider);
    }

    private static string Truncate(string value)
    {
        if(string.IsNullOrEmpty(value)) return string.Empty;

        return value.Length <= MaxValueLength
            ? value
            : string.Concat(value.AsSpan(0, MaxValueLength), $"... [truncated, original length {value.Length}]");
    }

    /// <summary>
    /// Sanitizes a value based on its associated key. If the key contains any substrings that are commonly associated with
    /// sensitive information (like "password" or "token"),
    /// </summary>
    /// <param name="key">The key associated with the value to be sanitized.</param>
    /// <param name="value">The value to be sanitized.</param>
    /// <returns>The sanitized value, or the original value if it is not considered sensitive.</returns>
    private static string SanitizeValue(string key, string value)
    {
        if(string.IsNullOrEmpty(key)) return value ?? string.Empty;

        bool isSensitive = SensitiveKeyParts.Any(part => key.Contains(part, StringComparison.OrdinalIgnoreCase));

        return isSensitive ? RedactedValue : value ?? string.Empty;
    }

    /// <summary>
    /// Encodes a string for safe display in HTML. This is used to prevent issues with special characters in the error report,
    /// and to help mitigate potential XSS risks if the error report is viewed in an email client or web browser.
    /// </summary>
    /// <param name="value">The string to encode for HTML display.</param>
    /// <returns>The HTML-encoded string.</returns>
    private static string Html(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

    /// <summary>
    /// Determines if the given user agent string belongs to a known bot/crawler.
    /// This is used to add a header to the error email which can help filter out bot requests in email clients that support filtering based on headers.
    /// </summary>
    /// <param name="userAgent">The user agent string to check.</param>
    /// <returns>True if the user agent belongs to a known bot/crawler; otherwise, false.</returns>
    private static bool IsItABot(string? userAgent)
    {
        // If the user agent string is null or empty, we can't determine if it's a bot, so we return false by default.
        // This means that if there is no user agent information available, we will treat the request as if it were not from a bot,
        // which is a safer assumption for error reporting purposes.
        if(string.IsNullOrEmpty(userAgent)) return false;

        // Check if the user agent string contains any of the known bot identifiers.
        // We use a case-insensitive comparison to ensure that we can match bot identifiers regardless of how they are capitalized in the user agent string.
        foreach(string bot in BotList)
            if(userAgent.Contains(bot, StringComparison.OrdinalIgnoreCase)) return true;

        // If none of the known bot identifiers are found in the user agent string, we return false, indicating that we do not believe this request is from a bot.
        return false;
    }

    /// <summary>
    /// Generates the HTML for the style section of the error report. This includes CSS styles to make the report more readable and
    /// visually organized when viewed in an email client or web browser.
    /// </summary>
    /// <returns>The HTML string containing the styles for the error report.</returns>
    private static string GetStyleHtml()
    {
        StringBuilder builder = new();

        builder.Append("<style>")
            .Append("body { font-family: Arial, sans-serif; font-size: 12px; color: #333; }")
            .Append("table { border-collapse: collapse; width: 100%; }")
            .Append("td { padding: 6px 8px; vertical-align: top; word-break: break-word; }")
            .Append(".header { background-color: #4CAF50; color: white; font-size: 18px; }")
            .Append(".sectionLabel { background-color: #5b9bd5; font-weight: bold; color: White; }")
            .Append(".itemLabel { background-color: #f9f9f9; font-weight: bold; width: 30%; }")
            .Append(".divider { height: 1px; background-color: #e5e5e5; padding: 0; }")
            .Append("</style>");

        return builder.ToString();
    }

    #endregion Private Methods

    #region Private Classes

    /// <summary>
    /// Represents the configuration settings for sending error report emails.
    /// This class encapsulates all of the necessary information for connecting to an SMTP server and sending an email,
    /// </summary>
    private class EmailConfig
    {
        public string FromAddress { get; set; } = Function.GetAppSetting("ExceptionReport:FromAddress")!;
        public string FromName { get; set; } = Function.GetAppSetting("ExceptionReport:FromName")!;
        public string ToAddress { get; set; } = Function.GetAppSetting("ExceptionReport:ToAddress")!;
        public string CCAddress { get; set; } = Function.GetAppSetting("ExceptionReport:CCAddress")!;
    }
    #endregion Private Classes
}