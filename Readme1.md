📚 FCS.Utilities Library - Comprehensive Developer & Business Analysis Guide (v1.0.3)
This document is designed for two audiences:

Business Analyst: To understand what features this library provides and why they are used in the system, without getting bogged down in code syntax.
Developer: To quickly grasp the core technical mechanism of each module, understand dependencies, and know the best practices (and common pitfalls) for implementation.
🗺️ Table of Contents
Section    File Path / Module    Primary Focus    Developer Warning / Key TakeawaySection    File Path / Module    Primary Focus    Developer Warning / Key TakeawaySection    File Path / Module    Primary Focus    Developer Warning / Key Takeaway
I. Core Infrastructure    AppInfo.cs, ExceptionHandlerMiddleware.cs, ErrorHandler.cs    System Metadata & Logging    Always use the global middleware for robust error capture. Never manually log critical errors without context.I. Core Infrastructure    AppInfo.cs, ExceptionHandlerMiddleware.cs, ErrorHandler.cs    System Metadata & Logging    Always use the global middleware for robust error capture. Never manually log critical errors without context.I. Core Infrastructure    AppInfo.cs, ExceptionHandlerMiddleware.cs, ErrorHandler.cs    System Metadata & Logging    Always use the global middleware for robust error capture. Never manually log critical errors without context.

