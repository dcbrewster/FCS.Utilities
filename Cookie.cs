using System;
using Microsoft.AspNetCore.Http;

namespace FCS.Utilities;

/// <summary>
/// Cookie Handling helpers as extension methods for HttpRequest/HttpResponse
/// </summary>
public static class Cookie
{
    /// <summary>
    /// Set a cookie on the provided HttpResponse
    /// </summary>
    public static void Set(this HttpResponse response, string key, string value, int? expireTime = null)
    {
        CookieOptions option = new();

        if(expireTime.HasValue)
            option.Expires = DateTime.Now.AddMinutes(expireTime.Value);

        response.Cookies.Append(key, value, option);
    }

    /// <summary>
    /// Get a cookie value from the provided HttpRequest
    /// </summary>
    public static string? Get(this HttpRequest request, string key) => request.Cookies[key];

    /// <summary>
    /// Delete the cookie from the provided HttpResponse
    /// </summary>
    public static void Remove(this HttpResponse response, string key) => response.Cookies.Delete(key);

    /// <summary>
    /// Check whether a cookie exists on the request
    /// </summary>
    public static bool Exists(this HttpRequest request, string key) => request.Cookies.ContainsKey(key);

}