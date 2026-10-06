using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FCS.Utilities;

/// <summary>
/// Provides utility functions for working with country data.
/// </summary>
public static partial class Function
{
    #region Public Methods
    /// <summary>
    /// Return List of SelectListItem containing Country Names
    /// </summary>
    /// <param name="country">Country to be marked "Selected"</param>
    public static List<SelectListItem> CountryDropDownList(string country = "")
    {
        List<SelectListItem> CountryList = [];                      // List To Hold Countries

        country ??= "";                                             // Fix If Null
        country = country.ToUpper(CultureInfo.CurrentCulture);      // Change Country To Upper Case

        foreach(Country p in Countries)                             // Process Each Country
        {
            CountryList.Add(new SelectListItem()                    // Add SelectListItem
            {
                Value = p.ID,                                       // Set Value
                Text = p.Name,                                      // Set Text

                // Mark Selected If Required
                Selected = string.Equals(p.ID.ToUpper(CultureInfo.CurrentCulture),
                    country,
                    StringComparison.Ordinal)
                    || string.Equals(p.Name.ToUpper(CultureInfo.CurrentCulture), country, StringComparison.Ordinal)
            });
        }

        return CountryList;                                         // Return List of SelectListItem
    }

    /// <summary>
    /// Return List of County Names
    /// </summary>
    public static List<string> CountryList() => Countries.ConvertAll(c => c.Name);

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// List Of Countries
    /// </summary>
    private static readonly List<Country> Countries =
    [
        new Country { ID = "CA", Name = "Canada" },
        new Country { ID = "US", Name = "United States" },
        new Country { ID = "AF", Name = "Afghanistan" },
        new Country { ID = "AX", Name = "Åland" },
        new Country { ID = "AL", Name = "Albania" },
        new Country { ID = "DZ", Name = "Algeria" },
        new Country { ID = "AS", Name = "American Samoa" },
        new Country { ID = "AD", Name = "Andorra" },
        new Country { ID = "AO", Name = "Angola" },
        new Country { ID = "AI", Name = "Anguilla" },
        new Country { ID = "AG", Name = "Antigua and Barbuda" },
        new Country { ID = "AR", Name = "Argentina" },
        new Country { ID = "AM", Name = "Armenia" },
        new Country { ID = "AW", Name = "Aruba" },
        new Country { ID = "AU", Name = "Australia" },
        new Country { ID = "AT", Name = "Austria" },
        new Country { ID = "AZ", Name = "Azerbaijan" },
        new Country { ID = "BS", Name = "Bahamas" },
        new Country { ID = "BH", Name = "Bahrain" },
        new Country { ID = "BD", Name = "Bangladesh" },
        new Country { ID = "BB", Name = "Barbados" },
        new Country { ID = "BY", Name = "Belarus" },
        new Country { ID = "BE", Name = "Belgium" },
        new Country { ID = "BZ", Name = "Belize" },
        new Country { ID = "BJ", Name = "Benin" },
        new Country { ID = "BM", Name = "Bermuda" },
        new Country { ID = "BT", Name = "Bhutan" },
        new Country { ID = "BO", Name = "Bolivia" },
        new Country { ID = "BQ", Name = "Bonaire, Sint Eustatius and Saba" },
        new Country { ID = "BA", Name = "Bosnia and Herzegovina" },
        new Country { ID = "BW", Name = "Botswana" },
        new Country { ID = "BR", Name = "Brazil" },
        new Country { ID = "AQ", Name = "British Antarctic Territory" },
        new Country { ID = "IO", Name = "British Indian Ocean Territory" },
        new Country { ID = "VG", Name = "British Virgin Islands" },
        new Country { ID = "BN", Name = "Brunei" },
        new Country { ID = "BG", Name = "Bulgaria" },
        new Country { ID = "BF", Name = "Burkina Faso" },
        new Country { ID = "BI", Name = "Burundi" },
        new Country { ID = "KH", Name = "Cambodia" },
        new Country { ID = "CM", Name = "Cameroon" },
        new Country { ID = "CV", Name = "Cape Verde" },
        new Country { ID = "KY", Name = "Cayman Islands" },
        new Country { ID = "CF", Name = "Central African Republic" },
        new Country { ID = "TD", Name = "Chad" },
        new Country { ID = "CL", Name = "Chile" },
        new Country { ID = "CN", Name = "China" },
        new Country { ID = "CX", Name = "Christmas Island" },
        new Country { ID = "CC", Name = "Cocos (Keeling) Island" },
        new Country { ID = "CO", Name = "Colombia" },
        new Country { ID = "KM", Name = "Comoros" },
        new Country { ID = "CG", Name = "Congo (Brazzaville)" },
        new Country { ID = "CD", Name = "Congo, Democratic Republic" },
        new Country { ID = "CK", Name = "Cook Islands" },
        new Country { ID = "CR", Name = "Costa Rica" },
        new Country { ID = "CI", Name = "Côte d'Ivoire (Ivory Coast)" },
        new Country { ID = "HR", Name = "Croatia" },
        new Country { ID = "CU", Name = "Cuba" },
        new Country { ID = "CW", Name = "Curaçao" },
        new Country { ID = "CY", Name = "Cyprus" },
        new Country { ID = "CZ", Name = "Czech Republic" },
        new Country { ID = "DK", Name = "Denmark" },
        new Country { ID = "DJ", Name = "Djibouti" },
        new Country { ID = "DM", Name = "Dominica" },
        new Country { ID = "DO", Name = "Dominican Republic" },
        new Country { ID = "TL", Name = "East Timor" },
        new Country { ID = "EC", Name = "Ecuador" },
        new Country { ID = "SV", Name = "El Salvador" },
        new Country { ID = "EG", Name = "Egypt" },
        new Country { ID = "GQ", Name = "Equatorial Guinea" },
        new Country { ID = "ER", Name = "Eritrea" },
        new Country { ID = "EE", Name = "Estonia" },
        new Country { ID = "SZ", Name = "Eswatini" },
        new Country { ID = "ET", Name = "Ethiopia" },
        new Country { ID = "FK", Name = "Falkland Islands" },
        new Country { ID = "FO", Name = "Faroe Islands" },
        new Country { ID = "FJ", Name = "Fiji" },
        new Country { ID = "FI", Name = "Finland" },
        new Country { ID = "FR", Name = "France" },
        new Country { ID = "GF", Name = "French Guiana" },
        new Country { ID = "PF", Name = "French Polynesia" },
        new Country { ID = "TF", Name = "French Southern and Antarctic Territories" },
        new Country { ID = "GA", Name = "Gabon" },
        new Country { ID = "GM", Name = "Gambia" },
        new Country { ID = "GE", Name = "Georgia" },
        new Country { ID = "DE", Name = "Germany" },
        new Country { ID = "GH", Name = "Ghana" },
        new Country { ID = "GI", Name = "Gibraltar" },
        new Country { ID = "GR", Name = "Greece" },
        new Country { ID = "GL", Name = "Greenland" },
        new Country { ID = "GD", Name = "Grenada" },
        new Country { ID = "GP", Name = "Guadeloupe" },
        new Country { ID = "GU", Name = "Guam" },
        new Country { ID = "GT", Name = "Guatemala" },
        new Country { ID = "GG", Name = "Guernsey" },
        new Country { ID = "GN", Name = "Guinea" },
        new Country { ID = "GW", Name = "Guinea Bissau" },
        new Country { ID = "GY", Name = "Guyana" },
        new Country { ID = "HT", Name = "Haiti" },
        new Country { ID = "HM", Name = "Heard and McDonald Islands" },
        new Country { ID = "HN", Name = "Honduras" },
        new Country { ID = "HK", Name = "Hong Kong" },
        new Country { ID = "HU", Name = "Hungary" },
        new Country { ID = "IS", Name = "Iceland" },
        new Country { ID = "IN", Name = "India" },
        new Country { ID = "ID", Name = "Indonesia" },
        new Country { ID = "IR", Name = "Iran" },
        new Country { ID = "IQ", Name = "Iraq" },
        new Country { ID = "IE", Name = "Ireland" },
        new Country { ID = "IM", Name = "Isle of Man" },
        new Country { ID = "IL", Name = "Israel" },
        new Country { ID = "IT", Name = "Italy" },
        new Country { ID = "JM", Name = "Jamaica" },
        new Country { ID = "JP", Name = "Japan" },
        new Country { ID = "JE", Name = "Jersey" },
        new Country { ID = "JO", Name = "Jordan" },
        new Country { ID = "KZ", Name = "Kazakhstan" },
        new Country { ID = "KE", Name = "Kenya" },
        new Country { ID = "KI", Name = "Kiribati" },
        new Country { ID = "KP", Name = "Korea, North" },
        new Country { ID = "KR", Name = "Korea, South" },
        new Country { ID = "XK", Name = "Kosovo" },
        new Country { ID = "KW", Name = "Kuwait" },
        new Country { ID = "KG", Name = "Kyrgyzstan" },
        new Country { ID = "LA", Name = "Laos" },
        new Country { ID = "LV", Name = "Latvia" },
        new Country { ID = "LB", Name = "Lebanon" },
        new Country { ID = "LS", Name = "Lesotho" },
        new Country { ID = "LR", Name = "Liberia" },
        new Country { ID = "LY", Name = "Libya" },
        new Country { ID = "LI", Name = "Liechtenstein" },
        new Country { ID = "LT", Name = "Lithuania" },
        new Country { ID = "LU", Name = "Luxembourg" },
        new Country { ID = "MO", Name = "Macau" },
        new Country { ID = "MG", Name = "Madagascar" },
        new Country { ID = "MW", Name = "Malawi" },
        new Country { ID = "MY", Name = "Malaysia" },
        new Country { ID = "MV", Name = "Maldives" },
        new Country { ID = "ML", Name = "Mali" },
        new Country { ID = "MT", Name = "Malta" },
        new Country { ID = "MH", Name = "Marshall Islands" },
        new Country { ID = "MR", Name = "Mauritania" },
        new Country { ID = "MU", Name = "Mauritius" },
        new Country { ID = "MQ", Name = "Martinique" },
        new Country { ID = "YT", Name = "Mayotte" },
        new Country { ID = "MX", Name = "Mexico" },
        new Country { ID = "FM", Name = "Micronesia" },
        new Country { ID = "MD", Name = "Moldova" },
        new Country { ID = "MC", Name = "Monaco" },
        new Country { ID = "MN", Name = "Mongolia" },
        new Country { ID = "ME", Name = "Montenegro" },
        new Country { ID = "MS", Name = "Montserrat" },
        new Country { ID = "MA", Name = "Morocco" },
        new Country { ID = "MZ", Name = "Mozambique" },
        new Country { ID = "MM", Name = "Myanmar" },
        new Country { ID = "NA", Name = "Namibia" },
        new Country { ID = "NR", Name = "Nauru" },
        new Country { ID = "NP", Name = "Nepal" },
        new Country { ID = "NL", Name = "Netherlands" },
        new Country { ID = "NC", Name = "New Caledonia" },
        new Country { ID = "NZ", Name = "New Zealand" },
        new Country { ID = "NI", Name = "Nicaragua" },
        new Country { ID = "NE", Name = "Niger" },
        new Country { ID = "NG", Name = "Nigeria" },
        new Country { ID = "NU", Name = "Niue" },
        new Country { ID = "NF", Name = "Norfolk Island" },
        new Country { ID = "MK", Name = "North Macedonia" },
        new Country { ID = "MP", Name = "Northern Mariana Islands" },
        new Country { ID = "NO", Name = "Norway" },
        new Country { ID = "OM", Name = "Oman" },
        new Country { ID = "PK", Name = "Pakistan" },
        new Country { ID = "PW", Name = "Palau" },
        new Country { ID = "PS", Name = "Palestine" },
        new Country { ID = "PA", Name = "Panama" },
        new Country { ID = "PG", Name = "Papua New Guinea" },
        new Country { ID = "PY", Name = "Paraguay" },
        new Country { ID = "PE", Name = "Peru" },
        new Country { ID = "PH", Name = "Philippines" },
        new Country { ID = "PN", Name = "Pitcairn Islands" },
        new Country { ID = "PL", Name = "Poland" },
        new Country { ID = "PT", Name = "Portugal" },
        new Country { ID = "PR", Name = "Puerto Rico" },
        new Country { ID = "QA", Name = "Qatar" },
        new Country { ID = "RE", Name = "Réunion" },
        new Country { ID = "RO", Name = "Romania" },
        new Country { ID = "RW", Name = "Rwanda" },
        new Country { ID = "RU", Name = "Russia" },
        new Country { ID = "BL", Name = "Saint Barthélemy" },
        new Country { ID = "SH", Name = "Saint Helena, Ascension and Tristan da Cunha" },
        new Country { ID = "KN", Name = "Saint Kitts and Nevis" },
        new Country { ID = "LC", Name = "Saint Lucia" },
        new Country { ID = "MF", Name = "Saint Martin" },
        new Country { ID = "PM", Name = "Saint Pierre and Miquelon" },
        new Country { ID = "VC", Name = "Saint Vincent and the Grenadines" },
        new Country { ID = "WS", Name = "Samoa" },
        new Country { ID = "SM", Name = "San Marino" },
        new Country { ID = "ST", Name = "São Tomé and Príncipe" },
        new Country { ID = "SA", Name = "Saudi Arabia" },
        new Country { ID = "SN", Name = "Senegal" },
        new Country { ID = "RS", Name = "Serbia" },
        new Country { ID = "SC", Name = "Seychelles" },
        new Country { ID = "SL", Name = "Sierra Leone" },
        new Country { ID = "SX", Name = "Sint Maarten" },
        new Country { ID = "SG", Name = "Singapore" },
        new Country { ID = "SK", Name = "Slovakia" },
        new Country { ID = "SB", Name = "Solomon Islands" },
        new Country { ID = "SO", Name = "Somalia" },
        new Country { ID = "ZA", Name = "South Africa" },
        new Country { ID = "SS", Name = "South Sudan" },
        new Country { ID = "GS", Name = "South Georgia and the South Sandwich Islands" },
        new Country { ID = "ES", Name = "Spain" },
        new Country { ID = "LK", Name = "Sri Lanka" },
        new Country { ID = "SD", Name = "Sudan" },
        new Country { ID = "SR", Name = "Suriname" },
        new Country { ID = "SE", Name = "Sweden" },
        new Country { ID = "CH", Name = "Switzerland" },
        new Country { ID = "SJ", Name = "Svalbard and Jan Mayen" },
        new Country { ID = "SY", Name = "Syria" },
        new Country { ID = "TW", Name = "Taiwan" },
        new Country { ID = "TJ", Name = "Tajikistan" },
        new Country { ID = "TZ", Name = "Tanzania" },
        new Country { ID = "TH", Name = "Thailand" },
        new Country { ID = "TG", Name = "Togo" },
        new Country { ID = "TK", Name = "Tokelau" },
        new Country { ID = "TO", Name = "Tonga" },
        new Country { ID = "TT", Name = "Trinidad and Tobago" },
        new Country { ID = "TN", Name = "Tunisia" },
        new Country { ID = "TR", Name = "Turkey" },
        new Country { ID = "TM", Name = "Turkmenistan" },
        new Country { ID = "TC", Name = "Turks and Caicos Islands" },
        new Country { ID = "TV", Name = "Tuvalu" },
        new Country { ID = "UG", Name = "Uganda" },
        new Country { ID = "UA", Name = "Ukraine" },
        new Country { ID = "AE", Name = "United Arab Emirates" },
        new Country { ID = "GB", Name = "United Kingdom" },
        new Country { ID = "US", Name = "United States" },
        new Country { ID = "UM", Name = "United States Minor Outlying Islands" },
        new Country { ID = "UY", Name = "Uruguay" },
        new Country { ID = "VI", Name = "U.S. Virgin Islands" },
        new Country { ID = "UZ", Name = "Uzbekistan" },
        new Country { ID = "VU", Name = "Vanuatu" },
        new Country { ID = "VA", Name = "Vatican" },
        new Country { ID = "VE", Name = "Venezuela" },
        new Country { ID = "VN", Name = "Vietnam" },
        new Country { ID = "WF", Name = "Wallis and Futuna" },
        new Country { ID = "YE", Name = "Yemen" },
        new Country { ID = "ZM", Name = "Zambia" },
        new Country { ID = "ZW", Name = "Zimbabwe" }
    ];
}

#endregion Private Methods

#region Sub Classes

/// <summary>
/// Represents a country with an ID and a name.
/// </summary>
public class Country
{
    /// <summary>
    /// Gets or sets the country ID.
    /// </summary>
    public required string ID { get; set; }
    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    public required string Name { get; set; }
}

#endregion Sub Classes