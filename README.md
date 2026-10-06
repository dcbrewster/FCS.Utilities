# FCS.Utilities Library Documentation

## Overview

This library provides a comprehensive collection of utility classes and extension methods for common operations in .NET applications. It includes data mapping, email handling, error reporting, string manipulation, date/time utilities, QR code generation, task scheduling, toast notifications, and JSON processing.

- **Current version:** 1.0.3
- **Target framework:** `net10.0-windows`
- **Nullable reference types:** enabled
- **Package:** generated on build
- **Author:** Dave Brewster / Forest City Software

---

## Table of Contents

1. [Data Mapping Utilities](#data-mapping-utilities)
2. [Email Helper Class](#email-helper-class)
3. [Error Handler Class](#error-handler-class)
4. [String Utilities](#string-utilities)
5. [Date/Time Utilities](#date-time-utilities)
6. [Type Casting Utilities](#type-casting-utilities)
7. [QR Code Generation](#qr-code-generation)
8. [Task Scheduler Utilities](#task-scheduler-utilities)
9. [Toast Notifications](#toast-notifications)
10. [JSON Utilities](#json-utilities)

---

## Data Mapping Utilities

### AttributeHelper Class

**Purpose:** Provides utility methods for retrieving custom attribute information from types and their members.

#### Method: `GetDataNames`

```csharp
public static List<string> GetDataNames(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, 
    string propertyName)
```

**Description:** Retrieves alternative data names for a property as defined by the `DataNamesAttribute`. If no attribute is found, returns the property name itself.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `type` | `Type` | The class type to search |
| `propertyName` | `string` | Name of the property to look for |

**Returns:** List of data names (or just the property name if not found)

**Example Usage:**

```csharp
// Define a custom attribute
[AttributeUsage(AttributeTargets.Property)]
public class DataNamesAttribute : Attribute
{
    public string[] ValueNames { get; set; }
}

// Apply to a class
public class Customer
{
    [DataNames("CustomerID", "ClientID")]
    public int Id { get; set; }
    
    [DataNames("FirstName", "First Name")]
    public string FirstName { get; set; }
}

// Usage
var dataNames = AttributeHelper.GetDataNames(typeof(Customer), "Id");
// Returns: ["CustomerID", "ClientID"]

var dataNames2 = AttributeHelper.GetDataNames(typeof(Customer), "NonExistentProperty");
// Returns: ["NonExistentProperty"]
```

---

### DataNamesMapper Class

**Purpose:** Maps data from a DataTable to entities using custom attributes.

#### Method: `Map` (Single Row)

```csharp
public TEntity Map(DataRow row, TEntity entity, Type type)
```

**Description:** Maps a single DataRow to an entity instance.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `row` | `DataRow` | The data row to map |
| `entity` | `TEntity` | Entity instance to populate |
| `type` | `Type` | Entity type |

**Returns:** Populated entity instance

**Example Usage:**

```csharp
public class Employee
{
    [DataNames("EmployeeID", "EmpNo")]
    public int Id { get; set; }
    
    [DataNames("FirstName", "First Name")]
    public string FirstName { get; set; }
}

// Map single row
DataRow row = table.Rows[0];
var employee = new DataNamesMapper<Employee>().Map(row, new Employee(), typeof(Employee));
```

#### Method: `Map` (DataTable to List)

```csharp
public IEnumerable<TEntity> Map(DataTable table)
```

**Description:** Converts all rows in a DataTable to entity instances.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `table` | `DataTable` | Source data table |

**Returns:** Collection of mapped entities

---

### PropertyMapHelper Class

**Purpose:** Helper class for mapping DataRow values to entity properties based on custom attributes.

#### Method: `Map`

```csharp
public static void Map(
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, 
    DataRow row, 
    PropertyInfo prop, 
    object entity)
```

**Description:** Maps a single property value from a DataRow to an entity. Supports multiple column names via `DataNamesAttribute`.

---

## Email Helper Class

### EmailHelper Class

**Purpose:** Sends email messages using MailKit with support for attachments, headers, and SMTP configuration.

#### Key Properties

| Property | Description |
|----------|-------------|
| `FromAddress` | Sender's email address |
| `FromName` | Sender's display name |
| `Subject` | Email subject line |
| `TextBody` / `HtmlBody` | Message content |
| `ToAddressList` | Recipient addresses |
| `CCAddressList` | Carbon copy recipients |
| `BCCAddressList` | Blind carbon copy recipients |
| `Attachments` | File attachments |
| `SmtpHost` | SMTP server address |
| `SmtpPort` | SMTP port (default: 25) |
| `UseSSL` | Use SSL encryption |

#### Method: `Send()`

```csharp
public Tuple<bool, string> Send()
```

**Description:** Sends the email and returns success status with any error message.

**Returns:**

- Item1: Boolean indicating success/failure
- Item2: Error message (if failed) or "Success"

**Example Usage:**

```csharp
var email = new EmailHelper
{
    FromAddress = "sender@example.com",
    FromName = "John Sender",
    Subject = "Test Email",
    TextBody = "Hello World!",
    IsHtml = false,
    ToAddressList = [new Tuple<string, string>("recipient@example.com", "Recipient")]
};

email.SmtpHost = "smtp.example.com";
email.SmtpPort = "587";
email.UseSSL = true;

var result = email.Send();
if (result.Item1)
{
    Console.WriteLine("Email sent successfully!");
}
else
{
    Console.WriteLine($"Failed: {result.Item2}");
}
```

---

## Error Handler Class

### ErrorHandler Class

**Purpose:** Generates detailed HTML error reports and sends them via email.

#### Method: `ReportError`

```csharp
public void ReportError(
    Exception ex, 
    string appName, 
    HttpContext httpContext, 
    IHostEnvironment? environment = null)
```

**Description:** Creates a comprehensive HTML error report including exception details, system information, HTTP request data (sanitized), and sends it via email.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `ex` | `Exception` | The exception to report |
| `appName` | `string` | Application name |
| `httpContext` | `HttpContext?` | HTTP context (optional) |
| `environment` | `IHostEnvironment?` | Hosting environment (optional) |

**Example Usage:**

```csharp
// In ASP.NET Core middleware or exception handler
try
{
    // Code that might throw
}
catch(Exception ex)
{
    var errorHandler = new ErrorHandler();
    
    // With HTTP context
    errorHandler.ReportError(
        ex, 
        "MyApplication", 
        HttpContext.Current, 
        HostingEnvironment);
    
    // Without HTTP context (e.g., background task)
    errorHandler.ReportError(ex, "MyApplication", null, HostingEnvironment);
}
```

**Features:**

- Sanitizes sensitive data (passwords, tokens, etc.)
- Includes request headers, query string, cookies
- Shows stack traces and inner exceptions
- Supports aggregate exception flattening
- Adds bot detection header for filtering

---

## String Utilities

### Extension Methods on `string`

#### `FormatName()` - Name Capitalization

```csharp
public static string? FormatName(this string? name)
```

**Description:** Formats names with proper capitalization, handling special cases like "Mc", "Mac", "O'", "D'".

**Example Usage:**

```csharp
Console.WriteLine("dave's".FormatName());    // "Dave's"
Console.WriteLine("mcfly".FormatName());     // "Mcfly"
Console.WriteLine("o'dell".FormatName());    // "O'Dell"
Console.WriteLine("mac jones ii".FormatName()); // "Mac Jones II"
```

---

#### `FormatTextForHtml()` - Line Breaks to HTML

```csharp
public static string FormatTextForHtml(this string? stringIn)
```

**Description:** Converts line breaks (CRLF, LF, CR) to `<br />` tags.

**Example Usage:**

```csharp
string text = "Line 1\r\nLine 2\nLine 3";
string html = text.FormatTextForHtml();
// Result: "<p>Line 1<br />Line 2<br />Line 3</p>"
```

---

#### `FormatCanadianPostalCode()` - Canadian Postal Code Formatting

```csharp
public static string FormatCanadianPostalCode(this string postalCode)
```

**Description:** Formats postal codes to "A9A 9A9" format.

**Example Usage:**

```csharp
Console.WriteLine("K1A0B1".FormatCanadianPostalCode());    // "K1A 0B1"
Console.WriteLine("k1a0b1".FormatCanadianPostalCode());    // "K1A 0B1" (uppercased)
Console.WriteLine("invalid".FormatCanadianPostalCode());   // ""
```

---

#### `FormatPhoneNumber()` - North American Phone Number Formatting

```csharp
public static string? FormatPhoneNumber(this string phone, int option = 0)
```

**Description:** Formats phone numbers to "123-456-7890" or "(123) 456-7890".

**Example Usage:**

```csharp
Console.WriteLine("1234567890".FormatPhoneNumber());    // "123-456-7890"
Console.WriteLine("1234567890".FormatPhoneNumber(1));  // "(123) 456-7890"
```

---

#### `ExtractUrl()` - Extract URLs from HTML

```csharp
public static string ExtractUrl(this string html, UrlType urlType = UrlType.All)
```

**Description:** Extracts URLs from HTML based on type (All, SrcOnly, HrefOnly, ImagesOnly).

**Example Usage:**

```csharp
string html = @"<img src='https://example.com/image.jpg' />";
Console.WriteLine(html.ExtractUrl(UrlType.SrcOnly));    // "https://example.com/image.jpg"
```

---

#### `ExtractUrls()` - Extract All URLs from HTML

```csharp
public static List<string> ExtractUrls(this string html, UrlType urlType = UrlType.All)
```

**Description:** Returns a list of all extracted URLs.

---

### String Sanitization Methods

| Method | Description | Example |
|--------|-------------|---------|
| `CharsOnly()` | Keep only alphanumeric characters | `"hello123!"` → `"hello123"` |
| `LettersOnly()` | Keep only letters and spaces | `"hello 123!"` → `"hello "` |
| `NumbersOnly()` | Keep only digits | `"123abc"` → `"123"` |
| `DecimalOnly()` | Keep numbers, dots, commas | `"1.234,56"` → `"1.234,56"` |
| `StripInvalidFileNameCharacters()` | Remove invalid filename chars | `"file:with:special@chars.txt"` → `"file with special chars.txt"` |

---

## Date/Time Utilities

### Extension Methods on `DateTime`

#### `ToUnixTimestampUtc()` - Convert to Unix Timestamp (UTC)

```csharp
public static long DateTimeToUnixTimestampUtc(this DateTime dateTime)
```

**Example Usage:**

```csharp
var timestamp = new DateTime(2024, 1, 15, 10, 30, 0).DateTimeToUnixTimestampUtc();
// Returns: 1705318200 (seconds since epoch)
```

---

#### `UnixTimeStampToLocalTimeZone()` - Convert from Unix Timestamp

```csharp
public static DateTime UnixTimeStampToLocalTimeZone(this long unixTimeStamp)
```

**Example Usage:**

```csharp
var localTime = 1705318200L.UnixTimeStampToLocalTimeZone();
// Returns: DateTime in local time zone
```

---

#### `ToShortDateWithFullDigitsString()` - Format as DD-MM-YYYY

```csharp
public static string ToShortDateWithFullDigitsString(this DateTime dt)
```

**Example Usage:**

```csharp
var formatted = new DateTime(2024, 1, 15).ToShortDateWithFullDigitsString();
// Returns: "15-01-2024"
```

---

### `GetDayInMonth()` - Find Specific Day of Week

```csharp
public static string? GetDayInMonth(
    int yearIn = 1970, 
    int monthIn = 1, 
    DayOfWeek dayIn = DayOfWeek.Monday, 
    int weekCount = 1)
```

**Description:** Finds the date of the Nth occurrence of a specific weekday.

**Example Usage:**

```csharp
// Get next Monday (weekCount=1)
var monday = new DateTime(2024, 1, 15).GetDayInMonth(year: 2024, month: 1, dayIn: DayOfWeek.Monday);
// Returns: "16"

// Get 3rd Friday of January 2024
var friday = new DateTime(2024, 1, 1).GetDayInMonth(year: 2024, month: 1, dayIn: DayOfWeek.Friday, weekCount: 3);
// Returns: "26"
```

---

### `BusinessDaysBetweenDates()` - Calculate Business Days

```csharp
public static int BusinessDaysBetweenDates(DateTime startDate, DateTime endDate)
```

**Description:** Counts business days (Monday-Friday) between two dates.

**Example Usage:**

```csharp
var days = new DateTime(2024, 1, 1).BusinessDaysBetweenDates(new DateTime(2024, 1, 31));
// Returns: 25 (excludes weekends and holidays)
```

---

## Type Casting Utilities

### Extension Methods on `object`

#### `ToInt()` - Convert to Integer

```csharp
public static int ToInt(object? i)
```

**Example Usage:**

```csharp
var result = "123".ToInt();    // Returns: 123
var result2 = null.ToInt();    // Returns: 0
```

---

#### `ToDouble()` - Convert to Double

```csharp
public static double ToDouble(object? i)
```

**Example Usage:**

```csharp
var result = "3.14".ToDouble();    // Returns: 3.14
var result2 = null.ToDouble();     // Returns: 0.0
```

---

#### `ToDecimal()` - Convert to Decimal

```csharp
public static decimal ToDecimal(object? i)
```

**Example Usage:**

```csharp
var result = "99.99".ToDecimal();    // Returns: 99.99m
var result2 = null.ToDecimal();      // Returns: 0.0m
```

---

#### `ToString()` - Convert to String with Options

```csharp
public static string ToString(object? i, int maxStringLength)
public static string ToString(object? i, int subStringStart, int maxStringLength)
```

**Example Usage:**

```csharp
var result = "Hello World".ToString(10);    // Returns: "" (too long)
var result2 = "Hello World".ToString(5, 3); // Returns: "Hel"
```

---

#### `ToBool()` - Convert to Boolean

```csharp
public static bool ToBool(object? i)
```

**Example Usage:**

```csharp
var result = "true".ToBool();    // Returns: true
var result2 = "yes".ToBool();    // Returns: true
var result3 = null.ToBool();     // Returns: false
```

---

#### `ToStringDecimal()` - Format Decimal String

```csharp
public static string ToStringDecimal(object? value, string format)
```

**Example Usage:**

```csharp
var formatted = 99.995m.ToStringDecimal("C");    // Returns: "$100.00"
```

---

## QR Code Generation

### QRCode Class

**Purpose:** Generates and reads QR codes using ZXing library.

#### Method: `Create()` - Generate QR Code

```csharp
public static void Create(string imageFileName, string content, int width = 500, int height = 500)
```

**Description:** Creates a QR code image file.

**Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `imageFileName` | `string` | Output filename |
| `content` | `string` | Content to encode |
| `width` | `int` | Image width (min: 100) |
| `height` | `int` | Image height (min: 100) |

**Example Usage:**

```csharp
QRCode.Create("qrcode.png", "https://example.com");
// QR code saved to qrcode.png
```

---

#### Method: `CreateContact()` - Contact QR Code

```csharp
public static void CreateContact(
    string imageFileName, 
    string name, 
    string telephone, 
    string email, 
    string address, 
    string notes)
```

**Description:** Creates a vCard-compatible QR code for contact information.

---

#### Method: `Read()` - Decode QR Code

```csharp
public static string Read(string? imageFileName = null)
```

**Description:** Reads and decodes content from a QR code image file.

**Example Usage:**

```csharp
var content = QRCode.Read("qrcode.png");
// Returns: "https://example.com"
```

---

## Task Scheduler Utilities

### `AddTask()` - Create Scheduled Task

```csharp
public static bool AddTask(
    string taskPath, 
    string taskDescription, 
    DateTime execTime, 
    string exePath, 
    string? workingDirectory = null)
```

**Description:** Creates a Windows Task Scheduler task.

**Example Usage:**

```csharp
// Schedule backup at 2:00 AM daily
bool success = TaskScheduler.AddTask(
    "BackupTask",
    "Daily Backup",
    new DateTime(DateTime.Now.Year, 1, 15, 2, 0, 0),
    @"C:\Scripts\backup.exe");

if (success) Console.WriteLine("Task created successfully!");
```

---

### `RemoveTask()` - Delete Scheduled Task

```csharp
public static void RemoveTask(string taskName)
```

**Description:** Removes a scheduled task.

---

## Toast Notifications

### Toasty Class

**Purpose:** Creates toast notifications for desktop applications using Microsoft.Toolkit.Uwp.Notifications.

#### Method: `Show()` - Display Toast

```csharp
public void Show(int delay = 1)
```

**Example Usage:**

```csharp
var toast = new Toasty();

toast.toast.AddText("Hello!")
           .AddButton(new ToastButton()
               .SetContent("OK")
               .SetBackgroundActivation())
           .Show(2); // Show after 2 seconds
```

---

## JSON Utilities

### `GetJsonFromContentAsync()` - Fetch JSON from URL

```csharp
public static async Task<T[]> GetJsonFromContentAsync<T>(string uri, JsonSerializerOptions? serializedOptions = null)
```

**Description:** Fetches and deserializes JSON array from a URL with retry logic.

**Example Usage:**

```csharp
var users = await JSON.GetJsonFromContentAsync<User[]>(
    "https://api.example.com/users");

// Returns: User[]
```

---

### `PostJsonHttpClient()` - Post JSON to Endpoint

```csharp
public static async Task PostJsonHttpClient(string uri, string content)
```

**Description:** Posts JSON content to a URL endpoint.

**Example Usage:**

```csharp
await JSON.PostJsonHttpClient(
    "https://api.example.com/users",
    @"{""name"": ""John"", ""age"": 30}");
```

---

### `NumberAsStringConverter` - Convert Numbers to Strings in JSON

```csharp
public class NumberAsStringConverter : JsonConverter<JsonElement>
```

**Description:** Converts numeric values to strings during JSON serialization.

**Example Usage:**

```csharp
var options = new JsonSerializerOptions { Converters = [new NumberAsStringConverter()] };
string json = JsonSerializer.Serialize(12345, options);
// Returns: "12345" (as string)
```

---

### `MetaDataConverter` - Merge MetaData with Original Class

```csharp
public class MetaDataConverter : JsonConverter<object>
```

**Description:** Combines properties from a meta file with the original class for JSON serialization.

**Example Usage:**

```csharp
// Original class
public class User { public string Name { get; set; } }

// Meta data (separate file)
[MetadataType(typeof(UserMeta))]
public class User { [JsonPropertyName("full_name")] public string FullName { get; set; } }

// Serialize with converter
string json = JsonSerializer.Serialize(user, options);
// Returns: {"name": "John", "full_name": "John Doe"}
```

---

## Summary of Key Features

| Feature | Category | Purpose |
|---------|----------|---------|
| `AttributeHelper` | Data Mapping | Retrieve custom attribute values |
| `DataNamesMapper` | Data Mapping | Map DataTable to entities |
| `EmailHelper` | Email | Send emails with attachments |
| `ErrorHandler` | Error Handling | Generate and send error reports |
| `StringUtilities` | String | Text manipulation and formatting |
| `DateHelper` | Date/Time | DateTime conversions and calculations |
| `TypeCasting` | Type Conversion | Safe type conversions |
| `QRCode` | QR Code | Generate and read QR codes |
| `TaskScheduler` | Task Scheduling | Windows task creation |
| `Toasty` | Notifications | Desktop toast notifications |
| `JSON` | JSON Processing | JSON fetch, post, serialization |

---

## Dependencies

- **MailKit** - Email handling (v2.0+)
- **ZXing** - QR code generation/reading (v3.5+)
- **Microsoft.Toolkit.Uwp.Notifications** - Toast notifications
- **DocumentFormat.OpenXml** - Excel export (via CreateExcelFile)