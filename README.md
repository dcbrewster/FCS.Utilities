# FCS.Utilities — Developer Reference

A collection of helper classes and extension methods used across Forest City Software projects: string clean-up, type conversion, dates, email, error reporting, database access, data mapping, Excel export, QR codes, drop-down list data, and more.

This guide is written for **junior developers**. Every public class, property and method is listed with what it does, what it returns, what can go wrong, and a short example you can copy.

| | |
|---|---|
| **Version** | 1.0.3 |
| **Target framework** | `net10.0-windows` (Windows only — uses `System.Drawing` and the Windows Task Scheduler) |
| **Nullable reference types** | Enabled |
| **Author** | Dave Brewster / Forest City Software |

> Example outputs in this document were checked by running the code with the `en-US` culture. Results that depend on culture (dates, currency) may look different on your machine.

---

## Table of Contents

1. [Getting Started](#1-getting-started)
2. [Concepts You Need First](#2-concepts-you-need-first)
3. [Configuration (appsettings.json)](#3-configuration-appsettingsjson)
4. [String Formatting & Validation — `Function` extensions](#4-string-formatting--validation--function-extensions)
5. [StringUtilities](#5-stringutilities)
6. [TypeCasting](#6-typecasting)
7. [DateHelper](#7-datehelper)
8. [Money Helpers — rounding and change](#8-money-helpers--rounding-and-change)
9. [Random Values](#9-random-values)
10. [Drop-Down List Data](#10-drop-down-list-data)
11. [Configuration & Environment Helpers](#11-configuration--environment-helpers)
12. [Reflection Helpers](#12-reflection-helpers)
13. [Passwords — `IEncryptor`](#13-passwords--iencryptor)
14. [EmailHelper](#14-emailhelper)
15. [ErrorHandler](#15-errorhandler)
16. [Exception Middleware](#16-exception-middleware)
17. [DBHelper](#17-dbhelper)
18. [DataHelpers](#18-datahelpers)
19. [Data Mapping (`DataNamesMapper`)](#19-data-mapping-datanamesmapper)
20. [CSV](#20-csv)
21. [JSON](#21-json)
22. [Excel Export — `CreateExcelFile`](#22-excel-export--createexcelfile)
23. [QRCode](#23-qrcode)
24. [TaskScheduler](#24-taskscheduler)
25. [Cookie](#25-cookie)
26. [GEO](#26-geo)
27. [AppInfo](#27-appinfo)
28. [ProcessControl](#28-processcontrol)
29. [Twitter Helpers](#29-twitter-helpers)
30. [Models](#30-models)
31. [Files Not Compiled](#31-files-not-compiled)
32. [Known Issues & Gotchas](#32-known-issues--gotchas)
33. [Dependencies](#33-dependencies)

---

## 1. Getting Started

### Add the library to your project

Either reference the NuGet package produced by the build (`FCS.Utilities.1.0.3.nupkg`), or add a project reference:

```xml
<ItemGroup>
  <ProjectReference Include="..\FCS.Utilities\FCS.Utilities.csproj" />
</ItemGroup>
```

Your project must also target Windows, e.g. `net10.0-windows`.

### Namespaces

| Namespace | What lives there |
|---|---|
| `FCS.Utilities` | Almost everything: `Function`, `StringUtilities`, `TypeCasting`, `DateHelper`, `EmailHelper`, `ErrorHandler`, `DBHelper`, `DataHelpers`, `CSV`, `JSON`, `QRCode`, `TaskScheduler`, `Cookie`, `GEO`, `AppInfo`, `ProcessControl`, `SelectListItem` |
| `FCS.Utilities.DataMapping` | `DataNamesMapper<T>`, `AttributeHelper`, `PropertyMapHelper` |
| `FCS.Utilities.DataMapping.Attributes` | `DataNamesAttribute` |
| `FCS.Utilities.Middleware` | `ExceptionHandlerMiddleware`, `UseGlobalExceptionHandler()` |
| `FCS.Utilities.Interfaces` | `IEncryptor` |
| `FCS.Utilities.Models` | `Territory` |
| `ExportToExcel` | `CreateExcelFile` (note: **not** under `FCS.Utilities`) |

A typical file starts with:

```csharp
using FCS.Utilities;
using static FCS.Utilities.Function;   // lets you call Function methods without the "Function." prefix
```

---

## 2. Concepts You Need First

### Extension methods

Many methods are **extension methods**. They are declared `static` but have `this` in front of the first parameter, which lets you call them as if they belonged to the type:

```csharp
// Declared as: public static string? FormatPhoneNumber(this string phone, int option = 0)

string? a = "5195551234".FormatPhoneNumber();          // extension-method style
string? b = Function.FormatPhoneNumber("5195551234");  // normal static-method style — same result
```

In the tables below, extension methods are marked **(ext)**.

### Static classes

`Function`, `StringUtilities`, `TypeCasting`, `DateHelper`, `DBHelper`, etc. are `static` classes — you never write `new StringUtilities()`. Call the method on the class name: `StringUtilities.ReverseString("abc")`.

`EmailHelper`, `ErrorHandler`, `QRCode` and `DataNamesMapper<T>` are normal classes — create an instance with `new`.

### `Tuple<bool, string>` results

Some methods (e.g. `EmailHelper.Send()`) return a `Tuple<bool, string>` instead of throwing:

- `Item1` — `true` if it worked, `false` if it failed
- `Item2` — a message explaining what happened

```csharp
Tuple<bool, string> result = email.Send();
if (!result.Item1) Console.WriteLine($"Failed: {result.Item2}");
```

### "Safe" methods return defaults

Many conversion methods **never throw**. When the input is bad they return a default (`0`, `""`, `false`, `null`, `DateTime.MinValue`, `Guid.Empty`). That is convenient, but it also means a bad value can silently turn into `0`. Check the "Returns" notes for each method.

---

## 3. Configuration (appsettings.json)

Several classes read settings using [`Function.GetAppSetting`](#getappsetting). Settings come from, in order of priority (last wins):

1. `appsettings.json` in the application's base directory
2. `appsettings.{Environment}.json` (environment taken from `ASPNETCORE_ENVIRONMENT`, then `DOTNET_ENVIRONMENT`, defaulting to `Production`)
3. Environment variables (use `__` instead of `:`, e.g. `Email__SmtpHost`)

All keys used by the library:

```json
{
  "ConnectionStrings": {
    "Default": "Server=.;Database=MyDb;Integrated Security=true;TrustServerCertificate=true"
  },
  "Email": {
    "SendEnabled": "true",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": "587",
    "SmtpUserId": "mailer@example.com",
    "SmtpPassword": "<from user-secrets or environment>",
    "SmtpPickupDirectory": "C:\\Temp\\Mail",
    "Override": ""
  },
  "ExceptionReport": {
    "FromAddress": "errors@example.com",
    "FromName": "My App Errors",
    "ToAddress": "dev-team@example.com",
    "CCAddress": "lead@example.com,qa@example.com"
  }
}
```

| Key | Used by | Meaning |
|---|---|---|
| `ConnectionStrings:<name>` | `DBHelper`, `Function.GetConnectionString` | SQL Server connection strings. `Default` is used when no name is given. |
| `Email:SendEnabled` | `EmailHelper.Send` | `false` = don't send; write a `.eml` file to disk instead. Default `true`. |
| `Email:SmtpHost` | `EmailHelper` | Used when `EmailHelper.SmtpHost` is blank. |
| `Email:SmtpPort` | `EmailHelper` | Used only when `EmailHelper.SmtpPort` is blank (it defaults to `"25"`, so normally ignored). |
| `Email:SmtpUserId` / `Email:SmtpPassword` | `EmailHelper` | Read only when the port is `587` and the property is blank. |
| `Email:SmtpPickupDirectory` | `EmailHelper` | Folder for `.eml` files when `SendEnabled` is `false`. |
| `Email:Override` | `EmailHelper` | If set to a valid address, **all** mail is redirected to it (great for test environments). |
| `ExceptionReport:*` | `ErrorHandler` | Sender and recipients of error-report emails. `CCAddress` may be comma-separated. |

> **Never commit real passwords.** Use [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) in development and environment variables in production.

---

## 4. String Formatting & Validation — `Function` extensions

`Function` is a large `static partial class` split over many files in the `Functions\` folder. This section covers its string helpers.

### Summary

| Member | Returns | Purpose |
|---|---|---|
| `FormatName(this string?)` (ext) | `string?` | Capitalise a person's name properly |
| `ProperCase(this string?, string cultureName = "en-CA")` (ext) | `string` | Title Case using a culture |
| `FormatPhoneNumber(this string, int option = 0)` (ext) | `string?` | Format North American phone numbers |
| `StripPhoneNumber(this string?)` (ext) | `string?` | Phone digits only (last 10) |
| `FormatCanadianPostalCode(this string)` (ext) | `string` | `"A9A 9A9"` format |
| `FormatCanadianPostalCodeShort(this string)` (ext) | `string` | `"A9A9A9"` format |
| `FormatCanadianOrUsPostalCode(this string)` (ext) | `string` | Canadian or US code, no spaces |
| `ValidateCanadianPostalCode(this string)` (ext) | `PostalCodeValidationResult` | Format and validate |
| `PostalAddress(...)` | `string` | Build a mailing address block |
| `FormatTextForHtml(this string?)` (ext) | `string` | Line breaks → `<br />` |
| `RemoveHtml(this string?)` (ext) | `string?` | Strip tags, collapse whitespace |
| `RemoveControlChars(this string?)` (ext) | `string?` | Remove non-printable characters (keeps CR/LF) |
| `RemoveLastCharacter(this string?)` (ext) | `string?` | Drop the final character |
| `ExtractSection(string, string)` | `string` | Pull a marked section out of a template |
| `ExtractUrl(this string, UrlType)` (ext) | `string` | First `src`/`href` URL in HTML |
| `ExtractUrls(this string, UrlType)` (ext) | `List<string>` | All `src`/`href` URLs in HTML |
| `TextToBase64(this string)` (ext) | `string` | Text → Base64 |
| `Base64ToText(this string)` (ext) | `string` | Base64 → text |
| `ReadImageToBytes(this string)` (ext) | `byte[]` | Read a file's bytes |
| `Base64ToImage(this byte[])` (ext) | `Image` | Bytes → `System.Drawing.Image` |
| `CurrencyStringToValue(this string)` (ext) | `decimal` | `"$1,234.56"` → `1234.56` |
| `IsValidEmailAddress(this string?)` (ext) | `bool` | RFC 5322 email check |
| `IsValidFileName(this string?)` (ext) | `bool` | Safe file name check |
| `IsValidPath(this string?)` (ext) | `bool` | Path syntax check |
| `IsValidPathWithExistingRoot(this string?)` (ext) | `bool` | Path syntax + drive exists |

---

### `FormatName`

```csharp
public static string? FormatName(this string? name)
```

Capitalises each word of a name. Knows about `Mc`, `Mac`, `O'`, `'s`, and Roman numerals `II`/`III`. Punctuation at the start/end of each word (like the `.` in `jr.`) is removed.

| Parameter | Description |
|---|---|
| `name` | The name to format. |

**Returns:** the formatted name, or `null` if `name` is `null`, empty or whitespace.

```csharp
"dave's".FormatName();            // "Dave's"
"mcfly".FormatName();             // "McFly"
"macfly".FormatName();            // "MacFly"
"o'dell".FormatName();            // "O'Dell"
"mac jones ii".FormatName();      // "Mac Jones II"
"dr. Mac jones jr.".FormatName(); // "Dr Mac Jones Jr"   (periods removed)
"JOHN SMITH".FormatName();        // "John Smith"
```

> ⚠️ Words that merely *start* with "mac" are also changed: `"mack"` → `"MacK"`. Check the result for names like Mack, Macy, Mace.

A `FormatNameTest()` method (returns a string of sample results) exists only in **Debug** builds.

---

### `ProperCase`

```csharp
public static string ProperCase(this string? stringIn, string cultureName = "en-CA")
```

Converts text to Title Case using .NET's `TextInfo.ToTitleCase`. Leading/trailing spaces are trimmed.

**Returns:** the converted string, or `""` for null/whitespace input.

```csharp
"the quick brown fox".ProperCase();   // "The Quick Brown Fox"
"the QUICK brown fox".ProperCase();   // "The QUICK Brown Fox"  — ALL-CAPS words are left alone
```

> Tip: to fully re-case all-caps input, lower-case it first: `text.ToLower().ProperCase()`, or use `FormatName()` for people's names.

---

### `FormatPhoneNumber`

```csharp
public static string? FormatPhoneNumber(this string phone, int option = 0)
```

Formats a North American phone number. Non-digits are removed, only the last 10 digits are kept (so a leading `+1` disappears), and anything after `"ext"` is treated as an extension. A 7-digit number (no area code) is formatted without one.

| Parameter | Description |
|---|---|
| `phone` | Phone number in any format. |
| `option` | `0` = `123-456-7890` (default), `1` = `(123) 456-7890`. |

**Returns:** the formatted number, or `null` if there are no digits.

```csharp
"5195551234".FormatPhoneNumber();           // "519-555-1234"
"(519)555-1234".FormatPhoneNumber(1);       // "(519) 555-1234"
"519-555-1234 ext 22".FormatPhoneNumber();  // "519-555-1234 Ext. 22"
"5551234".FormatPhoneNumber();              // "555-1234"
"5551234 ext 9".FormatPhoneNumber(1);       // "555-1234 Ext. 9"
```

### `StripPhoneNumber`

```csharp
public static string? StripPhoneNumber(this string? phone)
```

**Returns:** digits only, at most the last 10 digits; `null` if `phone` is `null`.

```csharp
"+1 (519) 555-1234".StripPhoneNumber();  // "5195551234"
```

---

### Canadian postal codes

```csharp
public static string FormatCanadianPostalCode(this string postalCode)
public static string FormatCanadianPostalCodeShort(this string postalCode)
public static string FormatCanadianOrUsPostalCode(this string postalCode)
public static PostalCodeValidationResult ValidateCanadianPostalCode(this string postalCode)
```

| Method | What it does |
|---|---|
| `FormatCanadianPostalCode` | Upper-cases, removes spaces, then inserts one space after the 3rd character. Short input is padded with `#`. Returns `""` for null or empty input. **Does not validate.** |
| `FormatCanadianPostalCodeShort` | Same, but without the space. |
| `FormatCanadianOrUsPostalCode` | Formats, then removes `#` padding and spaces — so a 5-digit US ZIP comes back unchanged. |
| `ValidateCanadianPostalCode` | Formats, then checks against Canada Post rules (valid letters only). Returns `IsValid` and `FormattedPostalCode`. When the code is **invalid**, `FormattedPostalCode` is `""`. |

```csharp
"k1a0b1".FormatCanadianPostalCode();       // "K1A 0B1"
"k1a 0b1".FormatCanadianPostalCode();      // "K1A 0B1"
"k1a0b1".FormatCanadianPostalCodeShort();  // "K1A0B1"
"90210".FormatCanadianOrUsPostalCode();    // "90210"
"invalid".FormatCanadianPostalCode();      // "INV ALI"   ← formatting is not validation!

var check = "k1a0b1".ValidateCanadianPostalCode();
if (check.IsValid) Save(check.FormattedPostalCode);   // "K1A 0B1"

var bad = "D1A0B1".ValidateCanadianPostalCode();      // D is never a valid first letter
// bad.IsValid == false, bad.FormattedPostalCode == ""
```

**Always use `ValidateCanadianPostalCode` when checking user input.**

---

### `PostalAddress`

```csharp
public static string PostalAddress(string? line1, string? line2, string? city,
    string? province, string? postal, string? country, bool html = false)
```

Builds a multi-line mailing address. Lines, city and country go through `FormatName()`, the province is upper-cased, the postal code goes through `FormatCanadianOrUsPostalCode()`. Empty parts are skipped.

| Parameter | Description |
|---|---|
| `line1`, `line2` | Street address lines. |
| `city` | City. |
| `province` | Province or state code. |
| `postal` | Postal or ZIP code. |
| `country` | Country name. |
| `html` | `true` = wrap in `<div>` and use `<br>` line breaks. |

```csharp
string addr = PostalAddress("123 main st", null, "london", "on", "n6a1b2", "canada");
// "123 Main St\nLondon, ON N6A1B2\nCanada\n"

string htmlAddr = PostalAddress("123 main st", null, "london", "on", "n6a1b2", "canada", html: true);
// "<div>\n\t123 Main St<br>\nLondon, ON N6A1B2<br>\nCanada<br>\n</div>\n"
```

---

### `FormatTextForHtml`

```csharp
public static string FormatTextForHtml(this string? stringIn)
```

Replaces every line break (`\r\n`, `\n`, `\r`) with `<br />`. Returns `""` for null/whitespace. It does **not** HTML-encode the text — encode user input first to avoid XSS.

```csharp
"Line 1\r\nLine 2\nLine 3".FormatTextForHtml();   // "Line 1<br />Line 2<br />Line 3"

// Safe use with user input:
string safe = System.Net.WebUtility.HtmlEncode(userComment).FormatTextForHtml();
```

### `RemoveHtml`

```csharp
public static string? RemoveHtml(this string? stringIn)
```

Replaces every tag and `&nbsp;` with a space, collapses runs of whitespace into one space, and trims. Null/whitespace input is returned unchanged.

```csharp
"<p>Hello&nbsp;<b>World</b></p>   <br/>!".RemoveHtml();   // "Hello World !"
```

### `RemoveControlChars`

```csharp
public static string? RemoveControlChars(this string? stringIn)
```

Removes invisible/control characters (tab, bell, zero-width characters, etc.) but **keeps** `\r` and `\n`.

```csharp
"A\tB\u0007C\r\nD".RemoveControlChars();   // "ABC\r\nD"
```

### `RemoveLastCharacter`

```csharp
public static string? RemoveLastCharacter(this string? stringIn)
```

```csharp
"a,b,c,".RemoveLastCharacter();   // "a,b,c"   — handy after building a comma list
"".RemoveLastCharacter();         // ""
```

---

### `ExtractSection`

```csharp
public static string ExtractSection(string sectionName, string template)
```

Finds a block in a template marked with HTML comments `<!-- NAME START -->` … `<!-- NAME END -->` (name is matched case-insensitively). The markers are **included** in the result.

**Returns:** the section, or `""` if the markers are missing or out of order.
**Throws:** `ArgumentException` if either argument is null/empty/whitespace.

```csharp
string template = "<p>Hi</p><!-- FOOTER START --><p>Bye</p><!-- FOOTER END -->";
string footer = ExtractSection("footer", template);
// "<!-- FOOTER START --><p>Bye</p><!-- FOOTER END -->"
```

---

### `ExtractUrl` / `ExtractUrls` and `UrlType`

```csharp
public static string ExtractUrl(this string html, UrlType urlType = UrlType.All)
public static List<string> ExtractUrls(this string html, UrlType urlType = UrlType.All)

public enum UrlType { All = 0, SrcOnly = 1, HrefOnly = 2, ImagesOnly = 3 }
```

| `UrlType` | Finds |
|---|---|
| `All` | `src="..."` and `href="..."` values |
| `SrcOnly` | `src="..."` only |
| `HrefOnly` | `href="..."` only |
| `ImagesOnly` | `src` values ending in jpg, jpeg, png, gif, tiff, bmp, svg or webp |

`ExtractUrl` returns the first match (or `""`); `ExtractUrls` returns all matches.

```csharp
string html = "<a href=\"https://a.com\">A</a><img src=\"/img/logo.png\"><script src=\"/js/app.js\"></script>";

html.ExtractUrls();                     // ["https://a.com", "/img/logo.png", "/js/app.js"]
html.ExtractUrls(UrlType.HrefOnly);     // ["https://a.com"]
html.ExtractUrls(UrlType.ImagesOnly);   // ["/img/logo.png"]
```

> ⚠️ Only **double-quoted** attributes are recognised. `<img src='a.jpg'>` returns nothing. For serious HTML parsing use a real parser such as AngleSharp or HtmlAgilityPack.

---

### Base64 and images

```csharp
public static string TextToBase64(this string data)       // UTF-8 text → Base64
public static string Base64ToText(this string text)       // Base64 → UTF-8 text
public static byte[] ReadImageToBytes(this string path)   // file → bytes
public static Image Base64ToImage(this byte[] base64)     // bytes → System.Drawing.Image
```

| Method | Throws |
|---|---|
| `TextToBase64` / `Base64ToText` | `ArgumentNullException` for null; `Base64ToText` throws `FormatException` for invalid Base64. |
| `ReadImageToBytes` | `ArgumentException` for blank path, `FileNotFoundException` if missing. |
| `Base64ToImage` | `ArgumentNullException` for null, `ArgumentException` for empty array. |

```csharp
"Hello".TextToBase64();       // "SGVsbG8="
"SGVsbG8=".Base64ToText();    // "Hello"

byte[] bytes = @"C:\images\logo.png".ReadImageToBytes();
using System.Drawing.Image img = bytes.Base64ToImage();
Console.WriteLine($"{img.Width} x {img.Height}");
```

> Despite its name, `Base64ToImage` takes **raw bytes**, not a Base64 string. To convert a Base64 string use `Convert.FromBase64String(s).Base64ToImage()`.

---

### `CurrencyStringToValue`

```csharp
public static decimal CurrencyStringToValue(this string currency)
```

Parses a US/Canadian-style currency string (`$`, thousands commas, decimal point, leading minus) and rounds to 2 decimals (half away from zero).

**Returns:** the value, or `0` if blank or unparseable.

```csharp
"$1,234.567".CurrencyStringToValue();   // 1234.57
"-$5.50".CurrencyStringToValue();       // -5.50
"abc".CurrencyStringToValue();          // 0
```

---

### `IsValidEmailAddress`

```csharp
public static bool IsValidEmailAddress(this string? emailAddress)
```

Uses MimeKit's RFC 5322 parser. The input must be **just** the address — display-name formats are rejected.

```csharp
"john@example.com".IsValidEmailAddress();          // true
"John <john@example.com>".IsValidEmailAddress();   // false
"john@".IsValidEmailAddress();                     // false
```

### `IsValidFileName`

```csharp
public static bool IsValidFileName(this string? fileName)
```

Checks the **file-name part** of the input (folders are ignored). Rejects blank names, Windows reserved names (`CON`, `PRN`, `AUX`, `NUL`, `COM1`–`COM9`, `LPT1`–`LPT9`, `CLOCK$`) and the characters `< > : | ? * &` (`&` is blocked because it breaks URLs).

```csharp
"report.pdf".IsValidFileName();             // true
@"C:\temp\report.pdf".IsValidFileName();    // true  (only "report.pdf" is checked)
"a&b.txt".IsValidFileName();                // false
"CON".IsValidFileName();                    // false
"CON.txt".IsValidFileName();                // true  ← Windows actually forbids this too
```

### `IsValidPath` / `IsValidPathWithExistingRoot`

```csharp
public static bool IsValidPath(this string? path)
public static bool IsValidPathWithExistingRoot(this string? path)
```

`IsValidPath` checks only the **syntax** (using `Path.GetFullPath`). `IsValidPathWithExistingRoot` also checks that the drive / UNC share / root exists. Neither checks that the file or folder itself exists — use `File.Exists` / `Directory.Exists` for that.

```csharp
@"C:\temp\x.txt".IsValidPath();                  // true (even if C:\temp doesn't exist)
"C:\\te\0mp".IsValidPath();                      // false (null character)
@"Q:\nothing".IsValidPathWithExistingRoot();     // false if there is no Q: drive
```

---

## 5. StringUtilities

`public static class StringUtilities` — general string helpers. Only the methods marked (ext) are extension methods; call the rest as `StringUtilities.Method(...)`.

### Conversions

| Method | Returns | Behaviour |
|---|---|---|
| `StringToInteger(this string)` (ext) | `int` | Parses numbers with thousands separators. **Returns `-1` if invalid** (careful: `-1` is also a valid number). |
| `StringToBoolean(this string)` (ext) | `bool` | `true` for `true`, `t`, `yes`, `y`, `1`, `-1` (any case, trimmed). Everything else (including `null`) → `false`. |
| `BooleanToString(this bool)` (ext) | `string` | `"true"` / `"false"` (lower case). |
| `ToCamelCase(this string)` (ext) | `string` | Lower-cases the first character: `"FirstName"` → `"firstName"`. |
| `SplitCamelCase(this string)` (ext) | `string` | ⚠️ **Broken** — see [Known Issues](#32-known-issues--gotchas). Do not use. |
| `GetBytes(string)` | `byte[]` | Raw UTF-16 bytes (2 bytes per char). |
| `GetString(byte[])` | `string` | Reverse of `GetBytes`. |

```csharp
"1,234".StringToInteger();     // 1234
"abc".StringToInteger();       // -1
"Yes".StringToBoolean();       // true
"no".StringToBoolean();        // false
true.BooleanToString();        // "true"
"FirstName".ToCamelCase();     // "firstName"
```

### Sanitising input

| Method | Returns | Keeps |
|---|---|---|
| `CharsOnly(string? input = null)` | `string` | `a–z`, `A–Z`, `0–9` **and `_`**. Null → `""`. |
| `LettersOnly(string? input = null)` | `string` | `a–z`, `A–Z` and spaces. Null → `""`. |
| `NumbersOnly(string? input = null)` | `string?` | Digits only. Null → `null`. |
| `DecimalOnly(string? input = null)` | `string?` | Digits, `.` and `,`. Null → `""`. |
| `DecimalOnlyAsDecimal(string? input = null)` | `decimal` | `DecimalOnly`, then converted with `TypeCasting.ToDecimal`. Null/invalid → `0`. |
| `ReturnInitialNumbers(string txt)` | `string` | The first number found (may include one decimal point), or `""`. |
| `StripInvalidFileNameCharacters(string filename)` | `string` | Removes `\ / : * ? " < > \| ( )`. |
| `RemoveWhitespace(string source)` | `string` | Removes all whitespace. |
| `CompareCharactersOnly(string, string)` | `bool` | Equal after removing whitespace, ignoring case. Nulls count as `""`. |

```csharp
StringUtilities.CharsOnly("hello_123! world");         // "hello_123world"
StringUtilities.LettersOnly("hello 123!");             // "hello "
StringUtilities.NumbersOnly("(519) 555-1234");         // "5195551234"
StringUtilities.DecimalOnly("$1,234.56 CAD");          // "1,234.56"
StringUtilities.DecimalOnlyAsDecimal("$1,234.56 CAD"); // 1234.56
StringUtilities.ReturnInitialNumbers("123 Departures");// "123"
StringUtilities.ReturnInitialNumbers("Gate 12.5b");    // "12.5"  (first number anywhere)
StringUtilities.StripInvalidFileNameCharacters("Show: Part (1)?.mp4"); // "Show Part 1.mp4"
StringUtilities.RemoveWhitespace(" a b\tc\n");         // "abc"
StringUtilities.CompareCharactersOnly("Hello World", " hello   world "); // true
```

### Searching and changing text

| Method | Returns | Behaviour |
|---|---|---|
| `ReverseString(string s)` | `string` | `"abc"` → `"cba"`. |
| `FirstWords(string text, int number)` | `string` | First *n* space-separated words. Never throws — returns what it has. |
| `RepeatString(string text, int number)` | `string` | `text` repeated *n* times. |
| `ReplaceFirstOccurrence(string Source, string Find, string Replace, StringComparison)` | `string` | Replace only the first match. Returns `Source` unchanged if `Find` isn't found or either string is null/empty. |
| `ReplaceLastOccurrence(string Source, string Find, string Replace, StringComparison)` | `string` | Replace only the last match. Same "unchanged" rules. |
| `ReturnConnectionStringSegment(string text, string search)` | `string?` | Value of one key in a connection string, or `null`. |
| `GetQueryStringParameters(string queryString)` | `IDictionary<string,string>` | Parses `?a=1&b=2`. Keys are case-insensitive; the first value of a repeated key wins. |
| `ReturnValidSQLInString(string StringIn)` | `string` | `"a,b"` → `"'a','b'"`. |
| `ReturnShortStringTime()` | `string` | Current time as `M` + `ddHHmmss`, e.g. `"1018143005"` for Oct 18 14:30:05. |

```csharp
StringUtilities.FirstWords("the quick brown fox", 2);    // "the quick"
StringUtilities.FirstWords("the quick", 5);              // "the quick"
StringUtilities.RepeatString("ab", 3);                   // "ababab"
StringUtilities.ReplaceFirstOccurrence("a-b-c", "-", "+", StringComparison.Ordinal); // "a+b-c"
StringUtilities.ReplaceLastOccurrence("a-b-c", "-", "+", StringComparison.Ordinal);  // "a-b+c"
StringUtilities.ReplaceFirstOccurrence("a-b-c", "x", "+", StringComparison.Ordinal); // "a-b-c" (not found)

StringUtilities.ReturnConnectionStringSegment(
    "Data Source=SQL01;Initial Catalog=Sales;Integrated Security=true", "Initial Catalog"); // "Sales"

var qs = StringUtilities.GetQueryStringParameters("?id=5&Name=Bob&id=6");
// qs["id"] == "5", qs["name"] == "Bob"
```

> ⚠️ `GetQueryStringParameters` throws `IndexOutOfRangeException` for a key without `=` (e.g. `"a=1&flag"`). In ASP.NET Core, prefer `Request.Query` or `Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery`.
> ⚠️ `ReturnValidSQLInString` builds SQL text by hand — **never use it with user input** (SQL injection). Use `SqlParameter` instead.

### HTML helpers

| Method | Returns | Behaviour |
|---|---|---|
| `StripHTML(string input)` | `string` | Removes tags. |
| `StripHTMLReplaceBR(string input)` | `string` | `<br>` → new line, removes other tags, decodes `&gt; &lt; &amp; &#39;`. |
| `StripHTML2(string input, int length)` | `string` | Removes `<style>` blocks and tags, decodes entities, truncates to `length` characters, then turns new lines into `<br />`. Use with `@Html.Raw()`. |
| `DecodeHtml(string input)` | `string?` | Decodes only `&gt; &lt; &amp; &#39;`. |
| `GetFullExceptionMessage(Exception ex, string webRoot)` | `string` | Numbered list of the exception and inner-exception messages, plus a shortened stack trace (removes `webRoot`, "Controller", "Model", ".cs", etc.). |

```csharp
StringUtilities.StripHTML("<p>Hi <b>there</b></p>");                      // "Hi there"
StringUtilities.StripHTMLReplaceBR("Line1<br/>Line2 &amp; more");          // "Line1\r\nLine2 & more"
StringUtilities.StripHTML2("<style>p{}</style><p>Line1<br/>Line2</p>", 8); // "Line1<br />L"
StringUtilities.DecodeHtml("&lt;b&gt; &amp; &#39;");                        // "<b> & '"

try { DoWork(); }
catch (Exception ex) { log.Error(StringUtilities.GetFullExceptionMessage(ex, @"C:\inetpub\wwwroot\MyApp")); }
```

### Hashing, passwords, and generic helpers

| Method | Returns | Behaviour |
|---|---|---|
| `CalculateMD5Hash(byte[])` / `CalculateMD5Hash(string)` | `string` | MD5 as upper-case hex. String overload uses ASCII encoding. |
| `CalculateMD5Hash_ToDouble(string)` | `double` | First 8 bytes of the MD5 hash as a `double`. |
| `CalculateMD5Hash_ToGuid(byte[])` / `(string)` | `Guid` | MD5 hash as a `Guid` — handy for a repeatable ID from text. |
| `CreatePassword(int passwordLength, bool includeSymbols = false)` | `string` | Random password with at least one digit, upper and lower letter. Excludes look-alikes `l` and `I`. |
| `EncryptPassword(string)` / `DecryptPassword(string)` | `string` | ⚠️ **Not encryption** — just Base64 of UTF-16 bytes. Anyone can reverse it. |
| `Max<T>(T a, T b)` / `Min<T>(T a, T b)` | `T` | Larger / smaller of two comparable values. |

```csharp
StringUtilities.CalculateMD5Hash("hello");         // "5D41402ABC4B2A76B9719D911017C592"
StringUtilities.CalculateMD5Hash_ToGuid("hello");  // 2a40415d-4bbc-762a-b971-9d911017c592
StringUtilities.CreatePassword(12);                // e.g. "qmEOQoNQe0zT"
StringUtilities.CreatePassword(12, includeSymbols: true);
StringUtilities.Max(3, 7);                         // 7
StringUtilities.Min("apple", "banana");            // "apple"
```

> MD5 is fine for checksums and cache keys but **must not** be used for passwords. See [Passwords](#13-passwords--iencryptor) for a safe approach.

---

## 6. TypeCasting

`public static class TypeCasting` — conversions that **never throw**. On failure they return a default value. Use them when reading loosely-typed data (form posts, `object` values, spreadsheet cells).

### Numbers

| Method | Returns | On null / failure |
|---|---|---|
| `ToInt(object?)` | `int` | `0`. Converts via `double`, so `"12.7"` → `13` (rounds). |
| `ToInt(int?)` | `int` | `0` |
| `ToNullableInt(string?)` | `int?` | `null`. Strict: `"12.7"` → `null`. |
| `ToNullableInt(decimal?)` | `int?` | `null`. Truncates: `12.7m` → `12`. |
| `ToNullableIntFromObject(object?)` | `int?` | `null` |
| `ToStringInt(int?)` | `string` | `""` |
| `ToDouble(object?)` | `double` | `0.0` |
| `ToNullableDouble(string?)` | `double?` | `null` |
| `ToStringDouble(double?)` | `string` | `""` |
| `ToFloat(object?)` | `float` | `0f` |
| `ToLong(object?)` | `long` | `0` |
| `ToDecimal(object?)` | `decimal` | `0.0m` |
| `ToNullableDecimal(string?)` | `decimal?` | `null` |
| `ToNullableDecimal(double?)` | `decimal?` | `null` |
| `ToNullableDecimalFromObject(object?)` | `decimal?` | `null` |
| `ToStringDecimal(object? value, string format)` | `string` | `""` |
| `ToStringDecimal(decimal?)` | `string` | `""` |
| `ToStringDecimal(decimal?, string format)` | `string` | `""` |
| `GetValue(object?)` | `object?` | Unwraps a boxed `int?`/`decimal?`; otherwise returns the input. |

```csharp
TypeCasting.ToInt("123");                       // 123
TypeCasting.ToInt("12.7");                      // 13
TypeCasting.ToInt("abc");                       // 0
TypeCasting.ToNullableInt("abc");               // null
TypeCasting.ToDecimal("99.99");                 // 99.99m
TypeCasting.ToLong("12345678901");              // 12345678901
TypeCasting.ToStringDecimal("99.995", "C");     // "$100.00"
TypeCasting.ToStringDecimal(1234.5m, "N2");     // "1,234.50"

// Typical use: reading a form value
int quantity = TypeCasting.ToInt(Request.Form["qty"].ToString());
decimal? discount = TypeCasting.ToNullableDecimal(Request.Form["discount"].ToString());
```

> Choosing between `ToInt` and `ToNullableInt`: use the **nullable** version when "no value" and "zero" mean different things (e.g. an optional field).

### Booleans

| Method | Returns | Behaviour |
|---|---|---|
| `ToBool(bool?)` | `bool` | `null` → `false`. |
| `ToBool(object?)` | `bool` | Uses `bool.Parse` — only `"true"`/`"false"` (any case) work. `"yes"` → `false`. |
| `ToNullableBool(object?)` | `bool?` | `null` → `null`; unparseable → `false`. |
| `ToBoolFromString(object?)` | `bool` | Flexible: anything starting with `t` → `true`, with `f` → `false`; `yes`/`y`/`1` → `true`; `no`/`n`/`0`/`na`/`n/a` → `false`. |
| `ToStringBool_YesBlank(string)` / `(bool?)` | `string` | `"Yes"` or `""`. |
| `ToStringBool_YesNo(string)` / `(bool?)` | `string` | `"Yes"` or `"No"`. |

```csharp
TypeCasting.ToBool((object)"true");          // true
TypeCasting.ToBool((object)"yes");           // false  ← strict
TypeCasting.ToBoolFromString("yes");         // true   ← flexible
TypeCasting.ToBoolFromString("0");           // false
TypeCasting.ToStringBool_YesNo((bool?)null); // "No"
```

> ⚠️ `ToBoolFromString` only looks at the first letter for t/f: `"tomato"` → `true`.

### Strings

| Method | Returns | Behaviour |
|---|---|---|
| `ToStringOrEmpty(object?)` | `string` | `null` → `""`. |
| `ToStringOrDefault(object?, string ifBlankValue)` | `string` | Returns `ifBlankValue` when null/empty/whitespace. |
| `ToString(object?, int maxStringLength)` | `string` | Returns the text only if it is **shorter than** `maxStringLength`; otherwise `""` (it does **not** truncate). |
| `ToString(object?, int subStringStart, int maxStringLength)` | `string` | Skips `subStringStart` characters, then truncates to `maxStringLength`. |
| `ToFixedString(object?, int maxStringLength)` | `string` | Pads with spaces to `maxStringLength`; text that is too long becomes all spaces. |

```csharp
TypeCasting.ToStringOrDefault("  ", "N/A");       // "N/A"
TypeCasting.ToString("Hello World", 20);          // "Hello World"
TypeCasting.ToString("Hello World", 10);          // ""   ← too long, so empty
TypeCasting.ToString("Hello World", 6, 3);        // "Wor"
TypeCasting.ToFixedString("abc", 6);              // "abc   "
```

### Dates, times and GUIDs

| Method | Returns | On null / failure |
|---|---|---|
| `ToDateTime(object?)` | `DateTime` | `DateTime.MinValue` |
| `ToDateTime(object?, string format)` | `DateTime` | `DateTime.MinValue` (exact format, invariant culture) |
| `ToNullableDateTime(object?)` | `DateTime?` | `null` |
| `ToNullableDateTime(object?, string format)` | `DateTime?` | `null` (exact format, current culture) |
| `ToNullableTimeSpan(object?)` | `TimeSpan?` | `null` |
| `ToTimeSpan(string?)` | `TimeSpan?` | `null` |
| `ToStringDateTime(object?, string format)` | `string` | `""` |
| `ToStringDateTime(DateTime?, string format)` | `string` | `""` |
| `ToStringDateTime(TimeSpan?, string format)` | `string` | `""` (treats the TimeSpan as a time of day) |
| `ToGuid(object?)` | `Guid` | `Guid.Empty` |
| `ToNullableGuid(object?)` | `Guid?` | `null` |

```csharp
TypeCasting.ToDateTime("2024-01-15");                    // 2024-01-15 00:00
TypeCasting.ToDateTime("15/01/2024", "dd/MM/yyyy");      // 2024-01-15 00:00
TypeCasting.ToDateTime("nope");                          // DateTime.MinValue (0001-01-01)
TypeCasting.ToStringDateTime("2024-01-15", "yyyy-MM-dd");// "2024-01-15"
TypeCasting.ToStringDateTime(new TimeSpan(14, 30, 0), "hh:mm tt"); // "02:30 PM"
TypeCasting.ToTimeSpan("01:30:00");                      // 01:30:00
TypeCasting.ToGuid("nope");                              // Guid.Empty
```

> Always check for `DateTime.MinValue` / `Guid.Empty` after calling the non-nullable versions, or use the nullable versions instead.

---

## 7. DateHelper

`public static class DateHelper` — date conversions and calculations.

### Unix timestamps

A Unix timestamp is the number of seconds since 1 January 1970 UTC.

| Method | Returns | Behaviour |
|---|---|---|
| `DateTimeToUnixTimestampUtc(this DateTime)` (ext) | `long` | Converts to UTC first (a `Local`/`Unspecified` date is treated as local time), then to seconds. **Recommended.** |
| `ToUnixTimeSeconds(this DateTime)` (ext) | `long` | Uses the date's clock value as-is, with **no** time-zone conversion. Only correct for UTC dates. |
| `UnixTimeStampToUtc(this long)` (ext) | `DateTime` | Seconds → UTC `DateTime`. |
| `UnixTimeStampToLocalTimeZone(this long)` (ext) | `DateTime` | Seconds → local `DateTime`. |
| `UnixTimeStampToDateTime(this double)` (ext) | `DateTime` | Same as above, for `double` input. |

```csharp
var utc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
long ts = utc.DateTimeToUnixTimestampUtc();       // 1705314600
DateTime back = 1705314600L.UnixTimeStampToUtc(); // 2024-01-15T10:30:00Z
DateTime local = 1705314600L.UnixTimeStampToLocalTimeZone();
```

### Formatting

| Method | Returns |
|---|---|
| `ToShortDateWithFullDigitsString(this DateTime)` (ext) | Short date with 2-digit day/month, current culture |
| `ToShortDateWithFullDigitsString(this DateTime, string culture)` (ext) | Same, named culture (e.g. `"en-US"`) |
| `ToShortDateWithFullDigitsString(this DateTime, CultureInfo)` (ext) | Same, `CultureInfo` |
| `ToLongDateWithoutDayNameString(this DateTime)` (ext) | Long date without the weekday, current culture |
| `ToLongDateWithoutDayNameString(this DateTime, string culture)` (ext) | Same, named culture |
| `ToLongDateWithoutDayNameString(this DateTime, CultureInfo)` (ext) | Same, `CultureInfo` |

```csharp
var d = new DateTime(2024, 1, 5);
d.ToShortDateWithFullDigitsString("en-US");   // "01/05/2024"
d.ToLongDateWithoutDayNameString();           // "January 5, 2024"   (en-US)
d.ToLongDateWithoutDayNameString("fr-CA");    // "5 janvier 2024"
```

> ⚠️ `ToShortDateWithFullDigitsString` only works for cultures whose short pattern uses single `d`/`M` (like `en-US`'s `M/d/yyyy`). For `en-CA` it returns `"2024-January-Friday"` and for `en-GB` `"Friday/January/2024"`. Use `d.ToString("MM/dd/yyyy")` or `d.ToString("yyyy-MM-dd")` instead.

### Calculations

```csharp
public static List<DateTime>? GetDaysFromRange(List<DateTime> DateRange, DayOfWeek dayOfWeek)
public static int BusinessDaysBetweenDates(DateTime startDate, DateTime endDate)
public static int BusinessDaysBetweenDates(string startDate, string endDate)   // parsed as en-US
public static int BusinessDaysBetweenDates(DateOnly startDate, DateOnly endDate)
```

**`GetDaysFromRange`** — returns the dates in the list that fall on `dayOfWeek`; `null` if the list is empty.

**`BusinessDaysBetweenDates`** — counts Monday–Friday days **after** `startDate` up to and **including** `endDate`. Times are ignored, dates may be in either order, holidays are **not** excluded.

```csharp
DateHelper.BusinessDaysBetweenDates(new DateTime(2024, 1, 5), new DateTime(2024, 1, 8)); // 1 (Fri → Mon)
DateHelper.BusinessDaysBetweenDates(new DateTime(2024, 1, 5), new DateTime(2024, 1, 5)); // 0
DateHelper.BusinessDaysBetweenDates("01/01/2024", "01/31/2024");                         // 22

var twoWeeks = Enumerable.Range(0, 14).Select(i => new DateTime(2024, 1, 1).AddDays(i)).ToList();
DateHelper.GetDaysFromRange(twoWeeks, DayOfWeek.Saturday);   // Jan 6, Jan 13
```

### `GetDayInMonth` (in `Function`)

```csharp
public static string? GetDayInMonth(int yearIn = 1970, int monthIn = 1,
    DayOfWeek dayIn = DayOfWeek.Monday, int weekCount = 1)
```

Finds the *n*th weekday of a month, e.g. "3rd Friday of January".

| Parameter | Description |
|---|---|
| `yearIn` | Year. Values below 1970 are replaced with the current year. |
| `monthIn` | Month 1–12 (invalid → 1). |
| `dayIn` | Day of the week to find. |
| `weekCount` | Which occurrence, 1–5 (invalid → 1). |

**Returns:** the date as a **short date string** in the current culture (e.g. `"1/19/2024"`), or `null` if that occurrence doesn't exist (e.g. a 5th Monday in a month with only four).

```csharp
string? third = GetDayInMonth(2024, 1, DayOfWeek.Friday, 3);   // "1/19/2024"
if (third != null)
{
    DateTime date = DateTime.Parse(third);
}
```

> ⚠️ Bug: returns `null` when the requested weekday is the day *before* the weekday the month starts on, in week 1. Example: `GetDayInMonth(2026, 6, DayOfWeek.Sunday, 1)` returns `null` instead of June 7. See [Known Issues](#32-known-issues--gotchas).

---

## 8. Money Helpers — rounding and change

Canada has no penny, so cash totals are rounded to the nearest 5 cents.

```csharp
public static decimal RoundToNearestFiveCents(decimal amount)
public static decimal RoundToNickel(this decimal change)   // (ext)
public static decimal RoundToNickel(this string amount)    // (ext) — throws if not a number

public static decimal CashChange(decimal? charge = 0, decimal? tendered = 0)
public static decimal CashChange(string charge, string tendered)
public static decimal CashChange(decimal? charge, string tendered)
public static decimal CashChange(string charge, decimal? tendered)
public static decimal CashChange(float charge, float tendered)
public static decimal CashChange(float charge, decimal? tendered)
public static decimal CashChange(decimal charge, float tendered)
```

- Rounding is half away from zero: `1.025` → `1.05`.
- `CashChange` returns `tendered − charge`, rounded to a nickel. If either amount is negative it returns `0`. If the customer paid **too little** the result is **negative**.
- String overloads accept `$` and thousands commas (en-US format); invalid strings count as `0`.

```csharp
1.02m.RoundToNickel();               // 1.00
1.03m.RoundToNickel();               // 1.05
CashChange(7.42m, 10m);              // 2.60
CashChange("$7.42", "$20.00");       // 12.60
CashChange(10m, 5m);                 // -5.00  ← customer still owes $5

decimal change = CashChange(total, paid);
if (change < 0) ShowMessage($"Amount owing: {-change:C}");
```

---

## 9. Random Values

```csharp
public static int RandomNumber(int min, int max)                  // min inclusive, max exclusive
public static string RandomString(int size, bool lowerCase = false)
public static string RandomPassword()
public static string RandomFileName(int size)
public static string RandomizeString(string? inputString)
```

| Method | Behaviour |
|---|---|
| `RandomNumber` | Returns `min` ≤ value < `max`. If `min > max`, `min` is set to `max` and the result is always `max`. |
| `RandomString` | Letters A–Z (or a–z). `size` below 1 becomes 1. |
| `RandomPassword` | 10 characters: 4 lower-case, 4 digits, 2 upper-case, shuffled. |
| `RandomFileName` | Cryptographically random, file-name-safe letters/digits. |
| `RandomizeString` | Shuffles the characters (Fisher–Yates). Null/empty → `""`. |

```csharp
int dice = RandomNumber(1, 7);       // 1..6
string code = RandomString(6);       // e.g. "QXBTRA"
string pwd = RandomPassword();       // e.g. "78Hy8uarS0"
string tmp = RandomFileName(12) + ".tmp";
RandomizeString("abcdef");           // e.g. "cfadbe"
```

> `RandomNumber`, `RandomString`, `RandomPassword` and `RandomizeString` use `Random.Shared`, which is **not** cryptographically secure. Don't use them for security tokens; use `RandomNumberGenerator` instead.

---

## 10. Drop-Down List Data

These methods return lists of [`SelectListItem`](#selectlistitem) ready to bind to an HTML `<select>`.

| Member | Type | Contents |
|---|---|---|
| `CountryDropDownList(string country = "")` | `List<SelectListItem>` | 248 countries; Canada and United States first. Selects by code (`"CA"`) or name, case-insensitive. |
| `CountryList()` | `List<string>` | Country names. |
| `ProvinceDropDownList(string province = "")` | `List<SelectListItem>` | 13 Canadian provinces/territories (`Group` = "Province"/"Territory"). |
| `ProvinceList` | `List<string>` | Names, e.g. `"Canada - Ontario"`. |
| `ProvinceListShortNames` | `List<string>` | Codes, e.g. `"ON"`. |
| `Provinces` | `List<Territory>` | The raw data. |
| `StateDropDownList(string state = "")` | `List<SelectListItem>` | 56 US states and territories (`Group` = "State", etc.). |
| `StateList` / `StateListShortNames` | `List<string>` | Names (`"US - Alabama"`) / codes (`"AL"`). |
| `States` | `List<Territory>` | The raw data. |
| `ProvinceStateDropDownList(string territory = "")` | `List<SelectListItem>` | 69 items: provinces then states. Note: `Group` is **not** set on these items. |
| `TerritoryList` / `ProvStateListShortNames` | `List<string>` | Province + state names / codes. |
| `MonthDropDownList(int monthToSelect = 0)` | `List<SelectListItem>` | 12 months, values `"1"`–`"12"`. |
| `MonthNamesList` / `MonthShortNamesList` | `List<string>` | `"January"`… / `"Jan"`… |
| `MonthList` | `List<MonthName>` | `Value`, `Name`, `ShortName`. |
| `DayDropDownList(int dayToSelect = 0)` | `List<SelectListItem>` | 7 days, values `"0"` (Sunday)–`"6"` (Saturday). |
| `DayNamesList` / `DayShortNamesList` | `List<string>` | `"Sunday"`… / `"Sun"`… |
| `DayList` | `List<DayName>` | `Value`, `Name`, `ShortName`. |

Nested model classes: `Function.MonthName` and `Function.DayName` (`int Value`, `string Name`, `string ShortName`), and `Country` (`string ID`, `string Name`).

```csharp
List<SelectListItem> months = MonthDropDownList(DateTime.Today.Month);
List<SelectListItem> provinces = ProvinceDropDownList("on");             // Ontario is Selected
string? chosen = CountryDropDownList("ca").Find(x => x.Selected)?.Text;  // "Canada"
string? state = ProvinceStateDropDownList("tx").Find(x => x.Selected)?.Text; // "US - Texas"
```

> `Provinces`, `States`, `MonthList` and `DayList` are shared `static readonly` lists — the *variable* can't be reassigned, but the list contents **can** be changed. Never call `Add`/`Remove` on them; copy them first (`Provinces.ToList()`) if you need to modify the data.

Using it in a Razor page (convert to MVC's own `SelectListItem` type, which the tag helper expects):

```cshtml
@using MvcItem = Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
<select asp-for="Province"
        asp-items="@FCS.Utilities.Function.ProvinceDropDownList(Model.Province)
                    .Select(p => new MvcItem(p.Text, p.Value, p.Selected))"></select>
```

---

## 11. Configuration & Environment Helpers

### `GetAppSetting`

```csharp
public static string? GetAppSetting(string setting)
public static string  GetAppSetting(string setting, string defaultValue)
```

Reads a value from `appsettings.json` / environment variables (see [Configuration](#3-configuration-appsettingsjson)). Use `:` to go into sections. The configuration is loaded once and reused (files reload automatically when changed).

**Throws:** `ArgumentException` if `setting` is blank.

```csharp
string? host = GetAppSetting("Email:SmtpHost");
string port = GetAppSetting("Email:SmtpPort", "25");
```

### `GetConnectionString`

```csharp
public static string? GetConnectionString(string? setting = "Default")
```

Returns `ConnectionStrings:<setting>` or `null`. Blank `setting` means `"Default"`.

> Differences from `GetAppSetting`: it reads `appsettings.json` from the **current working directory** (not the app folder), only looks at `ASPNETCORE_ENVIRONMENT`, and rebuilds the configuration on every call (slow in loops). In ASP.NET Core apps prefer injecting `IConfiguration`.

```csharp
string? cs = GetConnectionString();             // "Default"
string? reports = GetConnectionString("Reports");
```

### `WebPath`

```csharp
public static string WebPath { get; set; }
```

The folder where `QRCode.Create` saves images. Defaults to the folder of the entry assembly (or the current directory). In a web app, set it to `wwwroot` at start-up:

```csharp
Function.WebPath = app.Environment.WebRootPath;
```

### `GetIpAddressAsync`

```csharp
public static Task<string> GetIpAddressAsync()
```

Returns this machine's **public** IP address by calling `https://api.ipify.org`. Needs internet access; throws `HttpRequestException` on failure.

```csharp
string ip = await GetIpAddressAsync();   // e.g. "203.0.113.45"
```

---

## 12. Reflection Helpers

```csharp
public static bool IsNullable(Type type)
public static Type GetCoreType(Type? type)
public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
```

| Method | Behaviour |
|---|---|
| `IsNullable` | `true` for reference types and `Nullable<T>`; `false` for plain value types. Throws on `null`. |
| `GetCoreType` | Unwraps `int?` → `int`; other types returned unchanged; `null` → `typeof(object)`. |
| `GetAttribute<T>` | Gets an attribute placed on an enum member, or `null`. |

```csharp
IsNullable(typeof(int?));    // true
IsNullable(typeof(int));     // false
IsNullable(typeof(string));  // true
GetCoreType(typeof(int?));   // System.Int32
```

```csharp
public enum Status
{
    [System.ComponentModel.Description("Waiting for approval")] Pending,
    Approved
}

// elsewhere:
string text = Status.Pending.GetAttribute<System.ComponentModel.DescriptionAttribute>()?.Description
              ?? Status.Pending.ToString();   // "Waiting for approval"
```

---

## 13. Passwords — `IEncryptor`

### `IEncryptor` (interface only)

Namespace `FCS.Utilities.Interfaces`.

```csharp
public interface IEncryptor
{
    string GetSalt();                                               // create a random salt
    string GetHash(string value, string salt);                      // hash value + salt
    string CreatePasswordHash(string password, string hashAlgorithm); // hash with a named algorithm
    bool IsPasswordValid(string password, string hash, string salt);  // check a password against a stored hash
    bool PasswordStrengthCheck(string password, int minimumLength);   // basic strength rules
}
```

> ⚠️ **No implementation is currently shipped.** The `Encryptor` class (`Functions\Passwords.cs`) is excluded from the build in `FCS.Utilities.csproj`, and the `string.PasswordStrengthCheck()` extension method has been removed. Code that used `new Encryptor()` will not compile against this version.

### What to use instead

For new code, use the framework's built-in PBKDF2 support. A minimal pattern:

```csharp
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16, HashSize = 32, Iterations = 100_000;

    public static (string Hash, string Salt) Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool Verify(string password, string storedHash, string storedSalt)
    {
        byte[] salt = Convert.FromBase64String(storedSalt);
        byte[] attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return CryptographicOperations.FixedTimeEquals(attempt, Convert.FromBase64String(storedHash));
    }
}
```

In ASP.NET Core apps you can also use `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>`.

Never use `StringUtilities.EncryptPassword` or MD5 for passwords.

---

## 14. EmailHelper

`public class EmailHelper` — builds and sends an email with [MailKit](https://github.com/jstedfast/MailKit). Create one instance per message.

### Properties and fields

| Member | Type | Default | Description |
|---|---|---|---|
| `ToAddressList` | `List<Tuple<string,string>>` | empty | "To" recipients. Each tuple is (address, display name) — either order is accepted. |
| `CCAddressList` | `List<Tuple<string,string>>` | empty | CC recipients. |
| `BCCAddressList` | `List<Tuple<string,string>>` | empty | BCC recipients. |
| `ReplyToAddressList` | `List<Tuple<string,string>>` | empty | Reply-To addresses. |
| `Attachments` | `List<string>` | empty | Full paths of files to attach. Missing files make `Send()` fail. |
| `InlineAttachments` | `List<InlineAttachment>` | empty | Images embedded in the HTML body. |
| `StringAttachments` | `List<StringAttachment>` | empty | Attachments created from text (see `CreateAttachmentFromString`). |
| `FromAddress` | `string` | `""` | **Required.** Sender address (lower-cased). |
| `FromName` | `string` | `""` | Sender display name (defaults to `FromAddress`). |
| `Subject` | `string` | `""` | Subject (line breaks are replaced with spaces). |
| `TextBody` | `string` | `""` | Plain-text body. If `IsHtml` and `HtmlBody` is empty, this is used as the HTML. |
| `HtmlBody` | `string` | `""` | HTML body. |
| `IsHtml` | `bool` | `true` | Send an HTML message. When `true`, the text part is generated by stripping tags from `TextBody`. |
| `SmtpHost` | `string` | `""` | SMTP server. Falls back to `Email:SmtpHost`. |
| `SmtpPort` | `string` | `"25"` | Port as text (1–65535). |
| `SmtpUserId` | `string` | `""` | Login user. Required for port 587 (falls back to `Email:SmtpUserId`). |
| `SmtpPassword` | `string` | `""` | Login password. Required for port 587 (falls back to `Email:SmtpPassword`). |
| `UseSSL` | `bool` | `true` | Use STARTTLS when the server offers it. `false` = no encryption. |
| `Importance` | `MimeKit.MessageImportance` | `Normal` | Out-of-range values reset to `Normal`. |
| `Priority` | `MimeKit.MessagePriority` | `Normal` | Out-of-range values reset to `Normal`. |
| `SmtpPickupDirectory` | `string` | `""` | Folder for `.eml` files when sending is disabled. |
| `TimeoutMilliseconds` | `int` | `100000` | SMTP timeout. |
| `AllowInvalidServerCertificate` | `bool` | `false` | Accept bad TLS certificates. **Only for testing.** |

Every message automatically gets the header `X-SmtpEmail: Copyright Forest City Software 1999 - {year}`.

### Methods

#### `Send()`

```csharp
public Tuple<bool, string> Send()
```

Validates, builds and sends the message. It never throws for normal problems — check the result.

What it does, step by step:

1. **Validates**: at least one To/CC/BCC; valid `FromAddress`; `SmtpHost` set; valid port; user/password for port 587; every recipient is a valid address. Display names are tidied with `FormatName()`.
2. **Builds** the message, adding headers, recipients and attachments.
3. If `Email:Override` is a valid address, **all recipients are replaced** by that address and the original list is appended to the text body.
4. If `Email:SendEnabled` is `false`, the message is **written to a `.eml` file** (in `SmtpPickupDirectory` or the app folder) instead of being sent.
5. Otherwise connects, authenticates if a user id is set and the server supports it, sends, and disconnects.

**Returns:** `(true, "Success")`, `(true, "Email written to <path>")`, or `(false, "<reason>")`.

```csharp
var email = new EmailHelper
{
    FromAddress = "noreply@example.com",
    FromName = "Example Store",
    Subject = "Your order has shipped",
    HtmlBody = "<h1>Good news!</h1><p>Your order is on its way.</p>",
    TextBody = "Good news! Your order is on its way.",
    SmtpHost = "smtp.example.com",
    SmtpPort = "587",
    SmtpUserId = "noreply@example.com",
    SmtpPassword = GetAppSetting("Email:SmtpPassword")!
};

email.ToAddressList.Add(new Tuple<string, string>("jane@example.com", "Jane Doe"));
email.CCAddressList.Add(new("sales@example.com", ""));
email.Attachments.Add(@"C:\Invoices\INV-1001.pdf");

Tuple<bool, string> result = email.Send();
if (!result.Item1) logger.LogWarning("Email failed: {Reason}", result.Item2);
```

#### `CreateAttachmentFromString`

```csharp
public void CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
```

Adds an attachment built from text in memory — no temporary file needed.

| Parameter | Description |
|---|---|
| `content` | Attachment text (`null` → empty). |
| `name` | File name the recipient sees. **Required** — throws `ArgumentException` if blank. |
| `contentEncoding` | Text encoding (`null` → UTF-8). |
| `mediaType` | MIME type, e.g. `"text/csv"` (blank → `"text/plain"`). |

```csharp
string csv = "Id,Name\n1,Ann\n2,Bob";
email.CreateAttachmentFromString(csv, "customers.csv", Encoding.UTF8, "text/csv");
```

#### `AddHeader`

```csharp
public void AddHeader(string name, string value)
```

Adds a custom header. Blank names/values and values containing line breaks are silently ignored (prevents header injection).

```csharp
email.AddHeader("X-Campaign", "spring-sale");
```

#### `ClearAllRecipients`

```csharp
public void ClearAllRecipients()
```

Empties To, CC, BCC and Reply-To — useful when re-using one `EmailHelper` for several recipients.

```csharp
foreach (var customer in customers)
{
    email.ClearAllRecipients();
    email.ToAddressList.Add(new(customer.Email, customer.Name));
    email.Send();
}
```

### Inline images

```csharp
public class InlineAttachment(string filename, string cid)
{
    public string FileName { get; set; }   // full path of the image
    public string CID { get; set; }        // content id referenced from HTML (auto-generated if blank)
}
```

```csharp
email.InlineAttachments.Add(new InlineAttachment(@"C:\images\logo.png", "logo"));
email.HtmlBody = "<img src=\"cid:logo\" alt=\"Logo\" /><p>Welcome!</p>";
```

### Supporting classes

| Class | Members | Purpose |
|---|---|---|
| `StringAttachment` | `required string Content`, `required string Name`, `required Encoding ContentEncoding`, `required string MediaType` | Used by `CreateAttachmentFromString`. |
| `Header` | `required string Name`, `required string Value` | A custom email header. |
| `DSNSmtpClient` | Inherits `MailKit.Net.Smtp.SmtpClient` | Asks the server for **failure** delivery-status notifications, using the Message-Id as the envelope id. Not used by `EmailHelper`; use it directly with MailKit if you need bounce tracking. |

### Testing emails without sending

In `appsettings.Development.json`:

```json
{ "Email": { "SendEnabled": "false", "SmtpPickupDirectory": "C:\\Temp\\Mail" } }
```

Each `Send()` now creates a `.eml` file you can open in Outlook or any mail client. Alternatively set `"Override": "you@example.com"` to receive every message yourself.

---

## 15. ErrorHandler

`public class ErrorHandler` — builds a detailed HTML report of an exception and emails it to the addresses in `ExceptionReport:*` settings (sent through `EmailHelper`, so `Email:*` settings apply too).

### Methods

```csharp
public void ReportError(Exception ex, string appName, HttpContext httpContext, IHostEnvironment? environment = null)
public void ReportError(Exception ex, string appName, IHostEnvironment? environment = null)
```

| Parameter | Description |
|---|---|
| `ex` | The exception. If `null` the method does nothing. |
| `appName` | Shown in the subject `AN EXCEPTION HAS OCCURRED - {appName}`. Blank → the app domain name. |
| `httpContext` | The current web request (first overload). Adds request details. |
| `environment` | Optional; adds environment name, application name and content root. |

The method **never throws** — if the report can't be built a simpler fallback report is sent, and email failures are written to `System.Diagnostics.Trace`. If no recipients are configured nothing is sent.

### What the report contains

- **System**: machine, OS, .NET version, process, timestamps, and environment (if supplied).
- **Request** (web overload): method, URL, IPs/ports, user agent, referrer, endpoint, user name, authentication status, roles, then all headers, query string values, cookies and form values.
- **Exception**: type, message, source, stack trace, `Data` entries, every inner exception, and every inner exception of an `AggregateException`.
- **Diagnostic activity**: trace/span ids from `Activity.Current`.
- **Database specifics**: details of any `SqlException` or `OleDbException` (server, line number, error numbers).

**Security:** values whose key contains `password`, `pwd`, `token`, `secret`, `auth`, `cookie`, `apikey`, `bearer`, `jwt`, `creditcard`, `cardnumber`, `cvv`, `ssn`, `viewstate`, and similar are replaced with `*** REDACTED ***`. Long values are truncated. Requests from common bots (Googlebot, Bingbot, …) get an extra `X-Bot: ItsABot` email header so you can filter them in your mail client.

### Examples

In an MVC controller:

```csharp
public class OrdersController(IHostEnvironment env) : Controller
{
    private readonly ErrorHandler _errors = new();

    public IActionResult Submit(OrderModel model)
    {
        try
        {
            SaveOrder(model);
            return RedirectToAction("Thanks");
        }
        catch (Exception ex)
        {
            _errors.ReportError(ex, "Web Store", HttpContext, env);
            return View("Error");
        }
    }
}
```

In a background service or console app (no web request):

```csharp
catch (Exception ex)
{
    new ErrorHandler().ReportError(ex, "Nightly Import");
}
```

Using ASP.NET Core's built-in exception handler so **every** unhandled error is reported:

```csharp
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    if (feature?.Error != null)
        new ErrorHandler().ReportError(feature.Error, "Web Store", context, app.Environment);

    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("Something went wrong. Please try again later.");
}));
```

---

## 16. Exception Middleware

Namespace `FCS.Utilities.Middleware`.

```csharp
public sealed class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger) : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next);
}

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder);
}
```

`InvokeAsync` calls the rest of the pipeline and, if an exception escapes, logs it with `LogCritical`.

Because it implements `IMiddleware`, it **must be registered with dependency injection** before use:

```csharp
using FCS.Utilities.Middleware;

builder.Services.AddTransient<ExceptionHandlerMiddleware>();   // required
var app = builder.Build();
app.UseGlobalExceptionHandler();                                // add early in the pipeline
```

> ⚠️ In its current form the middleware only **logs** the error and then **swallows** it: it does not email a report, does not set a 500 status code, and writes no response. The client receives an empty `200 OK`. Use the `UseExceptionHandler` + `ErrorHandler` pattern in the previous section if you need proper error responses.

---

## 17. DBHelper

`public static class DBHelper` — run SQL Server stored procedures or SQL text and get a `DataSet` back. Results are placed in a table named **`"Results"`**.

### Members

| Member | Returns | Default timeout |
|---|---|---|
| `DBMinValue` (property) | `DateTime` — `1753-01-01`, the smallest SQL Server `datetime` | — |
| `RunStoredProc(string procName, SqlParameter[]? parameters = null, int timeOut = 15, string connectionStringKey = "")` | `DataSet` | 15 s |
| `RunStoredProcAsync(...)` same parameters | `Task<DataSet>` | 15 s |
| `RunSqlCommand(string command, int timeOut = 30, string connectionStringKey = "")` | `DataSet` | 30 s |
| `RunSqlCommandAsync(...)` same parameters | `Task<DataSet>` | 30 s |

| Parameter | Description |
|---|---|
| `procName` / `command` | Stored procedure name, or SQL text. Blank → empty `DataSet`. |
| `parameters` | `SqlParameter` array for the stored procedure. |
| `timeOut` | Command timeout in seconds. |
| `connectionStringKey` | Name under `ConnectionStrings` in appsettings. Blank → see note below. |

**Returns** an **empty** `DataSet` (no tables!) when the name/command or the connection string is missing. Database errors are thrown as `SqlException` — except `RunSqlCommand` in **Release** builds, which swallows them and returns an empty `DataSet`.

### Examples

```csharp
using Microsoft.Data.SqlClient;

// Stored procedure with parameters (async — preferred in web apps)
SqlParameter[] p =
[
    new("@CustomerId", System.Data.SqlDbType.Int) { Value = 42 },
    new("@FromDate", System.Data.SqlDbType.Date) { Value = new DateTime(2024, 1, 1) }
];

DataSet ds = await DBHelper.RunStoredProcAsync("dbo.GetOrders", p);

if (ds.Tables.Count > 0)
{
    foreach (DataRow row in ds.Tables["Results"]!.Rows)
        Console.WriteLine(DataHelpers.GetDataString(row, "OrderNumber"));
}

// Use a different connection string
DataSet report = DBHelper.RunStoredProc("dbo.MonthlySales", connectionStringKey: "Reporting", timeOut: 120);

// Plain SQL (no parameters supported — never concatenate user input!)
DataSet counts = await DBHelper.RunSqlCommandAsync("SELECT COUNT(*) AS Total FROM dbo.Orders");
```

> ⚠️ **Connection string caching:** the last connection string used is stored in a static field. A call with `connectionStringKey: "Reporting"` makes **later calls with no key use "Reporting" too**, not "Default". Always pass the key explicitly if your app uses more than one database. (This also means concurrent requests can interfere with each other.)
>
> ⚠️ `RunSqlCommand*` has no parameter support. Never build the SQL text from user input — use a stored procedure with `SqlParameter`s instead.

---

## 18. DataHelpers

`public static class DataHelpers` — read a typed value from a `DataRow`, turning `DBNull` into a safe default.

| Method | Returns | When the column is `DBNull` |
|---|---|---|
| `GetDataString(DataRow, string columnName)` | `string` | `""` |
| `GetDataDateTime(DataRow, string)` | `DateTime` | `DateTime.MinValue` |
| `GetDataDateTime2(DataRow, string)` | `DateTime` | `DateTime.MinValue` (identical to `GetDataDateTime`) |
| `GetDataDateOnly(DataRow, string)` | `DateOnly?` | `null` |
| `GetDataBool(DataRow, string)` | `bool` | `false` |
| `GetDataInteger(DataRow, string)` | `int` | `0` |
| `GetDataDecimal(DataRow, string)` | `decimal` | `0` |
| `GetDataDouble(DataRow, string)` | `double` | `0` |
| `GetDataFloat(DataRow, string)` | `float` | `0` |
| `GetDataGuid(DataRow, string)` | `Guid` | `Guid.Empty` |
| `GetDbValue<T>(DataRow, string)` | `T` | Throws — uses `Convert.ChangeType` (converts between compatible types) |

These methods use a direct **cast**, so the column's .NET type must match exactly. Reading a SQL `decimal` column with `GetDataInteger` throws `InvalidCastException`. A missing column name throws `ArgumentException`.

| SQL Server type | Use |
|---|---|
| `int` | `GetDataInteger` |
| `bigint` | `GetDbValue<long>` |
| `decimal` / `money` | `GetDataDecimal` |
| `float` | `GetDataDouble` |
| `real` | `GetDataFloat` |
| `bit` | `GetDataBool` |
| `datetime` / `datetime2` | `GetDataDateTime` |
| `date` | `GetDataDateOnly` |
| `uniqueidentifier` | `GetDataGuid` |
| `varchar` / `nvarchar` | `GetDataString` |

```csharp
DataRow row = ds.Tables["Results"]!.Rows[0];

string name       = DataHelpers.GetDataString(row, "CustomerName");
int qty           = DataHelpers.GetDataInteger(row, "Quantity");
decimal price     = DataHelpers.GetDataDecimal(row, "UnitPrice");
DateOnly? shipped = DataHelpers.GetDataDateOnly(row, "ShipDate");
long big          = DataHelpers.GetDbValue<long>(row, "Quantity");   // converts int → long
```

---

## 19. Data Mapping (`DataNamesMapper`)

Turns `DataTable` rows into C# objects automatically, matching column names to property names. The `[DataNames]` attribute lets one property accept several possible column names — useful when different procedures or imports use different column names.

### `DataNamesAttribute`

Namespace `FCS.Utilities.DataMapping.Attributes`.

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class DataNamesAttribute : Attribute
{
    public DataNamesAttribute();
    public DataNamesAttribute(params string[] valueNames);
    public List<string> ValueNames { get; set; }
}
```

List the column names in priority order. The **first column that exists and is not `DBNull`** is used.

### `DataNamesMapper<TEntity>`

Namespace `FCS.Utilities.DataMapping`. `TEntity` must be a class with a public parameterless constructor.

| Method | Returns | Behaviour |
|---|---|---|
| `Map(DataTable table)` | `IEnumerable<TEntity>` | One object per row. Maps **all** public properties: properties with `[DataNames]` use those names, others use their own property name. |
| `Map(DataRow row)` | `TEntity` | ⚠️ Maps **only** properties that have `[DataNames]`. |
| `Map(DataRow row, TEntity entity, Type type)` | `TEntity` | Same as above, filling an existing object. |

### Example

```csharp
using FCS.Utilities.DataMapping;
using FCS.Utilities.DataMapping.Attributes;

public class Employee
{
    [DataNames("EmployeeID", "EmpNo")]     // accepts either column
    public int Id { get; set; }

    [DataNames("FirstName", "First Name")]
    public string? FirstName { get; set; }

    [DataNames("HireDate")]
    public DateTime HireDate { get; set; }

    public decimal Salary { get; set; }    // no attribute: matched by property name "Salary"
}

var table = new DataTable();
table.Columns.Add("EmpNo", typeof(int));
table.Columns.Add("First Name", typeof(string));
table.Columns.Add("HireDate", typeof(string));
table.Columns.Add("Salary", typeof(decimal));
table.Rows.Add(7, "  Ann  ", "01152024", 50000m);   // HireDate in MMddyyyy format
table.Rows.Add(8, DBNull.Value, "2024-02-01", 60000m);

List<Employee> staff = new DataNamesMapper<Employee>().Map(table).ToList();
// staff[0]: Id=7, FirstName="Ann" (trimmed), HireDate=2024-01-15, Salary=50000
// staff[1]: Id=8, FirstName=null,          HireDate=2024-02-01, Salary=60000

Employee one = new DataNamesMapper<Employee>().Map(table.Rows[0]);
// one.Salary == 0  ← single-row Map ignores properties without [DataNames]
```

Combine it with `DBHelper`:

```csharp
DataSet ds = await DBHelper.RunStoredProcAsync("dbo.GetEmployees");
List<Employee> employees = ds.Tables.Count > 0
    ? new DataNamesMapper<Employee>().Map(ds.Tables["Results"]!).ToList()
    : [];
```

### Supported property types

| Property type | Conversion |
|---|---|
| `string` | `ToString()` then `Trim()` |
| `char` | first character |
| `bool`, `bool?` | `StringToBoolean()` (`"yes"`, `"1"`, `"true"` … → `true`) |
| `int`, `int?`, `long` | `Parse` — **throws** if the text isn't a whole number |
| `decimal` | `Parse` — **throws** if invalid |
| `double`, `double?` | `TryParse` — invalid values ignored |
| `DateTime`, `DateTime?`, `DateOnly`, `DateOnly?` | `TryParse`, then `MMddyyyy` — invalid values ignored |
| `Guid` | `TryParse` (also `{B}` format) — invalid values ignored |

Other types (`float`, `short`, `long?`, `decimal?`, enums, …) are **silently skipped**. `DBNull` values are skipped, so the property keeps its default.

### `AttributeHelper`

```csharp
public static List<string> GetDataNames(Type type, string propertyName)
```

Returns the `[DataNames]` list for a property, or a one-item list containing the property name if it has no attribute (or doesn't exist).

```csharp
AttributeHelper.GetDataNames(typeof(Employee), "Id");      // ["EmployeeID", "EmpNo"]
AttributeHelper.GetDataNames(typeof(Employee), "Salary");  // ["Salary"]
```

### `PropertyMapHelper`

```csharp
public static void Map(Type type, DataRow row, PropertyInfo prop, object entity)
```

The low-level method used by `DataNamesMapper`: finds the right column for `prop` and sets the value on `entity`. You normally don't call it directly. (If a property has `[DataNames]` with **no** names, every column in the row is tried and the first non-null one is used.)

---

## 20. CSV

`public static class CSV` (uses [CsvHelper](https://joshclose.github.io/CsvHelper/)).

### `GetCsvDataAsync<T>`

```csharp
public static Task<List<T>> GetCsvDataAsync<T>(string url)
```

Downloads a CSV file from a URL and converts each row to a `T`. The first row must be a header whose names match `T`'s property names. Fields are trimmed; bad data, missing fields and header mismatches are ignored.

**Returns:** the list of records; an **empty list** if the file is empty or can't be parsed.
**Throws:** `HttpRequestException` if the download fails (network errors are *not* swallowed).

```csharp
public class Rate
{
    public string Currency { get; set; } = "";
    public decimal Value { get; set; }
}

List<Rate> rates = await CSV.GetCsvDataAsync<Rate>("https://example.com/rates.csv");
if (rates.Count == 0) logger.LogWarning("No rates were loaded");
```

### `StripCharsForCsv`

```csharp
public static string StripCharsForCsv(string input)
```

Replaces line breaks and commas with spaces so a value can't break a hand-written CSV line.

```csharp
CSV.StripCharsForCsv("a,b\r\nc");   // "a b c"
```

---

## 21. JSON

`public static class JSON` plus two `System.Text.Json` converters.

### `GetJsonFromContentAsync<T>`

```csharp
public static Task<T[]> GetJsonFromContentAsync<T>(string uri, JsonSerializerOptions? serializedOptions = null)
```

GETs a URL whose response is a **JSON array** and deserializes it into `T[]`. Pass the **element type** as `T`.

**Returns:** the array, or an empty array after repeated failures. Writes diagnostic messages to the console.

```csharp
public record User(int Id, string Name);

var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
User[] users = await JSON.GetJsonFromContentAsync<User>("https://api.example.com/users", options);
```

> ⚠️ The retry loop can run **forever** in some cases (unknown host name, invalid JSON, unsupported content type). Don't call it against URLs you don't trust to be up; consider `HttpClient.GetFromJsonAsync<T[]>()` with a timeout instead.

### `GetJsonAsync`

```csharp
public static Task<string> GetJsonAsync(string uri)
```

Downloads the response body as a string. Throws `HttpRequestException` on failure.

```csharp
string json = await JSON.GetJsonAsync("https://api.example.com/status");
```

### `PostJsonHttpClient`

```csharp
public static Task PostJsonHttpClient(string uri, string content)
```

POSTs `content` as JSON. Throws `HttpRequestException` for non-success status codes.

> ⚠️ `content` is serialized **as a JSON string**, so `{"name":"John"}` is sent as `"{\"name\":\"John\"}"` (double-encoded). Most APIs expect an object, so for those use `HttpClient.PostAsJsonAsync(uri, myObject)` directly.

### `NumberAsStringConverter`

```csharp
public class NumberAsStringConverter : JsonConverter<JsonElement>
```

When serializing a `JsonElement`, writes every number (including inside arrays and objects) as a string. Reading is not supported (`NotImplementedException`).

```csharp
var options = new JsonSerializerOptions { Converters = { new NumberAsStringConverter() } };
JsonElement el = JsonDocument.Parse("{\"id\":5,\"items\":[1,2.5],\"name\":\"x\"}").RootElement;

JsonSerializer.Serialize(el, options);   // {"id":"5","items":["1","2.5"],"name":"x"}
```

It only affects values of type `JsonElement`; normal classes and numbers are not changed.

### `MetaDataConverter`

```csharp
public class MetaDataConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options);
    public override bool CanConvert(Type typeToConvert);   // always true
}
```

Lets a "buddy" metadata class (via `[MetadataType]`) supply `[JsonPropertyName]` names for a class you can't edit — for example an Entity Framework class that is regenerated from the database.

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

// Generated class — don't edit
public partial class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = "";
}

// Your partial + metadata class
[MetadataType(typeof(CustomerMeta))]
[JsonConverter(typeof(MetaDataConverter))]
public partial class Customer { }

public class CustomerMeta
{
    [JsonPropertyName("id")]        public int CustomerId { get; set; }
    [JsonPropertyName("full_name")] public string Name { get; set; } = "";
}

string json = JsonSerializer.Serialize(new Customer { CustomerId = 1, Name = "Ann" });
// {"CustomerId":1,"Name":"Ann","id":1,"full_name":"Ann"}

Customer? c = JsonSerializer.Deserialize<Customer>("{\"id\":7,\"full_name\":\"Bob\"}");
// c.CustomerId == 7, c.Name == "Bob"
```

> ⚠️ Apply it with `[JsonConverter(typeof(MetaDataConverter))]` on the class. Do **not** add it to `JsonSerializerOptions.Converters` — `CanConvert` returns `true` for every type, so it would try to handle strings and ints too.
> Note that output contains both the original and the metadata names, and while reading, a value that fails to deserialize and ends in a character such as `L` (`"12L"`) is converted with an unusual rule (`12 + 1000`). Test with your own data before relying on it.

---

## 22. Excel Export — `CreateExcelFile`

Namespace **`ExportToExcel`**. Based on Mike Gledhill's OpenXML exporter. Creates `.xlsx` files with no Excel installation required.

| Method | Returns | Behaviour |
|---|---|---|
| `CreateExcelDocument<T>(List<T> list, string xlsxFilePath)` | `bool` | One sheet from a list; columns = public properties. |
| `CreateExcelDocument(DataTable dt, string xlsxFilePath)` | `bool` | One sheet named after `dt.TableName`. |
| `CreateExcelDocument(DataSet ds, string excelFilename)` | `bool` | One sheet per table. |
| `StreamExcelDocument<T>(List<T> list, string filename)` | `FileStreamResult` | For ASP.NET Core: builds the file and returns it as a download. |
| `ListToDataTable<T>(List<T> list)` | `DataTable` | Converts a list to a `DataTable` (nullable properties become nullable columns). |
| `GetExcelColumnName(int columnIndex)` | `string` | 0-based index → column letters. |

The `CreateExcelDocument` methods return `false` (and write to `Trace`) instead of throwing.

Formatting applied automatically: a dark-grey header row with white bold text, column width 20, integers as `#,##0`, decimals/doubles as `#,##0.00`, dates as `dd/MMM/yyyy` (or `dd/MMM/yyyy hh:mm:ss` when there's a time). Control characters that would corrupt the file are removed.

```csharp
using ExportToExcel;

var orders = new List<Order>
{
    new() { Number = "A-100", Total = 19.99m, Placed = DateTime.Today },
    new() { Number = "A-101", Total = 5.00m,  Placed = DateTime.Today.AddDays(-1) }
};

bool ok = CreateExcelFile.CreateExcelDocument(orders, @"C:\Reports\orders.xlsx");

// From DBHelper results
DataSet ds = await DBHelper.RunStoredProcAsync("dbo.MonthlySales");
CreateExcelFile.CreateExcelDocument(ds, @"C:\Reports\sales.xlsx");   // sheet name "Results"

CreateExcelFile.GetExcelColumnName(0);    // "A"
CreateExcelFile.GetExcelColumnName(26);   // "AA"
CreateExcelFile.GetExcelColumnName(702);  // "AAA"
```

ASP.NET Core download:

```csharp
public IActionResult Export()
{
    List<Order> orders = _repo.GetOrders();
    return CreateExcelFile.StreamExcelDocument(orders, "orders.xlsx");
}
```

---

## 23. QRCode

`public class QRCode` (uses ZXing.Net).

| Member | Behaviour |
|---|---|
| `QRCode()` | Empty constructor. |
| `QRCode(string imageFileName, string content)` | Creates the QR image immediately (same as `Create`). |
| `static void Create(string imageFileName, string content, int width = 500, int height = 500)` | Saves a QR code image to `Path.Combine(Function.WebPath, imageFileName)`. Format is chosen from the extension (`.png`, `.jpg`, …). |
| `static void CreateContact(string imageFileName, string name, string telephone, string email, string address, string notes)` | QR code containing a **MECARD** contact. Empty parts are skipped. |
| `static string Read(string? imageFileName = null)` | Decodes a QR code from a **full** image path. Returns `""` if the file doesn't exist or no code is found. |

**Throws:** `Create` throws `ArgumentException` for blank arguments or a size under 100 pixels, and `IOException` if the image can't be saved. `Read` throws `IOException` if the image can't be opened.

```csharp
Function.WebPath = @"C:\inetpub\wwwroot\MyApp\wwwroot\qr";

QRCode.Create("site.png", "https://example.com");                   // saved under WebPath
QRCode.Create(@"C:\Temp\big.png", "https://example.com", 800, 800); // absolute path also works

QRCode.CreateContact("jane.png", "Doe,Jane", "519-555-1234", "jane@example.com",
    "123 Main St, London ON", "Met at conference");

string text = QRCode.Read(@"C:\Temp\big.png");   // "https://example.com"
```

---

## 24. TaskScheduler

`public static class TaskScheduler` — creates and removes tasks in the **Windows Task Scheduler**.

> The name clashes with `System.Threading.Tasks.TaskScheduler`. If you also have `using System.Threading.Tasks;`, write the full name `FCS.Utilities.TaskScheduler`.

### `AddTask`

```csharp
public static bool AddTask(string taskPath, string taskDescription, DateTime execTime,
    string exePath, string? workingDirectory = null)
```

Creates a task that runs `exePath` **every day** at the time of day in `execTime`, starting on that date. The task runs as the current user, only while they are logged on, with normal (non-admin) rights.

| Parameter | Description |
|---|---|
| `taskPath` | Task name in the root folder of Task Scheduler. |
| `taskDescription` | Description shown in Task Scheduler. |
| `execTime` | First run date and daily time. |
| `exePath` | Program to run. |
| `workingDirectory` | Optional start folder. |

**Returns:** `false` if a task with that name already exists; otherwise `true`.

```csharp
bool created = FCS.Utilities.TaskScheduler.AddTask(
    "MyApp Nightly Backup",
    "Backs up the MyApp database",
    DateTime.Today.AddHours(2),                 // 2:00 AM daily, starting today
    @"C:\Tools\backup.exe",
    @"C:\Tools");
```

### `RemoveTask`

```csharp
public static void RemoveTask(string taskName)
```

Deletes the **first** task in the root folder whose name matches `taskName` as a **regular expression**.

> ⚠️ Throws if no task matches. Because the name is a regex, characters like `.`, `(` or `+` have special meaning — use `Regex.Escape(name)` for exact names.

```csharp
FCS.Utilities.TaskScheduler.RemoveTask(System.Text.RegularExpressions.Regex.Escape("MyApp Nightly Backup"));
```

---

## 25. Cookie

`public static class Cookie` — extension methods on ASP.NET Core `HttpRequest` / `HttpResponse`.

| Method | Behaviour |
|---|---|
| `Set(this HttpResponse response, string key, string value, int? expireTime = null)` | Adds a cookie. `expireTime` is in **minutes**; `null` = session cookie (deleted when the browser closes). |
| `Get(this HttpRequest request, string key)` | Returns the value or `null`. |
| `Remove(this HttpResponse response, string key)` | Deletes the cookie. |
| `Exists(this HttpRequest request, string key)` | `true` if the request has the cookie. |

```csharp
// In a controller
Response.Set("theme", "dark", expireTime: 60 * 24 * 30);   // 30 days

if (Request.Exists("theme"))
{
    string? theme = Request.Get("theme");
}

Response.Remove("theme");
```

> Cookies are created with default options (not `HttpOnly`, not `Secure`). Don't store sensitive data in them; for anything important create `CookieOptions` yourself.

---

## 26. GEO

```csharp
public static double GEO.Distance(double lat1, double lat2, double lon1, double lon2, bool Miles = true)
```

Great-circle distance between two points using the Haversine formula (earth radius 3,956 mi / 6,371 km).

> ⚠️ Note the parameter order: **both latitudes first, then both longitudes**. Use named arguments to avoid mistakes.

```csharp
// London, ON → Toronto, ON
double miles = GEO.Distance(lat1: 42.9849, lat2: 43.6532, lon1: -81.2453, lon2: -79.3832);              // ≈ 104.3
double km    = GEO.Distance(lat1: 42.9849, lat2: 43.6532, lon1: -81.2453, lon2: -79.3832, Miles: false); // ≈ 168
```

---

## 27. AppInfo

`public static class AppInfo` — information about the assembly and process.

| Member | Type | Value |
|---|---|---|
| `Title` | `string` | `[AssemblyTitle]` |
| `Product` | `string` | `[AssemblyProduct]` |
| `Copyright` | `string` | `[AssemblyCopyright]` |
| `CompanyName` | `string` | `[AssemblyCompany]` |
| `Version` | `string` | Assembly version, e.g. `"1.0.3.0"` |
| `ExeName` | `string` | Current process name (e.g. `"MyApp"`) |
| `ExePath` | `string` | Folder containing the assembly |
| `ApplicationName` | `string` | Same as `ExeName` |
| `LayoutFileName` | `string` (static readonly) | `{ExePath}\{ExeName}.layout` |
| `SettingFileName` | `string` (static readonly) | `{ExePath}\{ExeName}.cfg` |
| `LogFileName` | `string` (static readonly) | `{ExePath}\{ExeName}.log` |

> ⚠️ `Title`, `Product`, `Copyright`, `CompanyName`, `Version` and `ExePath` describe the **FCS.Utilities library itself**, not your application (they use `Assembly.GetExecutingAssembly()`). For your own app's version use `Assembly.GetEntryAssembly()?.GetName().Version`. `ExeName` does reflect your application.

```csharp
Console.WriteLine($"Running {AppInfo.ExeName}, logging to {AppInfo.LogFileName}");
Console.WriteLine($"FCS.Utilities version {AppInfo.Version}");
```

---

## 28. ProcessControl

```csharp
public static bool ProcessControl.IsReleaseBuild()
```

Returns `true` if compiled in Release mode, `false` in Debug.

> ⚠️ Because the check is compiled into FCS.Utilities, it reports how **the library** was built, not your app. In your own code use `#if DEBUG`, or `app.Environment.IsDevelopment()` in ASP.NET Core.

---

## 29. Twitter Helpers

```csharp
public static Tuple<bool, string> DoesTwitterUserNameExists(string userName)
public static string BuildTwitterLink(string userName)
```

| Method | Behaviour |
|---|---|
| `DoesTwitterUserNameExists` | Calls the Twitter/X v2 API. Returns `(true, displayName)` if the user exists, `(false, errorDetail)` if not, or `(false, "Twitter Not Available")` on any failure. Blocks the calling thread. |
| `BuildTwitterLink` | `"@dotnet"` → `"https://twitter.com/dotnet"`. |

```csharp
BuildTwitterLink("@dotnet");   // "https://twitter.com/dotnet"

var (exists, info) = DoesTwitterUserNameExists("dotnet");
```

> ⚠️ `DoesTwitterUserNameExists` uses an API bearer token **hard-coded in the source**. It may no longer be valid, and secrets should not live in source code. Treat this method as deprecated.

---

## 30. Models

### `SelectListItem`

Namespace `FCS.Utilities`. Returned by all drop-down list methods.

| Property | Type | Description |
|---|---|---|
| `Text` | `string` (required) | Text shown to the user. |
| `Value` | `string` (required) | Value submitted with the form. |
| `Selected` | `bool` | Pre-selected item. |
| `Disabled` | `bool` | Item can't be chosen. |
| `Group` | `string` | Group name (e.g. "Province"/"Territory"). Defaults to `""`. |

> This is **not** `Microsoft.AspNetCore.Mvc.Rendering.SelectListItem`. If you import both namespaces you'll get an "ambiguous reference" error — use an alias (see [Drop-Down List Data](#10-drop-down-list-data)).

### `Territory`

Namespace `FCS.Utilities.Models`.

| Property | Type | Example |
|---|---|---|
| `ID` | `string` (required) | `"ON"` |
| `Name` | `string` (required) | `"Canada - Ontario"` |
| `Group` | `string` (required) | `"Province"` |

### `Country`

Namespace `FCS.Utilities`. `ID` (`"CA"`) and `Name` (`"Canada"`), both required.

### `PostalCodeValidationResult`

```csharp
public readonly record struct PostalCodeValidationResult(bool IsValid, string FormattedPostalCode);
```

Returned by `ValidateCanadianPostalCode`. `FormattedPostalCode` is `""` when `IsValid` is `false`. Can be deconstructed:

```csharp
var (isValid, formatted) = "k1a0b1".ValidateCanadianPostalCode();
```

### `IEncryptor`

See [Passwords](#13-passwords--iencryptor).

---

## 31. Files Not Compiled

These source files are in the repository but are **excluded from the build** in `FCS.Utilities.csproj`, so their classes are **not** available:

| File | Class | Note |
|---|---|---|
| `Toasty.cs` | `Toasty` | Windows toast notifications; marked "not production ready". |
| `Functions\Passwords.cs` | `Encryptor` | Password hashing; see [Passwords](#13-passwords--iencryptor) for an alternative. |

---

## 32. Known Issues & Gotchas

These were found while writing this guide. They are listed here so you can avoid them until they are fixed.

| # | Area | Problem | Workaround |
|---|---|---|---|
| 1 | `StringUtilities.SplitCamelCase` | Returns garbage such as `"Firs$1 $2ame"` (the regex uses `ExplicitCapture`, so `$1`/`$2` are not replaced). | Don't use. `Regex.Replace(s, "(?<=[a-z])(?=[A-Z])", " ")` works for simple cases. |
| 2 | `GetDayInMonth` | Returns `null` in week 1 when the wanted weekday is the day before the month's first weekday (e.g. 1st Sunday of June 2026). | Use your own calculation or check for `null`. |
| 3 | `DateHelper.ToShortDateWithFullDigitsString` | Wrong output for cultures whose pattern already uses `dd`/`MM` (en-CA, en-GB, …). | Use `ToString("yyyy-MM-dd")` etc. |
| 4 | `IEncryptor` | Interface has no implementation (`Encryptor` is excluded from the build). | Use `Rfc2898DeriveBytes.Pbkdf2` or ASP.NET Core Identity's `PasswordHasher`. |
| 5 | `DBHelper` | Last connection string is cached in a static field; later calls without a key reuse it. | Always pass `connectionStringKey`. |
| 6 | `DBHelper.RunSqlCommand` | In Release builds, SQL errors are swallowed and an empty `DataSet` is returned. | Use `RunSqlCommandAsync`, or check `Tables.Count`. |
| 7 | `ExceptionHandlerMiddleware` | Logs and swallows exceptions, returns empty `200 OK`, sends no email. | Use `UseExceptionHandler` + `ErrorHandler`. |
| 8 | `JSON.GetJsonFromContentAsync` | Retry loop can be infinite for unknown hosts / invalid JSON. | Use `HttpClient.GetFromJsonAsync` with a timeout. |
| 9 | `JSON.PostJsonHttpClient` | Sends the content double-encoded as a JSON string. | Use `HttpClient.PostAsJsonAsync(uri, obj)`. |
| 10 | `AppInfo`, `ProcessControl` | Report the library's assembly/build, not your app's. | Use `Assembly.GetEntryAssembly()` / `#if DEBUG`. |
| 11 | `StringUtilities.EncryptPassword` | Only Base64 encodes — not secure. | Use PBKDF2 (see section 13). |
| 12 | `StringUtilities.ReturnValidSQLInString` | SQL injection risk; also doubles quotes on already-quoted input. | Use parameters / table-valued parameters. |
| 13 | `StringUtilities.GetQueryStringParameters` | Throws for a key without `=`. | Use `QueryHelpers.ParseQuery`. |
| 14 | `ExtractUrl(s)` | Single-quoted attributes are not found. | Use an HTML parser. |
| 15 | `DataNamesMapper.Map(DataRow)` | Ignores properties without `[DataNames]`, unlike `Map(DataTable)`. | Add `[DataNames]` or use `Map(table)`. |
| 16 | `ProvinceStateDropDownList` | `Group` is not set on the returned items. | Use `ProvinceDropDownList` / `StateDropDownList` if you need groups. |
| 17 | `Function.DoesTwitterUserNameExists` | Hard-coded API token in source. | Treat as deprecated; rotate the token. |
| 18 | `TaskScheduler.RemoveTask` | Throws when nothing matches; name is a regex. | `Regex.Escape` and wrap in `try/catch`. |
| 19 | `IsValidFileName` | Allows reserved names with an extension (`CON.txt`). | Check `Path.GetFileNameWithoutExtension` against the reserved list too. |
| 20 | `TypeCasting.ToBoolFromString` | Any text starting with `t` is `true` (`"tomato"`). | Validate the input first. |

---

## 33. Dependencies

| Package | Version | Used for |
|---|---|---|
| CsvHelper | 33.1.0 | `CSV` |
| MailKit / MimeKit | 4.18.1 | `EmailHelper`, `IsValidEmailAddress` |
| Microsoft.Data.SqlClient | 7.1.1 | `DBHelper`, `ErrorHandler` |
| Microsoft.EntityFrameworkCore | 10.0.12 | (referenced) |
| System.Configuration.ConfigurationManager | 10.0.12 | Configuration |
| System.Data.OleDb | 10.0.12 | `ErrorHandler` (OleDb details) |
| TaskScheduler | 2.12.2 | `TaskScheduler` |
| ZXing.Net + ZXing.Net.Bindings.Windows.Compatibility | 0.16.11 / 0.16.16 | `QRCode` |
| DocumentFormat.OpenXml | 2.19.0 | `CreateExcelFile` |
| Microsoft.Toolkit.Uwp.Notifications | 7.1.3 | (`Toasty`, not compiled) |
| Microsoft.AspNetCore.App (framework) | — | `Cookie`, middleware, `ErrorHandler`, `StreamExcelDocument` |

---

*Copyright © Forest City Software 2018 – 2026.*
