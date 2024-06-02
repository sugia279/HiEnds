using System;
using System.Data;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace HiEndsCore.Helper
{
    public static class CsvUtilities
    {
        public static string DataTableToCsv(DataTable dataTable, string delimiter = ",")
        {
            var csv = DataColumnsToCsv(dataTable, delimiter);
            // Write the data rows
            foreach (DataRow row in dataTable.Rows)
            {
                csv.Append(DataRowToCsv(dataTable.Columns.Count, row, delimiter));
            }

            return csv.ToString();
        }

        public static string DataRowToCsv(int columnCount, DataRow row, string delimiter = ",")
        {
            StringBuilder csv = new StringBuilder();
            for (int i = 0; i < columnCount; i++)
            {
                if (row[i] is string)
                {
                    csv.Append(ConvertStringToCsvFormat((string)row[i]));
                    if (i < columnCount - 1)
                        csv.Append(delimiter);
                }
                else
                {
                    csv.Append(delimiter);
                }
            }

            return csv.AppendLine().ToString();
        }

        static string ConvertStringToCsvFormat(string input)
        {
            // Check if the input contains special characters (comma or double quotes)
            bool containsSpecialCharacters = input.Contains(",") || input.Contains("\"");

            if (containsSpecialCharacters)
            {
                // If the input contains special characters, wrap it in double quotes and escape existing double quotes
                input = "\"" + input.Replace("\"", "\"\"") + "\"";
            }

            return input;
        }

        public static StringBuilder DataColumnsToCsv(DataTable dataTable, string delimiter = ",")
        {
            StringBuilder csv = new StringBuilder();

            // Write the column headers
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                csv.Append(dataTable.Columns[i].ColumnName);
                if (i < dataTable.Columns.Count - 1)
                    csv.Append(delimiter);
            }

            csv.AppendLine();
            return csv;
        }

        //static string ConvertDataRowToCsv(DataRow row)
        //{
        //    // Create a DataTable with a single row to convert
        //    DataTable rowTable = row.Table.Clone();
        //    rowTable.ImportRow(row);

        //    return ConvertDataTableToCsv(rowTable);
        //}

        public static DataTable CsvToDataTable(string csvContent, bool hasHeaderRecord = true)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = hasHeaderRecord
            };

            using (TextReader reader = new StringReader(csvContent))
            using (var csv = new CsvReader(reader, config))
            {
                // Read the records into a dynamic object
                var records = csv.GetRecords<dynamic>().ToList();

                // Create a DataTable
                DataTable dataTable = new DataTable();

                // If there are no records, create columns from the header line
                if (hasHeaderRecord && records.Count == 0)
                {
                    foreach (var header in csv.HeaderRecord)
                    {
                        dataTable.Columns.Add(header);
                    }
                }
                else
                {
                    // Add columns to the DataTable based on the properties of the dynamic object
                    var record = records.First();
                    foreach (var property in ((IDictionary<string, object>)record).Keys)
                    {
                        dataTable.Columns.Add(property);
                    }
                }

                // Add data to the DataTable
                foreach (var r in records)
                {
                    DataRow row = dataTable.NewRow();
                    var recordDict = (IDictionary<string, object>)r;
                    foreach (var property in recordDict.Keys)
                    {
                        var value = recordDict[property];
                        row[property] = value;
                    }
                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }
        }

    }

}
