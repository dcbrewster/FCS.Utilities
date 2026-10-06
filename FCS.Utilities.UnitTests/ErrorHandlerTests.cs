using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FCS.Utilities.UnitTests;

[TestClass]
public sealed class ErrorHandlerTests
{
    [TestMethod]
    public void BuildErrorReport_RedactsSensitiveValuesAndHtmlEncodesOutput()
    {
        DefaultHttpContext context = new();
        context.TraceIdentifier = "trace-123";
        context.Request.Method = "POST";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.test");
        context.Request.Path = "/orders/save";
        context.Request.QueryString = new QueryString("?token=secret-token&orderId=123");
        context.Request.Headers.Authorization = "Bearer secret-token";
        context.Request.Headers.UserAgent = "<script>agent</script>";

        InvalidOperationException exception = new("Failure <script>alert(1)</script>");
        exception.Data["password"] = "super-secret-password";
        exception.Data["safe"] = "safe-value";

        string report = BuildReport(exception, context);

        StringAssert.Contains(report, "trace-123");
        StringAssert.Contains(report, "Failure &lt;script&gt;alert(1)&lt;/script&gt;");
        StringAssert.Contains(report, "*** REDACTED ***");
        StringAssert.Contains(report, "?orderId=123&amp;token=%2A%2A%2A%20REDACTED%20%2A%2A%2A");
        StringAssert.Contains(report, "safe-value");
        Assert.IsFalse(report.Contains("super-secret-password", StringComparison.Ordinal));
        Assert.IsFalse(report.Contains("Bearer secret-token", StringComparison.Ordinal));
        Assert.IsFalse(report.Contains("secret-token", StringComparison.Ordinal));
    }

    [TestMethod]
    public void BuildErrorReport_WhenExceptionIsAggregate_IncludesFlattenedInnerExceptions()
    {
        AggregateException exception = new(
            "Aggregate failure",
            new InvalidOperationException("First inner"),
            new ArgumentException("Second inner"));

        string report = BuildReport(exception);

        StringAssert.Contains(report, "Aggregate Exception Details");
        StringAssert.Contains(report, "First inner");
        StringAssert.Contains(report, "Second inner");
    }

    [DataTestMethod]
    [DataRow("Mozilla/5.0 compatible Googlebot/2.1", true)]
    [DataRow("ordinary browser", false)]
    [DataRow(null, false)]
    public void IsItABot_ReturnsExpectedResult(string? userAgent, bool expected)
    {
        Assert.AreEqual(expected, ErrorHandler.IsItABot(userAgent));
    }

    private static string BuildReport(Exception exception, HttpContext? context = null)
    {
        MethodInfo method = typeof(ErrorHandler).GetMethod("BuildErrorReport", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(nameof(ErrorHandler), "BuildErrorReport");

        object?[] parameters = [exception, "Test Application", context, null];

        return (string)method.Invoke(new ErrorHandler(), parameters)!;
    }
}