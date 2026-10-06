using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FCS.Utilities;

#nullable enable

/// <summary>
/// Provides utility methods for working with JSON data, including fetching JSON from URLs and posting JSON content to endpoints.
/// It also includes custom JSON converters for handling specific serialization scenarios.
/// </summary>
public static class JSON
{
    /// <summary>
    /// Get Json String
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    /// <param name="uri">Url</param>
    /// <param name="serializedOptions">Json Serializer Options</param>
    public static async Task<T[]> GetJsonFromContentAsync<T>(string uri, JsonSerializerOptions? serializedOptions = null)
    {
        HttpClient httpClient = new();                                          // Create httpClient Object
        int TryAgain = 2;                                                       // Set Control Flat

        while (TryAgain != 0)
        {
            try
            {
                HttpRequestMessage request = new(HttpMethod.Get, uri);          // Build Request

                // Send Request And Get Response
                using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();                             // Set Flag

                if (response.IsSuccessStatusCode)                                // Check Response Code
                {
                    try
                    {
                        T[]? Result = await response.Content.ReadFromJsonAsync<T[]>(serializedOptions).ConfigureAwait(true);
                        TryAgain = 0;                                           // We're All Good

                        return Result!;
                    }
                    catch (NotSupportedException)
                    {
                        Console.WriteLine("The content type is not supported.");    // When content type is not valid
                    }
                    catch (JsonException ex)
                    {
                        // Invalid JSON
                        Console.WriteLine("Invalid JSON.");
                        Console.WriteLine($"{ex.Message}, Http {response.StatusCode}");

                        string content = string.Empty;

                        try
                        {
                            content = response.Content.ReadAsStringAsync().Result;
                        }
                        catch
                        {
                            throw new InvalidOperationException($"{ex.Message}, Http {response.StatusCode} {response.ReasonPhrase}: {content}", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Check For Invalid Host Error.
                if (ex.Message.Contains("No such host is known."))
                {
                    Console.WriteLine($"Host Name Is Invalid. {ex.Message.Replace("No such host is known.", "")}");
                }
                else
                {
                    Console.WriteLine($"Call To Host Just Screwed Up. Error Message:\n{ex.Message}");
                    TryAgain--;
                }
            }
        }

        return [];
    }

    /// <summary>
    /// Download JSON (string) From Url
    /// </summary>
    /// <param name="uri">URL</param>
    public static async Task<string> GetJsonAsync(string uri)
    {
        HttpClient httpClient = new();             // Create httpClient Object

        return await httpClient.GetStringAsync(uri);        // Return String From Url
    }

    /// <summary>
    /// Post JSON To Uri
    /// </summary>
    /// <param name="uri">Uri of Request</param>
    /// <param name="content">Json String</param>
    public static async Task PostJsonHttpClient(string uri, string content)
    {
        HttpResponseMessage postResponse = await new HttpClient().PostAsJsonAsync(uri, content).ConfigureAwait(false);

        postResponse.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Post JSON To Uri
    /// </summary>
    /// <param name="uri">Uri of Request</param>
    /// <param name="content">Json String</param>
    /// <returns></returns>
    private static async Task PostJsonContent(string uri, string content)
    {
        HttpClient httpClient = new();

        HttpRequestMessage postRequest = new(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(content)
        };

        HttpResponseMessage postResponse = await httpClient.SendAsync(postRequest).ConfigureAwait(false);

        postResponse.EnsureSuccessStatusCode();
    }
}

/// <summary>
/// Converts numeric JSON values to their string representation during serialization while preserving the structure of
/// arrays and objects.
/// </summary>
/// <remarks>This converter only supports write operations. The <see cref="Read"/> method throws <see
/// cref="NotImplementedException"/>. When writing, numeric values are converted to strings, and arrays and objects are
/// processed recursively to convert all nested numeric values.</remarks>
public class NumberAsStringConverter : JsonConverter<JsonElement>
{
    /// <summary>
    /// Reads and converts the JSON to a <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>The converted <see cref="JsonElement"/>.</returns>
    /// <exception cref="NotImplementedException">This method is not implemented.</exception>
    public override JsonElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

    /// <summary>
    /// Writes the JSON element to the output, converting numeric values to string representations.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The JSON element to write.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, JsonElement value, JsonSerializerOptions options)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Number:
                writer.WriteStringValue(value.GetRawText());
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();

                foreach (JsonElement item in value.EnumerateArray())
                {
                    Write(writer, item, options);
                }

                writer.WriteEndArray();
                break;

            case JsonValueKind.Object:
                writer.WriteStartObject();

                foreach (JsonProperty prop in value.EnumerateObject())
                {
                    writer.WritePropertyName(prop.Name);
                    Write(writer, prop.Value, options);
                }

                writer.WriteEndObject();
                break;

            default:
                value.WriteTo(writer);
                break;
        }
    }
}

/// <summary>
/// MetaDataConverter merges the JsonPropertyName from the MetaData class with the
/// corresponding property in the original class name.
/// </summary>
public class MetaDataConverter : JsonConverter<object>
{
    /// <summary>
    /// Combine the properties from the original file with the properties of the meta file.
    /// </summary>
    /// <param name="type"></param>
    /// <returns>Combined Property List</returns>
    private static PropertyInfo[] GetCombinedProperties(Type type)
    {
        MetadataTypeAttribute? metadataAttr = type.GetCustomAttribute<MetadataTypeAttribute>();
        PropertyInfo[] metadataProps = metadataAttr?.MetadataClassType.GetProperties() ?? [];
        PropertyInfo[] originalProps = type.GetProperties();

        return [.. originalProps.Union(metadataProps)];
    }

    /// <summary>
    /// Read And Deserialize Json Data
    /// </summary>
    /// <param name="reader">JsonReader</param>
    /// <param name="typeToConvert">Property Type</param>
    /// <param name="options">Serialization Options</param>
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using JsonDocument doc = JsonDocument.ParseValue(ref reader);           // Open The Json Document
        object result = Activator.CreateInstance(typeToConvert)!;               // Create An Object Of Requested Type
        PropertyInfo[] combinedProps = GetCombinedProperties(typeToConvert);    // Get Combined Properties From POCO And MetaData Models

        foreach (JsonProperty prop in doc.RootElement.EnumerateObject())         // Process Each Property
        {
            foreach (PropertyInfo property in combinedProps)                     // Process Each Combined Property
            {
                JsonPropertyNameAttribute? metaAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>(); // Get Json Property Name From MetaData Model
                string nameToUse = metaAttr?.Name ?? property.Name;                                             // Use metaArray If Available Else Use Property Name

                if (prop.Name.Equals(nameToUse, StringComparison.OrdinalIgnoreCase))							// Did We Find Matching Property
                {
                    PropertyInfo? actualProperty = typeToConvert.GetProperty(property.Name);                    // Get The Property Info

                    object? value = null;                                                                       // Create Object

                    if (actualProperty != null)                                                                 // Do We Have A Property?
                    {
                        try
                        {
                            // Extract Property Value
                            value = JsonSerializer.Deserialize(prop.Value.GetRawText(), actualProperty.PropertyType);
                        }
                        catch (JsonException)
                        {
                            // Have String (ie 12L)
                            double newValue = 0;

                            // Calculate Value - Remove "L"
                            if (prop.Value.ToString().Length > 0) newValue = Convert.ToDouble(prop.Value.ToString()[..^1]);

                            value = JsonSerializer.Deserialize(newValue += 1000, actualProperty.PropertyType);  // Extract Property Value;
                        }

                        actualProperty.SetValue(result, value);                                                 // Set Value To Property

                        break;                                                                                  // Bail
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Writes the specified value as JSON by serializing all combined properties.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (PropertyInfo property in GetCombinedProperties(value.GetType()))
        {
            JsonPropertyNameAttribute? metaAttr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            string? nameToUse = metaAttr?.Name ?? property.Name;
            PropertyInfo? actualProperty = value.GetType().GetProperty(property.Name);

            if (actualProperty != null)
            {
                writer.WritePropertyName(nameToUse);
                JsonSerializer.Serialize(writer, actualProperty.GetValue(value), actualProperty.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Can this object be converted?
    /// </summary>
    /// <param name="typeToConvert">Object Type</param>
    public override bool CanConvert(Type typeToConvert) => typeof(object).IsAssignableFrom(typeToConvert);
}