// #define DATA_CONTAINS_FORMULAE
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace ExportToExcel;

// July 2019
// http://www.mikesknowledgebase.com
//
// Release history
// -  Sep 2019:
//       Added a "StreamExcelDocument" function, to make the library easy to use from ASP.NET Core.
// -  Jul 2017:
//       The ReplaceHexadecimalSymbols function was incorrectly removing any & characters in the data to be exported.
// -  Sep 2016:
//       Make sure figures with a decimal part are formatted with a full-stop as a decimal point.
// -  February 2016:
//       Added some code, to demonstrate how to add Named Ranges to your Excel file.
// -  July 2015:
//       Numeric columns are now written with different styling, depending on if they are integers or floats/doubles (floats are now displayed with 2 decimal places by default)
//       BugFix: would create invalid Excel files when the user's language setting had a comma as the decimal point character.
// -  May 2015:
//       I now save DateTime values to the Excel file as a number, then format them in Excel to be displayed as a date or date-time
//       Added a handful of Styles to the exported file, in GenerateStyleSheet(), to demonstrate how to do this in OpenXML
//       Optionally allow formulae to be exported  (you need to uncomment the DATA_CONTAINS_FORMULAE definition at the top of this file)
// -  Feb 2015:
//       Needed to replace "Response.End();" with some other code, to make sure the Excel was fully written to the HTTP Response
//       New ReplaceHexadecimalSymbols() function to prevent non-ASCII characters from creating invalid Excel files.
//       Changed GetExcelColumnName() to cope with more than 702 columns (!)
//  - Jan 2015:
//       Throwing an exception when trying to export a DateTime containing null.
//       Was missing the function declaration for "CreateExcelDocument(DataSet ds, string filename, System.Web.HttpResponse Response)"
//       Removed the "Response.End();" from the web version, as recommended in: https://support.microsoft.com/kb/312629/EN-US/?wa=wsignin1.0
//  - Mar 2014:
//       Now writes the Excel data using the OpenXmlWriter classes, which are much more memory efficient.
//  - Nov 2013:
//       Changed "CreateExcelDocument(DataTable dt, string xlsxFilePath)" to remove the DataTable from the DataSet after creating the Excel file.
//       You can now create an Excel file via a Stream (making it more ASP.Net friendly)
//  - Jan 2013: Fix: Couldn't open .xlsx files using OLEDB  (was missing "WorkbookStylesPart" part)
//  - Nov 2012:
//       List<>s with Nullable columns weren't be handled properly.
//       If a value in a numeric column doesn't have any data, don't write anything to the Excel file (previously, it'd write a '0')
//  - Jul 2012: Fix: Some worksheets weren't exporting their numeric data properly, causing "Excel found unreadable content in '___.xslx'" errors.
//  - Mar 2012: Fixed issue, where Microsoft.ACE.OLEDB.12.0 wasn't able to connect to the Excel files created using this class.
//
//
//  (c) www.mikesknowledgebase.com 2019
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
//  (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
//  publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
//  subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//  FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
public class CreateExcelFile
{
    private const int DEFAULT_COLUMN_WIDTH = 20;        // Set each column to be 20 wide

    public static bool CreateExcelDocument<T>(List<T> list, string xlsxFilePath)
    {
        DataSet ds = new();
        ds.Tables.Add(ListToDataTable(list));

        return CreateExcelDocument(ds, xlsxFilePath);
    }

    #region HELPER_FUNCTIONS

    /// <summary>
    /// Convert a List<T> to a DataTable, so we can use the same code to export either a DataTable or a List<T> to Excel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static DataTable ListToDataTable<T>(List<T> list)
    {
        DataTable dt = new();

        foreach(PropertyInfo info in typeof(T).GetProperties())
            dt.Columns.Add(new DataColumn(info.Name, GetNullableType(info.PropertyType)));

        foreach(T t in list)
        {
            DataRow row = dt.NewRow();

            foreach(PropertyInfo info in typeof(T).GetProperties())
            {
                if(!IsNullableType(info.PropertyType))
                    row[info.Name] = info.GetValue(t, null);
                else
                    row[info.Name] = (info.GetValue(t, null) ?? DBNull.Value);
            }

            dt.Rows.Add(row);
        }

        return dt;
    }