II. Data Handling & Persistence	DataMapping/*, TypeCasting.cs, DBHelper.cs, CSV.cs	Moving data between formats (CSVII. Data Handling & Persistence    DataMapping/*, TypeCasting.cs, DBHelper.cs, CSV.cs    Moving data between formats (CSV II. Data Handling & Persistence    DataMapping/*, TypeCasting.cs, DBHelper.cs, CSV.cs    Moving data between formats (CSV 

→
→ Object, Row
→
→ Object).    Use the Mapper and TypeCasting utilities to prevent runtime type casting exceptions.
III. Network & API Integration    JSON.cs, EmailHelper.cs, Twitter.cs    Communication with external services (APIs, SMTP).    Must handle network timeouts and service authentication credentials securely.III. Network & API Integration    JSON.cs, EmailHelper.cs, Twitter.cs    Communication with external services (APIs, SMTP).    Must handle network timeouts and service authentication credentials securely.III. Network & API Integration    JSON.cs, EmailHelper.cs, Twitter.cs    Communication with external services (APIs, SMTP).    Must handle network timeouts and service authentication credentials securely.
IV. Data Modeling Utilities    GEO.cs, Function\CountryDropDownList.cs, Functions\DayDropDownList.cs    Structuring data for UI/Business logic.    These modules provide predefined lists of constants (e.g., country codes) that should not be modified in application code.IV. Data Modeling Utilities    GEO.cs, Function\CountryDropDownList.cs, Functions\DayDropDownList.cs    Structuring data for UI/Business logic.    These modules provide predefined lists of constants (e.g., country codes) that should not be modified in application code.IV. Data Modeling Utilities    GEO.cs, Function\CountryDropDownList.cs, Functions\DayDropDownList.cs    Structuring data for UI/Business logic.    These modules provide predefined lists of constants (e.g., country codes) that should not be modified in application code.
V. String & Formatting    StringUtilities.cs, ProperCase.cs, Functions\ExtractUrl.cs, etc.    Cleaning, standardizing, and manipulating text data.    Always validate user input using these utilities before saving or processing it further.V. String & Formatting    StringUtilities.cs, ProperCase.cs, Functions\ExtractUrl.cs, etc.    Cleaning, standardizing, and manipulating text data.    Always validate user input using these utilities before saving or processing it further.V. String & Formatting    StringUtilities.cs, ProperCase.cs, Functions\ExtractUrl.cs, etc.    Cleaning, standardizing, and manipulating text data.    Always validate user input using these utilities before saving or processing it further.
I. Core Infrastructure (The Foundation)
💾 Metadata Retrieval (AppInfo.cs)
Business Purpose: Provides a standardized way to access critical application metadata (Version, Product, Company). This ensures that all parts of the system can accurately report "Who," "What," and "When" an error or event occurred.
Developer Mechanism: Uses C# reflection to read attributes defined on the assembly itself.

Property    Purpose    Developer TipProperty    Purpose    Developer Tip
Title, Product, etc.    Reads fixed metadata set at compile time.    These values are static and ideal for display purposes (UI, headers).Title, Product, etc.    Reads fixed metadata set at compile time.    These values are static and ideal for display purposes (UI, headers).
ExeName / ExePath    Gets the location of the running program.    Use this when building paths to ensure logs or configuration files are saved in the correct execution directory.ExeName / ExePath    Gets the location of the running program.    Use this when building paths to ensure logs or configuration files are saved in the correct execution directory.
🚨 Error Handling (ErrorHandler.cs & Middleware)
Business Purpose: When an error occurs, a BA needs confidence that the developers will receive enough information to fix it immediately. This module automates the creation of comprehensive "forensic reports."
Developer Mechanism: It uses HttpContext and built-in CLR exception objects (Exception, AggregateException) to gather contextual data (headers, request body).

Feature/Method    Technical Functionality    Critical Warning / Best PracticeFeature/Method    Technical Functionality    Critical Warning / Best Practice
Middleware Handler    Catches exceptions globally in the ASP.NET Core pipeline before they reach the user.    MUST be registered at the application startup. This is the fail-safe safety net for all requests.Middleware Handler    Catches exceptions globally in the ASP.NET Core pipeline before they reach the user.    MUST be registered at the application startup. This is the fail-safe safety net for all requests.
ReportError(...)    Generates the full HTML report body, including system and request details.    Always pass an optional HttpContext. If missing (e.g., in a background service), the error log will still capture environment data.ReportError(...)    Generates the full HTML report body, including system and request details.    Always pass an optional HttpContext. If missing (e.g., in a background service), the error log will still capture environment data.
Data Redaction    Automatically scans HTTP headers and query parameters for keys like password, token, auth.    Crucial Security Feature. Prevents accidentally sending sensitive credentials via email reports.Data Redaction    Automatically scans HTTP headers and query parameters for keys like password, token, auth.    Crucial Security Feature. Prevents accidentally sending sensitive credentials via email reports.
🧩 Data Mapping & Persistence (DataMapping/* & TypeCasting.cs)
This group is the developer's bridge between messy data sources and clean C# objects.

Module / Class    Business Purpose    Developer Mechanism    Key Concept to UnderstandModule / Class    Business Purpose    Developer Mechanism    Key Concept to UnderstandModule / Class    Business Purpose    Developer Mechanism    Key Concept to Understand
DataNamesMapper<T>    Converts structured database rows into usable application objects.    Uses Reflection combined with the @DataNamesAttribute to dynamically map column names (DB_COL) to class properties (MyProp).    The mapper is intelligent enough to find the correct property even if the DB schema changes its naming convention (if attributes are updated).DataNamesMapper<T>    Converts structured database rows into usable application objects.    Uses Reflection combined with the @DataNamesAttribute to dynamically map column names (DB_COL) to class properties (MyProp).    The mapper is intelligent enough to find the correct property even if the DB schema changes its naming convention (if attributes are updated).DataNamesMapper<T>    Converts structured database rows into usable application objects.    Uses Reflection combined with the @DataNamesAttribute to dynamically map column names (DB_COL) to class properties (MyProp).    The mapper is intelligent enough to find the correct property even if the DB schema changes its naming convention (if attributes are updated).
TypeCasting    Provides safe conversion utilities.    Wraps all Convert.ChangeType() calls in robust try/catch logic, preventing runtime crashes due to data mismatch.    Always use methods like ToNullableDecimal(object?) rather than manual casting ((decimal)obj) for maximum safety.TypeCasting    Provides safe conversion utilities.    Wraps all Convert.ChangeType() calls in robust try/catch logic, preventing runtime crashes due to data mismatch.    Always use methods like ToNullableDecimal(object?) rather than manual casting ((decimal)obj) for maximum safety.TypeCasting    Provides safe conversion utilities.    Wraps all Convert.ChangeType() calls in robust try/catch logic, preventing runtime crashes due to data mismatch.    Always use methods like ToNullableDecimal(object?) rather than manual casting ((decimal)obj) for maximum safety.
DBHelper    Wrapper for SQL communication.    Manages connection opening, parameter injection, and result set retrieval. Supports both synchronous (RunStoredProc) and asynchronous (RunStoredProcAsync) calls.    Performance: Always use the Async versions to prevent blocking threads in web applications.DBHelper    Wrapper for SQL communication.    Manages connection opening, parameter injection, and result set retrieval. Supports both synchronous (RunStoredProc) and asynchronous (RunStoredProcAsync) calls.    Performance: Always use the Async versions to prevent blocking threads in web applications.DBHelper    Wrapper for SQL communication.    Manages connection opening, parameter injection, and result set retrieval. Supports both synchronous (RunStoredProc) and asynchronous (RunStoredProcAsync) calls.    Performance: Always use the Async versions to prevent blocking threads in web applications.
CSV	External data ingestion from URLs.	Handles the entire process: network fetchCSV    External data ingestion from URLs.    Handles the entire process: network fetch CSV    External data ingestion from URLs.    Handles the entire process: network fetch 
→
→ string parsing
→
→ object deserialization.    The method swallows exceptions by default (catch {}). While useful for robustness, be aware that this can mask critical connection issues.

II. Data Modeling & Utilities (The Constant Sources)

🗺️ Location Utilities
GEO.cs: Calculates the distance between two points on Earth using Latitude/Longitude.
BA Use Case: Calculating shipping rates or service radius based on geography.
Developer Detail: Uses the Haversine formula. The Miles boolean allows easy toggling between imperial and metric units.
📅 Date & Time Utilities

1. DateHelper.cs (Extension)

Primary Function: Provides flexible date/time conversion methods, particularly for handling timestamps.
Key Feature: Unix Timestamps: Handles the complexity of time zone conversions between local system time and universal UTC seconds since Jan 1, 1970.
2. MonthDropDownList.cs & Function\CountryDropDownList.cs (Static)
BA Use Case: Guarantees that dropdown lists display data in a standardized format recognized by the business unit.
Developer Mechanism: These classes are essentially constant, read-only lookup tables for all global units (e.g., US States, Canadian Provinces). They ensure consistency by converting inputs and outputs to uppercase (ToUpper()) before comparison.
🧵 String & Formatting Utilities
This section contains the highest density of methods and is crucial for data cleanup.

1. StringUtilities.cs
Best Practice: Use this module as your first stop whenever raw user input or external text needs to be cleaned up before storage or display.
Example: If a search query contains special characters, use CharsOnly() to ensure only safe alphanumeric values remain.
2. Functions\FormatName.cs (Extension)
Goal: To format human names correctly (e.g., "doe's"
→
→ "Doe's"). This handles the complex rules of apostrophes and common prefixes (Mc, Mac).
3. Function\Base64.cs (Extension)
Usage 1 (Text): Converts text to a Base64 string for safe transmission or embedding.
Usage 2 (Image): Allows the conversion of raw base64 bytes back into an in-memory image object (System.Drawing.Bitmap), bypassing file system saves.

III. Business Process Utilities

📧 Email Messaging (EmailHelper.cs)
Business Purpose: Provides a robust, all-in-one solution for sending professional emails that meet corporate standards (e.g., adding company headers, managing attachments).
Key Concepts & Dependencies: Uses the industry-standard MailKit library for reliable SMTP communication.

Component    Role    Developer Detail / Risk MitigationComponent    Role    Developer Detail / Risk Mitigation
Properties (FromAddress, etc.)    Used to build the message headers and sender identity.    Always set these properties before calling Send(). The system must know who is sending the report/email.Properties (FromAddress, etc.)    Used to build the message headers and sender identity.    Always set these properties before calling Send(). The system must know who is sending the report/email.
CreateAttachmentFromString()    Handles attachments whose content is available in memory (e.g., a dynamically generated PDF byte array).    Use this instead of file path attachments if you want to process or generate the attachment content before emailing it.CreateAttachmentFromString()    Handles attachments whose content is available in memory (e.g., a dynamically generated PDF byte array).    Use this instead of file path attachments if you want to process or generate the attachment content before emailing it.
Send()    The execution method that connects, sends, and handles all potential failure states.    Always check the returned Tuple<bool, string> for success/failure status before proceeding with business logic.Send()    The execution method that connects, sends, and handles all potential failure states.    Always check the returned Tuple<bool, string> for success/failure status before proceeding with business logic.
🖼️ Media & Interaction (QRCode.cs)
Purpose: Handles the creation and decoding of QR codes.
Method: Create(imageFileName, content): Generates a standard QR code image file based on provided data (e.g., a URL or text).
Developer Detail: It enforces minimum size constraints (100x100) and requires explicit error handling for file system write failures (IOException).
⚙️ Task Scheduling (TaskScheduler.cs)
Purpose: Automates the scheduling of tasks on the local Windows machine, allowing background processes to run reliably at specified times.
Method: AddTask(...): Registers a new scheduled task definition in the OS scheduler.
Developer Detail: This function is complex due to interacting with the operating system shell; understand its limitations regarding user permissions (TaskRunLevel.LUA).
