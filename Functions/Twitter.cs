using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Send the Twitter Name To Twitter To Validate It's Existence
    /// </summary>
    /// <param name="userName"></param>
    public static Tuple<bool, string> DoesTwitterUserNameExists(string userName)
    {
        // Create HttpClient
        using (HttpClient TwitterApi = new())
        {
            // Add Twitter API Bearer Token To Request Headers
            TwitterApi.DefaultRequestHeaders.Add("Authorization", "Bearer AAAAAAAAAAAAAAAAAAAAAHRTKAEAAAAAwWcud5qFRtLFL%2B%2Fn2wxN%2Bjf5CXo%3DjdUIL7dIysSkQGg2R6VBqjOq9tnCCqFFpjeS3QaUdzgneZGmdn");

            Twitter? results;                                                           // Create Class For Results From Twitter

            try
            {
                Task<HttpResponseMessage> resp = TwitterApi.GetAsync($"https://api.twitter.com/2/users/by/username/{userName}");

                string json = resp.Result.Content.ReadAsStringAsync().Result;           // Read the response content

                results = JsonSerializer.Deserialize<Twitter>(json);                    // De-serialize Results

                if (results != null)                                                    // Check if results is not null before dereferencing
                {
                    // Not Null Means Valid UserName
                    if (results.Data != null) return new Tuple<bool, string>(true, results.Data.Name);

                    // Invalid UserName - check if Errors is not null and has items
                    if (results.Errors?.Count > 0) return new Tuple<bool, string>(false, results.Errors[0].Detail);
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        return new Tuple<bool, string>(false, "Twitter Not Available");                 // Validation Problem
    }

    /// <summary>
    /// Prepend The Twitter URL To The Supplied UserName
    /// </summary>
    /// <param name="userName"></param>
    public static string BuildTwitterLink(string userName) => $"https://twitter.com/{userName.Replace("@", "", StringComparison.OrdinalIgnoreCase)}";
}

/// <summary>
/// Classes For De-Serializing Twitter API Results
/// </summary>
internal class Twitter
{
    /// <summary>
    /// Data Object Returned When Twitter UserName Is Valid
    /// </summary>
    [JsonPropertyName("data")]
    public Data? Data { get; set; }

    /// <summary>
    /// Gets or sets the collection of errors.
    /// </summary>
    [JsonPropertyName("errors")]
    public List<Errors>? Errors { get; set; }
}

/// <summary>
/// Represents data containing an identifier, name, and username.
/// </summary>
internal class Data
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// The name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";
}

/// <summary>
/// Represents structured error information.
/// </summary>
internal class Errors
{
    /// <summary>
    /// Gets or sets the detailed information.
    /// </summary>
    [JsonPropertyName("detail")]
    public string Detail { get; set; } = "";

    /// <summary>
    /// Gets or sets the title of the error.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the type of the resource.
    /// </summary>
    [JsonPropertyName("resource_type")]
    public string ResourceType { get; set; } = "";

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    [JsonPropertyName("parameters")]
    public string Parameters { get; set; } = "";

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    /// <summary>
    /// Gets or sets the type identifier.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
}