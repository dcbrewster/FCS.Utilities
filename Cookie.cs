namespace FCS.Utilities;

/// <summary>
/// Cookie Handling
/// </summary>
public static class Cookie
{
    //public static void Get(string key)
    //{
    //    //read cookie from IHttpContextAccessor
    //    string cookieValueFromContext = _httpContextAccessor.HttpContext.Request.Cookies[key];
    //}

    //public static void Get1(string key)
    //{
    //    //read cookie from Request object
    //    string cookieValueFromReq = Request.Cookies[key];
    //}

    ///// <summary>
    ///// Set A Cookie
    ///// </summary>
    ///// <param name="key">Cookie Key</param>
    ///// <param name="value">Value To Store In The Cookie</param>
    ///// <param name="expireTime">Expiration Time</param>
    //public static void Set(string key, string value, int? expireTime)
    //{
    //    CookieOptions option = new();

    //    if(expireTime.HasValue)
    //        option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
    //    else
    //        option.Expires = DateTime.Now.AddMilliseconds(10);

    //    Response.Cookies.Append(key, value, option);
    //}

    ///// <summary>
    ///// Delete the key
    ///// </summary>
    ///// <param name="key">Key Name</param>
    //public static void Remove(string key) => Response.Cookies.Delete(key);

    //public static bool Exists(this HttpRequest request, string key) => request.Cookies.ContainsKey(key);

    /////// <summary>
    /////// Cookie Handling
    /////// </summary>
    ////public static class Cookie
    ////{
    ////    /// <summary>
    ////    /// Set Cookie Value
    ////    /// </summary>
    ////    /// <param name="response">HttpResponse</param>
    ////    /// <param name="key">Cookie Key</param>
    ////    /// <param name="value">Cookie Value</param>
    ////    /// <param name="expireTime">Number Of Seconds Until Expiry - default to Session Cookie</param>
    ////    /// <param name="httpOnly">Cookie is http only - default true</param>
    ////    /// <param name="secure">Cookie is secure - default true</param>
    ////    public static void Set(this HttpResponse response, string key, string value, int? expireTime = null, bool httpOnly = true, bool secure = true)
    ////    {
    ////        CookieOptions cookieOptions = new();                                              // Create Cookie Options Object

    ////        if(expireTime.HasValue)
    ////            cookieOptions.Expires = DateTime.Now.AddSeconds(expireTime.Value);                        // Set Expiry Time

    ////        cookieOptions.Secure = secure;                                                                // Make Cookie Secure
    ////        cookieOptions.HttpOnly = httpOnly;                                                            // Make Cookie HttpOnly
    ////        response.Cookies.Append(key, value, cookieOptions);                                           // Add Cookie To Response
    ////    }

    ////    /// <summary>
    ////    /// Get Cookie Value
    ////    /// </summary>
    ////    /// <param name="request"></param>
    ////    /// <param name="key"></param>
    ////    /// <returns>Value of Cookie</returns>
    ////    public static string Get(this HttpRequest request, string key) => request.Cookies[key];

    ////    /// <summary>
    ////    /// Remove Cookie
    ////    /// </summary>
    ////    /// <param name="response"></param>
    ////    /// <param name="key"></param>
    ////    public static void Remove(this HttpResponse response, string key) => response.Cookies.Delete(key);

    ////    //public static void RemoveAllCookies(this HttpResponse response)
    ////    //{
    ////    //    StringValues cookies = response.Headers["Set-Cookie"];

    ////    //    if (cookies != null)
    ////    //    {
    ////    //        List<string>? cookieList = cookies.ToList();

    ////    //        foreach (string cookie in cookieList)
    ////    //        {
    ////    //            response.Headers.Remove("Set-Cookie");
    ////    //        }
    ////    //    }
    ////    //}
}