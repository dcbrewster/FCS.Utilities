using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace FCS.Utilities;

/// <summary>
/// A utility class for fetching and parsing CSV data from a given URL into a list of objects of type T.
/// </summary>
public static class CSV
{
    /// <summary>
    /// The HttpClient instance used for making HTTP requests to fetch CSV data.
    /// </summary>
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Fetches CSV data from the specified URL and parses it into a list of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects to parse the CSV data into.</typeparam>
    /// <param name="season">The season for which to fetch the CSV data.</param>
    /// <param name="url">The URL of the CSV data.</param>
    /// <returns>A list of objects of type T parsed from the CSV data.</returns>
    public static async Task<List<T>> GetCsvDataAsync<T>(string url)
    {
        string? csv = await _httpClient.GetStringAsync(url);    // Fetch CSV data as a string from the specified URL
        using StringReader? reader = new(csv);        // Create a StringReader to read the CSV data

        try
        {
            // Configure CsvHelper to handle CSV parsing with specific settings
            CsvConfiguration? config = new(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,            // Ignore bad data found in the CSV
                MissingFieldFound = null,       // Ignore missing fields in the CSV
                HeaderValidated = null,         // Ignore header validation errors
                TrimOptions = TrimOptions.Trim  // Trim whitespace from fields
            };

            using CsvReader? csvReader = new(reader, config);   // Create a CsvReader to parse the CSV data with the specified configuration

            if(!csvReader.Read()) return [];                    // If file is empty, return empty list

            csvReader.ReadHeader();                                     // Read the header row of the CSV data

            IEnumerable<T>? records = csvReader.GetRecords<T>();        // Parse the CSV data into a collection of objects of type T

            return records.ToList();                                    // Convert the collection to a list and return it
        }
        catch
        {
            // Swallow exceptions to preserve existing behavior; consider logging.
            return [];

        }
    }
}