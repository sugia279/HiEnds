using HiEndsCore.Helper;
using HiEndsCore.Interface;
using HiEndsCore.Models;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using static System.String;

namespace HiEndsExtractor.DataDriver
{
    // AssemblyDataExtractor.cs
    public class AssemblyDataExtractor : IDataExtractor
    {
        public string Name => "Assembly";
        
        public T ExtractData<T>(string sourcePath, ExtractionTemplate template)
        {
            // Use the sourceInfo to extract data from assemblies using reflection
            // Use filterConditions.MainAttributes and filterConditions.LoadedAttributes for filtering
            // Return the extracted data as a DataTable
            Assembly assembly = Assembly.LoadFrom(sourcePath);

            DataTable dt = new DataTable("AssemblyDataSource");

            List<string> loadedAttributes = template.LoadedAttributes;
            foreach (var attribute in loadedAttributes)
            {
                var attrName = attribute.Split(':');
                var dtColumn = new DataColumn(attrName[0]);
                dtColumn.DataType = typeof(string);
                dt.Columns.Add(dtColumn);
            }
            
            foreach (var type in assembly.GetTypes())
            {
                if (template.FilterConditions.Namespaces != null 
                    && !template.FilterConditions.Namespaces.Any(a => type.Namespace != null && type.Namespace.Contains(a)))
                {
                    continue;
                }

                if (type.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    continue;

                foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
                {
                    bool matchedMethod = false;

                    if (template.FilterConditions.Classes != null
                        && !template.FilterConditions.Classes.Any(a => !IsNullOrEmpty(method.Name) && method.Name.Contains(a)))
                    {
                        continue;
                    }

                    var custAttributes = method.GetCustomAttributes(false);

                    if (template.FilterConditions.Attributes?.Count > 0)
                    {
                        foreach (var mainAtts in template.FilterConditions.Attributes)
                        {
                            if (custAttributes.Any(a => a.GetType().Name == mainAtts))
                            {
                                matchedMethod = true;
                                break;
                            }
                        }
                    }

                    if (matchedMethod)
                    {
                        var dtRow = dt.NewRow();
                        
                        foreach (var loadedAttribute in loadedAttributes)
                        {
                            switch (loadedAttribute)
                            {
                                case "FullName":
                                    dtRow["FullName"] = $"{type.FullName ?? Empty}";
                                    break;
                                case "MethodName":
                                    dtRow["MethodName"] = $"{method.Name}";
                                    break;
                                case "FullMethodName":
                                    dtRow["FullMethodName"] = $"{type.FullName ?? Empty}.{method.Name}";
                                    break;
                                default:
                                    var custAtt = custAttributes.FirstOrDefault(x => x.GetType().Name.Equals(loadedAttribute));
                                    if (custAtt == null)
                                    {
                                        if (template.AttributeNotExistString != null)
                                            dtRow[loadedAttribute] = template.AttributeNotExistString;
                                    }
                                    else
                                    {
                                        var properties = custAtt.GetType().GetProperties();
                                        string values = Empty;
                                        foreach (var propertyInfo in properties)
                                        {
                                            if (propertyInfo.PropertyType.FullName != "System.Object")
                                            {
                                                values += $@"{propertyInfo.GetValue(custAtt, null) ?? template.NullValueString ?? Empty} | ";
                                            }
                                        }
                                        char[] charsToTrim1 = { ' ', '|', ' ' };
                                        dtRow[custAtt.GetType().Name] = values.TrimEnd(charsToTrim1);
                                    }
                                    break;
                            }
                        }
                        dt.Rows.Add(dtRow);
                    }
                }
            }

            return (T)(object)dt;
            
        }
        
        public T TransformData<T>(T data, ExtractionTemplate eTemplate, string filter)
        {
            DataTable dataTable = (data as DataTable)!;
            RemapColumnName(eTemplate, dataTable);

            DataTable filteredData = new DataTable();
            DataRow[] drs = dataTable.Select(filter);
            if (drs.Length > 0)
                filteredData = drs.CopyToDataTable();
            else
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    filteredData.Columns.Add(col.ColumnName);
                }
            }
            return (T)(object)filteredData;
        }

        public T MergeToExistingData<T>(T data, string existingData, DataType type)
        {
            // Call the ExtractData method to get the data
            DataTable dataTable = (data as DataTable)!;
            DataTable currentData = new DataTable();
            switch (type)
            {
                case DataType.json:
                    currentData = JsonUtilities.JsonToDataTable(existingData);
                    currentData.Merge(dataTable, false, MissingSchemaAction.Add);
                    break;
                case DataType.csv:
                    currentData = CsvUtilities.CsvToDataTable(existingData);
                    currentData.Merge(dataTable, false, MissingSchemaAction.Add);
                    break;
            }
            return (T)(object)currentData;
        }

        public string LoadDataAsString<T>(T data, DataType type)
        {
            // Call the ExtractData method to get the data
            DataTable dataTable = (data as DataTable)!;
            string result = string.Empty;
            switch (type)
            {
                case DataType.json:
                    result = JsonUtilities.DataTableToJson(dataTable);
                    break;
                case DataType.csv:
                    result = CsvUtilities.DataTableToCsv(dataTable);
                    break;
            }
            return result;
        }


        private static void RemapColumnName(ExtractionTemplate template, DataTable data)
        {
            foreach (var val in template.Remap)
            {
                if (data.Columns.Contains(val.Key) && !IsNullOrEmpty(val.Value))
                {
                    data.Columns[val.Key]!.ColumnName = val.Value;
                }
                //if (!IsNullOrEmpty(val) && val.Split(':').Length > 1)
                //{
                //    string[] columnNames = val.Split(':');
                //    if (data.Columns.Contains(columnNames[0]) && !IsNullOrEmpty(columnNames[1]))
                //    {
                //        data.Columns[columnNames[0]]!.ColumnName = columnNames[1];
                //    }
                //}
            }
        }
    }
}