    /// <summary>
    /// Get the underlying type of a nullable type, or return the original type if it's not nullable.
    /// </summary>
    /// <param name="t">The type to check for nullability.</param>
    /// <returns>The underlying type if the input is nullable; otherwise, the original type.</returns>
    private static Type GetNullableType(Type t)
    {
        Type returnType = t;

        if(t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            returnType = Nullable.GetUnderlyingType(t);

        return returnType;
    }

    /// <summary>
    /// Determine if a given type is nullable (string, array, or Nullable<T>).
    /// </summary>
    /// <param name="type">The type to check for nullability.</param>
    /// <returns>True if the type is nullable; otherwise, false.</returns>
    private static bool IsNullableType(Type type)
    {
        return type == typeof(string)
            || type.IsArray
            || (type.IsGenericType
                && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
    }

    /// <summary>
    /// Create an Excel file from a DataTable and write it to a specified file path.
    /// </summary>
    /// <param name="dt">The DataTable containing the data to be written to the Excel file.</param>
    /// <param name="xlsxFilePath">The file path where the Excel file will be created.</param>
    /// <returns>True if the Excel file was successfully created; otherwise, false.</returns>
    public static bool CreateExcelDocument(DataTable dt, string xlsxFilePath)
    {
        DataSet ds = new();

        ds.Tables.Add(dt);

        bool result = CreateExcelDocument(ds, xlsxFilePath);

        ds.Tables.Remove(dt);

        return result;
    }

    #endregion HELPER_FUNCTIONS

    /// <summary>
    /// Create an Excel file, and write it to a file.
    /// </summary>
    /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
    /// <param name="excelFilename">Name of file to be written.</param>
    /// <returns>True if successful, false if something went wrong.</returns>
    public static bool CreateExcelDocument(DataSet ds, string excelFilename)
    {
        try
        {
            using(SpreadsheetDocument spreadsheet = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
            {
                WriteExcelFile(ds, spreadsheet);
            }

            Trace.WriteLine("Successfully created: " + excelFilename);

            return true;
        }
        catch(Exception ex)
        {
            Trace.WriteLine("Failed, exception thrown: " + ex.Message);

            return false;
        }
    }

    /// <summary>
    /// Create an Excel file from a List<T> and stream it to the client as a downloadable file in an ASP.NET Core application.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list.</typeparam>
    /// <param name="list">The list of objects to be written to the Excel file.</param>
    /// <param name="filename">The name of the Excel file to be created.</param>
    /// <returns>A FileStreamResult representing the Excel file to be downloaded by the client.</returns>
    public static FileStreamResult StreamExcelDocument<T>(List<T> list, string filename)
    {
        // If you're using ASP.NET Core 2.x+, then this is the function to call !
        // It will create a temporary Excel file, stream its contents, then delete it.
        string TempFilename = Path.Join(Path.GetTempPath(), filename);

        CreateExcelDocument(list, TempFilename);

        byte[] rawData = File.ReadAllBytes(TempFilename);
        MemoryStream ms = new MemoryStream(rawData);

        FileStreamResult fr = new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        fr.FileDownloadName = filename;

        File.Delete(TempFilename);      // We don't need this anymore

        return fr;
    }

    /// <summary>
    /// Create an Excel file, and write it to a Stream (eg MemoryStream).
    /// </summary>
    /// <param name="ds">DataSet containing the data to be written to the Excel.</param>
    /// <param name="spreadsheet">SpreadsheetDocument to write the data to.</param>
    private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
    {
        // Create the Excel file contents.  This function is used when creating an Excel file either writing
        // to a file, or writing to a MemoryStream.
        spreadsheet.AddWorkbookPart();
        spreadsheet.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

        DefinedNames definedNamesCol = new();

        spreadsheet.WorkbookPart.Workbook.Append(new BookViews(new WorkbookView()));

        // If we don't add a "WorkbookStylesPart", OLEDB will refuse to connect to this .xlsx file !
        WorkbookStylesPart workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
        workbookStylesPart.Stylesheet = GenerateStyleSheet();
        workbookStylesPart.Stylesheet.Save();

        // Loop through each of the DataTables in our DataSet, and create a new Excel Worksheet for each.
        uint worksheetNumber = 1;
        Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());

        foreach(DataTable dt in ds.Tables)
        {
            // For each worksheet you want to create
            string worksheetName = dt.TableName;

            // Create worksheet part, and add it to the sheets collection in workbook
            WorksheetPart newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
            Sheet sheet = new()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                SheetId = worksheetNumber,
                Name = worksheetName
            };

            // If you want to define the Column Widths for a Worksheet, you need to do this *before* appending the SheetData
            // http://social.msdn.microsoft.com/Forums/en-US/oxmlsdk/thread/1d93eca8-2949-4d12-8dd9-15cc24128b10/

            sheets.Append(sheet);

            // Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
            WriteDataTableToExcelWorksheet(dt, newWorksheetPart, definedNamesCol);

            worksheetNumber++;
        }

        spreadsheet.WorkbookPart.Workbook.Append(definedNamesCol);
        spreadsheet.WorkbookPart.Workbook.Save();
    }

    /// <summary>
    /// Generate a Stylesheet for the Excel file, defining fonts, fills, borders, and cell formats.
    /// </summary>
    /// <returns>A Stylesheet object representing the styles for the Excel file.</returns>
    private static Stylesheet GenerateStyleSheet()
    {
        // If you want certain Excel cells to have a different Format, color, border, fonts, etc, then you need to define a "CellFormats" records containing
        // these attributes, then assign that style number to your cell.
        //
        // For example, we'll define "Style # 3" with the attributes we'd like for our header row (Row #1) on each worksheet, where the text is a bit bigger,
        // and is white text on a dark-gray background.
        //
        // NB: The NumberFormats from 0 to 163 are hardcoded in Excel (described in the following URL), and we'll define a couple of custom number formats below.
        // https://msdn.microsoft.com/en-us/library/documentformat.openxml.spreadsheet.numberingformat.aspx
        // http://lateral8.com/articles/2010/6/11/openxml-sdk-20-formatting-excel-values.aspx
        //
        uint iExcelIndex = 164;

        return new Stylesheet(
            new NumberingFormats(
                //
                new NumberingFormat()                                                  // Custom number format # 164: especially for date-times
                {
                    NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                    FormatCode = StringValue.FromString("dd/MMM/yyyy hh:mm:ss")
                },
                new NumberingFormat()                                                   // Custom number format # 165: especially for date times (with a blank time)
                {
                    NumberFormatId = UInt32Value.FromUInt32(iExcelIndex++),
                    FormatCode = StringValue.FromString("dd/MMM/yyyy")
                }
           ),
            new Fonts(
                new Font(                                                               // Index 0 - The default font.
                    new FontSize() { Val = 10 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Arial" }),
                new Font(                                                               // Index 1 - A 12px bold font, in white.
                    new Bold(),
                    new FontSize() { Val = 12 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "FFFFFF" } },
                    new FontName() { Val = "Arial" }),
                new Font(                                                               // Index 2 - An Italic font.
                    new Italic(),
                    new FontSize() { Val = 10 },
                    new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                    new FontName() { Val = "Times New Roman" })
            ),
            new Fills(
                new Fill(                                                           // Index 0 - The default fill.
                    new PatternFill() { PatternType = PatternValues.None }),
                new Fill(                                                           // Index 1 - The default fill of gray 125 (required)
                    new PatternFill() { PatternType = PatternValues.Gray125 }),
                new Fill(                                                           // Index 2 - The yellow fill.
                    new PatternFill(
                        new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FFFFFF00" } }
                    )
                    { PatternType = PatternValues.Solid }),
                new Fill(                                                           // Index 3 - Dark-gray fill.
                    new PatternFill(
                        new ForegroundColor() { Rgb = new HexBinaryValue() { Value = "FF404040" } }
                    )
                    { PatternType = PatternValues.Solid })
            ),
            new Borders(
                new Border(                                                         // Index 0 - The default border.
                    new LeftBorder(),
                    new RightBorder(),
                    new TopBorder(),
                    new BottomBorder(),
                    new DiagonalBorder()),
                new Border(                                                         // Index 1 - Applies a Left, Right, Top, Bottom border to a cell
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new DiagonalBorder())
            ),
            new CellFormats(
                new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 },                         // Style # 0 - The default cell style.  If a cell does not have a style index applied it will use this style combination instead
                new CellFormat() { NumberFormatId = 164 },                                         // Style # 1 - DateTimes
                new CellFormat() { NumberFormatId = 165 },                                         // Style # 2 - Dates (with a blank time)
                new CellFormat(new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center })
                { FontId = 1, FillId = 3, BorderId = 0, ApplyFont = true, ApplyAlignment = true },       // Style # 3 - Header row
                new CellFormat() { NumberFormatId = 3 },                                           // Style # 4 - Number format: #,##0
                new CellFormat() { NumberFormatId = 4 },                                           // Style # 5 - Number format: #,##0.00
                new CellFormat() { FontId = 1, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 6 - Bold
                new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 7 - Italic
                new CellFormat() { FontId = 2, FillId = 0, BorderId = 0, ApplyFont = true },       // Style # 8 - Times Roman
                new CellFormat() { FontId = 0, FillId = 2, BorderId = 0, ApplyFill = true },       // Style # 9 - Yellow Fill
                new CellFormat(                                                                    // Style # 10 - Alignment
                    new Alignment() { Horizontal = HorizontalAlignmentValues.Center, Vertical = VerticalAlignmentValues.Center }
                )
                { FontId = 0, FillId = 0, BorderId = 0, ApplyAlignment = true },
                new CellFormat() { FontId = 0, FillId = 0, BorderId = 1, ApplyBorder = true }      // Style # 11 - Border
            )
        ); // return
    }

    /// <summary>
    /// Write the contents of a DataTable to an Excel Worksheet, using OpenXmlWriter to prevent memory issues.
    /// </summary>
    /// <param name="dt">DataTable containing the data to be written to the Excel worksheet.</param>
    /// <param name="worksheetPart">WorksheetPart representing the Excel worksheet to write to.</param>
    /// <param name="definedNamesCol">DefinedNames collection for the Excel workbook.</param>
    private static void WriteDataTableToExcelWorksheet(DataTable dt, WorksheetPart worksheetPart, DefinedNames definedNamesCol)
    {
        OpenXmlWriter writer = OpenXmlWriter.Create(worksheetPart, Encoding.ASCII);
        writer.WriteStartElement(new Worksheet());

        // To demonstrate how to set column-widths in Excel, here's how to set the width of all columns to our default of "25":
        UInt32 inx = 1;

        writer.WriteStartElement(new Columns());

        foreach(DataColumn dc in dt.Columns)
        {
            writer.WriteElement(new Column { Min = inx, Max = inx, CustomWidth = true, Width = DEFAULT_COLUMN_WIDTH });
            inx++;
        }

        writer.WriteEndElement();
        writer.WriteStartElement(new SheetData());

        string cellValue = "";
        string cellReference = "";

        // Create a Header Row in our Excel file, containing one header for each Column of data in our DataTable.
        // We'll also create an array, showing which type each column of data is (Text or Numeric), so when we come to write the actual
        // cells of data, we'll know if to write Text values or Numeric cell values.
        int numberOfColumns = dt.Columns.Count;
        bool[] IsIntegerColumn = new bool[numberOfColumns];
        bool[] IsFloatColumn = new bool[numberOfColumns];
        bool[] IsDateColumn = new bool[numberOfColumns];
        string[] excelColumnNames = new string[numberOfColumns];

        for(int n = 0; n < numberOfColumns; n++)
            excelColumnNames[n] = GetExcelColumnName(n);

        // Create the Header row in our Excel Worksheet
        // We'll set the row-height to 20px, and (using the "AppendHeaderTextCell" function) apply some formatting to the cells.
        uint rowIndex = 1;

        writer.WriteStartElement(new Row
        {
            RowIndex = rowIndex,
            Height = 20,
            CustomHeight = true
        });

        for(int colInx = 0; colInx < numberOfColumns; colInx++)
        {
            DataColumn col = dt.Columns[colInx];

            AppendHeaderTextCell(excelColumnNames[colInx] + "1", col.ColumnName, writer);
            IsIntegerColumn[colInx] = (col.DataType.FullName.StartsWith("System.Int"));
            IsFloatColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
            IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");

            // Uncomment the following lines, for an example of how to create some Named Ranges in your Excel file
#if FALSE
            // For each column of data in this worksheet, let's create a Named Range, showing where there are values in this column
            //      eg  "NamedRange_UserID"  = "Drivers!$A2:$A6"
            //          "NamedRange_Surname" = "Drivers!$B2:$B6"
            string columnHeader = col.ColumnName.Replace(" ", "_");
            string NamedRange = $"{worksheetName}!${excelColumnNames[colInx]}2:${excelColumnNames[colInx]}{dt.Rows.Count + 1}";

            DefinedName definedName = new() {
                Name = "NamedRange_" + columnHeader,
                Text = NamedRange
            };

            definedNamesCol.Append(definedName);
#endif
        }
        writer.WriteEndElement();   // End of header "Row"

        // Now, step through each row of data in our DataTable...
        double cellFloatValue = 0;
        CultureInfo ci = new("en-US");

        foreach(DataRow dr in dt.Rows)
        {
            // ...create a new row, and append a set of this row's data to it.
            ++rowIndex;
            writer.WriteStartElement(new Row { RowIndex = rowIndex });

            for(int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                cellValue = dr.ItemArray[colInx].ToString();
                cellValue = ReplaceHexadecimalSymbols(cellValue);
                cellReference = excelColumnNames[colInx] + rowIndex.ToString();

                // Create cell with data
                if(IsIntegerColumn[colInx] || IsFloatColumn[colInx])
                {
                    // For numeric cells without any decimal places.
                    // If this numeric value is NULL, then don't write anything to the Excel file.
                    cellFloatValue = 0;

                    bool bIncludeDecimalPlaces = IsFloatColumn[colInx];

                    if(double.TryParse(cellValue, out cellFloatValue))
                    {
                        cellValue = cellFloatValue.ToString(ci);
                        AppendNumericCell(cellReference, cellValue, bIncludeDecimalPlaces, writer);
                    }
                }
                else if(IsDateColumn[colInx])
                {
                    // For date values, we save the value to Excel as a number, but need to set the cell's style to format
                    // it as either a date or a date-time.
                    if(DateTime.TryParse(cellValue, out DateTime dateValue))
                    {
                        AppendDateCell(cellReference, dateValue, writer);
                    }
                    else
                    {
                        // This should only happen if we have a DataColumn of type "DateTime", but this particular value is null/blank.
                        AppendTextCell(cellReference, cellValue, writer);
                    }
                }
                else
                {
                    // For text cells, just write the input data straight out to the Excel file.
                    AppendTextCell(cellReference, cellValue, writer);
                }
            }

            writer.WriteEndElement(); // End of Row
        }

        writer.WriteEndElement(); // End of SheetData
        writer.WriteEndElement(); // End of worksheet
        writer.Close();
    }

    /// <summary>
    /// Append a "Header" cell to the Excel file, with some formatting applied (white text on a dark-gray background).
    /// </summary>
    /// <param name="cellReference">The reference of the cell (e.g., "A1").</param>
    /// <param name="cellStringValue">The text value to be displayed in the cell.</param>
    /// <param name="writer">The OpenXmlWriter used to write to the Excel file.</param>
    private static void AppendHeaderTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
    {
        // Add a new "text" Cell to the first row in our Excel worksheet
        // We set these cells to use "Style # 3", so they have a gray background color & white text.
        writer.WriteElement(new Cell
        {
            CellValue = new CellValue(cellStringValue),
            CellReference = cellReference,
            DataType = CellValues.String,
            StyleIndex = 3
        });
    }

    private static void AppendTextCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
    {
        // Add a new "text" Cell to our Row

#if DATA_CONTAINS_FORMULAE
        // If this item of data looks like a formula, let's store it in the Excel file as a formula rather than a string.
        if (cellStringValue.StartsWith("="))
        {
            AppendFormulaCell(cellReference, cellStringValue, writer);

            return;
        }
#endif

        // Add a new Excel Cell to our Row
        writer.WriteElement(new Cell
        {
            CellValue = new CellValue(cellStringValue),
            CellReference = cellReference,
            DataType = CellValues.String
        });
    }

    /// <summary>
    /// Append a "date" cell to the Excel file, with the specified DateTime value.
    /// </summary>
    /// <param name="cellReference">The reference of the cell (e.g., "A1").</param>
    /// <param name="dateTimeValue">The DateTime value to be displayed in the cell.</param>
    /// <param name="writer">The OpenXmlWriter used to write to the Excel file.</param>
    private static void AppendDateCell(string cellReference, DateTime dateTimeValue, OpenXmlWriter writer)
    {
        // Add a new "datetime" Excel Cell to our Row.
        //
        // If the "time" part of the DateTime is blank, we'll format the cells as "dd/MMM/yyyy", otherwise ""dd/MMM/yyyy hh:mm:ss"
        // (Feel free to modify this logic if you wish.)
        //
        // In our GenerateStyleSheet() function, we defined two custom styles, to make this work:
        //     Custom style#1 is a style containing our custom NumberingFormat # 164 (show each date as "dd/MMM/yyyy hh:mm:ss")
        //     Custom style#2 is a style containing our custom NumberingFormat # 165 (show each date as "dd/MMM/yyyy")
        //
        // So, if our time element is blank, we'll assign style 2, but if there IS a time part, we'll apply style 1.
        string cellStringValue = dateTimeValue.ToOADate().ToString(CultureInfo.InvariantCulture);
        bool bHasBlankTime = (dateTimeValue.Date == dateTimeValue);

        writer.WriteElement(new Cell
        {
            CellValue = new CellValue(cellStringValue),
            CellReference = cellReference,
            StyleIndex = UInt32Value.FromUInt32(bHasBlankTime ? 2 : (uint)1),
            DataType = CellValues.Number        // Use this, rather than CellValues.Date
        });
    }

    /// <summary>
    /// Append a "formula" cell to the Excel file, with the specified formula string.
    /// </summary>
    /// <param name="cellReference">The reference of the cell (e.g., "A1").</param>
    /// <param name="cellStringValue">The formula string to be applied to the cell.</param>
    /// <param name="writer">The OpenXmlWriter used to write to the Excel file.</param>
    private static void AppendFormulaCell(string cellReference, string cellStringValue, OpenXmlWriter writer)
    {
        // Add a new "formula" Excel Cell to our Row
        writer.WriteElement(new Cell
        {
            CellFormula = new CellFormula(cellStringValue),
            CellReference = cellReference,
            DataType = CellValues.Number
        });
    }

    /// <summary>
    /// Append a "numeric" cell to the Excel file, with the specified numeric value.
    /// </summary>
    /// <param name="cellReference">The reference of the cell (e.g., "A1").</param>
    /// <param name="cellStringValue">The numeric value to be displayed in the cell.</param>
    /// <param name="bIncludeDecimalPlaces">Indicates whether to include decimal places in the cell formatting.</param>
    /// <param name="writer">The OpenXmlWriter used to write to the Excel file.</param>
    private static void AppendNumericCell(string cellReference, string cellStringValue, bool bIncludeDecimalPlaces, OpenXmlWriter writer)
    {
        // Add a new numeric Excel Cell to our Row.
        UInt32 cellStyle = (UInt32)(bIncludeDecimalPlaces ? 5 : 4);

        writer.WriteElement(new Cell
        {
            CellValue = new CellValue(cellStringValue),
            CellReference = cellReference,
            StyleIndex = cellStyle,                                 // Style #4 formats with 0 decimal places, style #5 formats with 2 decimal places
            DataType = CellValues.Number
        });
    }

    /// <summary>
    /// Replace any non-ASCII characters in the input string with an empty string, effectively removing them.
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    private static string ReplaceHexadecimalSymbols(string txt)
    {
        // I've often seen cases when a non-ASCII character will slip into the data you're trying to export,
        // and this will cause an invalid Excel to be created.
        // This function removes such characters.
        string r = "[\x00-\x08\x0B\x0C\x0E-\x1F]";

        return Regex.Replace(txt, r, "", RegexOptions.Compiled);
    }

    /// <summary>
    /// Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Z, AA, AB, AC... AY, AZ, BA, BB..)
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public static string GetExcelColumnName(int columnIndex)
    {
        // Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Z, AA, AB, AC... AY, AZ, BA, BB..)
        // eg GetExcelColumnName(0) should return "A"
        //    GetExcelColumnName(1) should return "B"
        //    GetExcelColumnName(25) should return "Z"
        //    GetExcelColumnName(26) should return "AA"
        //    GetExcelColumnName(27) should return "AB"
        //    GetExcelColumnName(701) should return "ZZ"
        //    GetExcelColumnName(702) should return "AAA"
        //    GetExcelColumnName(1999) should return "BXX"
        //    ..etc..

        int firstInt = columnIndex / 676;
        int secondInt = columnIndex % 676 / 26;

        if(secondInt == 0)
        {
            secondInt = 26;
            firstInt--;
        }

        int thirdInt = columnIndex % 26;
        char firstChar = (char)('A' + firstInt - 1);
        char secondChar = (char)('A' + secondInt - 1);
        char thirdChar = (char)('A' + thirdInt);

        if(columnIndex < 26) return thirdChar.ToString();
        if(columnIndex < 702) return $"{secondChar}{thirdChar}";

        return $"{firstChar}{secondChar}{thirdChar}";
    }
}