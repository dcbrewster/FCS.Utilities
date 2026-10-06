using System;

namespace FCS.Utilities;

/// <summary>
/// GeoLocation class
/// </summary>
public static class GEO
{
    /// <summary>
    /// Calculate the distance between two lat/long points on the earth.
    /// </summary>
    public static double Distance(double lat1, double lat2, double lon1, double lon2, bool Miles = true)
    {
        lon1 = ToRadians(lon1);                         // Convert to radians
        lon2 = ToRadians(lon2);                         // Convert to radians
        lat1 = ToRadians(lat1);                         // Convert to radians
        lat2 = ToRadians(lat2);                         // Convert to radians

        // Haversine formula
        double dlon = lon2 - lon1;                      // Difference in longitude
        double dlat = lat2 - lat1;                      // Difference in latitude
        double a = Math.Pow(Math.Sin(dlat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
            * Math.Pow(Math.Sin(dlon / 2), 2);

        double c = 2 * Math.Asin(Math.Sqrt(a));

        return c * (Miles ? 3956 : 6371);                 // Return the result. Radius of earth in 6,371 kilometers or 3,956 miles.
    }

    /// <summary>
    /// Convert from Degrees to Radians
    /// </summary>
    /// <param name="angleIn10thofaDegree"></param>
    private static double ToRadians(double angleIn10thofaDegree) => (angleIn10thofaDegree * Math.PI) / 180;
}