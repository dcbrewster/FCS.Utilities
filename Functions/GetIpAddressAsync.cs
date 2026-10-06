using System.Net.Http;
using System.Threading.Tasks;

namespace FCS.Utilities;

public static partial class Function
{
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Get The Host Machine's IPAddress
    /// </summary>
    /// <returns>String Containing IP Address</returns>
    public static async Task<string> GetIpAddressAsync() => await _httpClient.GetStringAsync("https://api.ipify.org").ConfigureAwait(false);
}