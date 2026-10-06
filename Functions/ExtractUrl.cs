using System;
using System.Collections.Generic;

namespace FCS.Utilities;

public static partial class Function
{
    private static readonly string[] ImageFilters = ["jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg", "webp"];

    /// <summary>
    /// Attempt To Extract links from Html code.
    /// Options (based on UrlType):
    /// All - Return all 'src' and 'href' links
    /// SrcOnly - Return only 'src' links
    /// HrefOnly - Return only 'href' links
    /// ImagesOnly - Return only 'images' links
    /// <para>
    /// ImagesOnly uses the same logic as SrcOnly but then filters the results by checking for matches with the ImageFilters array.
    /// </para>
    /// </summary>
    /// <param name="html">HTML string to parse</param>
    /// <param name="urlType">Type of URL to extract</param>
    public static string ExtractUrl(this string html, UrlType urlType = UrlType.All)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;       // Return Empty String if Input is Null or WhiteSpace

        int srcStart = 0;                                               // Starting Position of 'src' Link
        int hrefStart = 0;                                              // Starting Position of 'href' Link
        bool imagesOnly = false;                                        // Flag to Indicate if Only Image Links Should be Returned

        if (urlType == UrlType.ImagesOnly)                              // If UrlType is ImagesOnly, Set UrlType to SrcOnly to Extract 'src' Links, Then Filter for Images
        {
            urlType = UrlType.SrcOnly;                                  // Set UrlType to SrcOnly to Extract 'src' Links, Then Filter for Images
            imagesOnly = true;                                          // Set Flag to Indicate that Only Image Links Should be Returned
        }

        // Find "src" Starting Position
        if (urlType is UrlType.All or UrlType.SrcOnly) srcStart = html.IndexOf("src=", StringComparison.OrdinalIgnoreCase);

        // Find "href" Starting Position
        if (urlType is UrlType.All or UrlType.HrefOnly) hrefStart = html.IndexOf("href=", StringComparison.OrdinalIgnoreCase);

        // Process Which Ever Link Was Found First
        if (urlType == UrlType.All)             // If UrlType is All, Process Which Ever Link Was Found First
        {
            if (srcStart < 0)                   // If "src" Link Was Not Found, Set srcStart to hrefStart + 1 to Ensure That hrefStart is Processed First
                srcStart = hrefStart + 1;       // If "href" Link Was Not Found, Set hrefStart to srcStart + 1 to Ensure That srcStart is Processed First
            else if (hrefStart < 0)             // If "href" Link Was Not Found, Set
                hrefStart = srcStart + 1;       // If Both "src" and "href" Links Were Not Found, Return Empty String
        }

        // Look for 'src'
        if ((srcStart > 0 && urlType == UrlType.SrcOnly) || (hrefStart > 0 && srcStart < hrefStart && urlType == UrlType.All))
        {
            srcStart += 5;                                  // Move Starting Position to the Beginning of the URL (After 'src=')
            int posEnd = html.IndexOf('"', srcStart);       // Find the Ending Position of the URL (Next Double Quote After Starting Position)

            if (posEnd < 0) return string.Empty;            // If Ending Double Quote Was Not Found, Return Empty String

            string url = html[srcStart..posEnd];            // Extract the URL from the HTML String

            if (!imagesOnly) return url;                    // If Not Filtering for Images, Return the URL

            foreach (string filter in ImageFilters)         // Loop Through Image Filters to Check if the URL Contains Any of the Image Extensions
            {
                // If the URL Contains an Image Extension, Return the URL
                if (url.Contains($".{filter}", StringComparison.OrdinalIgnoreCase)) return url;
            }
        }

        // Look for 'href'
        if ((hrefStart > 0 && urlType == UrlType.HrefOnly) || (srcStart > 0 && hrefStart < srcStart && urlType == UrlType.All))
        {
            hrefStart += 6;                                 // Move Starting Position to the Beginning of the URL (After 'href=')

            int posEnd = html.IndexOf('"', hrefStart);      // Find the Ending Position of the URL (Next Double Quote After Starting Position)

            if (posEnd < 0) return string.Empty;            // If Ending Double Quote Was Not Found, Return Empty String

            return html[hrefStart..posEnd];                 // Extract and Return the URL from the HTML String
        }

        return string.Empty;                                // If No Valid URL Was Found, Return Empty String
    }

    /// <summary>
    /// Extract all the urls found within the supplied string
    /// Options (based on UrlType):
    /// All - Return all 'src' and 'href' links
    /// SrcOnly - Return only 'src' links
    /// HrefOnly - Return only 'href' links
    /// ImagesOnly - Return only 'images' links
    /// <para>
    /// ImagesOnly uses the same logic as SrcOnly but then filters the results by checking for matches with the ImageFilters array.
    /// </para>
    /// </summary>
    /// <param name="html">HTML string to parse</param>
    /// <param name="urlType">Type of URL to extract</param>
    public static List<string> ExtractUrls(this string html, UrlType urlType = UrlType.All)
    {
        List<string> urls = [];                                 // List to Store Extracted URLs

        if (string.IsNullOrWhiteSpace(html)) return urls;       // Return Empty List if Input is Null or WhiteSpace

        do
        {
            string url = ExtractUrl(html, urlType);             // Extract a URL from the HTML String Based on the Specified UrlType

            if (!string.IsNullOrEmpty(url))                     // If a Valid URL Was Extracted, Add it to the List of URLs
            {
                urls.Add(url);                                  // Find the Starting Position of the Extracted URL in the HTML String and Move the Starting Position to the End of the Extracted URL to Continue Searching for More URLs

                // Find the Starting Position of the Extracted URL in the HTML String and Move the Starting Position to the End of the Extracted URL to Continue Searching for More URLs
                int newStart = html.IndexOf(url, StringComparison.Ordinal) + url.Length;

                html = html[newStart..];                        // Update the HTML String to Start from the End of the Extracted URL for the Next Iteration
            }
            else
            {
                html = string.Empty;                            // If No Valid URL Was Extracted, Set HTML to Empty String to Exit the Loop
            }
        } while (html != string.Empty);                         // Continue Looping Until No More Valid URLs Can Be Extracted (When HTML Becomes Empty String)

        return urls;
    }
}

/// <summary>
/// UrlType Enum
/// </summary>
public enum UrlType
{
    /// <summary>
    /// Return all 'src' and 'href' links
    /// </summary>
    All = 0,

    /// <summary>
    /// Return only 'src' links
    /// </summary>
    SrcOnly = 1,

    /// <summary>
    /// Includes only hyperlink references.
    /// </summary>
    HrefOnly = 2,

    /// <summary>
    /// Return only 'images' links. Uses the same logic as SrcOnly but then filters the results by checking for matches with the ImageFilters array.
    /// </summary>
    ImagesOnly = 3
}