using HiEndsCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HiEndsCore.Helper
{
    public class JsonUtilities
    {

        // Convert a DataTable to JSON string using Newtonsoft.Json
        public static string DataTableToJson(DataTable dataTable)
        {
            string jsonData = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            return jsonData;
        }

        public static DataTable JsonToDataTable(string jsonData)
        {
            JArray jsonArray = JArray.Parse(jsonData);
            // Create a DataTable with dynamic columns
            DataTable dataTable = new DataTable();

            // Add columns based on the JSON properties
            foreach (JObject item in jsonArray)
            {
                foreach (var property in item.Properties())
                {
                    if (!dataTable.Columns.Contains(property.Name))
                    {
                        // Infer the data type from the JSON value
                        Type dataType = property.Value.Type == JTokenType.Integer ? typeof(int) :
                            property.Value.Type == JTokenType.String ? typeof(string) :
                            property.Value.Type == JTokenType.Float ? typeof(double) :
                            property.Value.Type == JTokenType.Boolean ? typeof(bool) :
                            typeof(object);

                        dataTable.Columns.Add(property.Name, dataType);
                    }
                }
            }

            // Populate the DataTable with JSON data
            foreach (var jToken in jsonArray)
            {
                var item = (JObject)jToken;
                DataRow row = dataTable.NewRow();
                foreach (var property in item.Properties())
                {
                    row[property.Name] = property.Value.ToObject(dataTable.Columns[property.Name].DataType);
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        public static string CreateJsonString(string propertyName, object value)
        {
            var jsonObject = new JObject(new JProperty(propertyName, JToken.FromObject(value)));
            return jsonObject.ToString(Formatting.Indented);
        }

        public static string ConvertDataRowToJson(DataRow dataRow)
        {
            // Convert the DataRow to JSON
            // This is a simple example, and you might want to customize it based on your DataRow structure
            var rowData = new Dictionary<string, object>();
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                rowData[column.ColumnName] = dataRow[column];
            }

            return JsonConvert.SerializeObject(rowData, Formatting.Indented);
        }
        
        public static string MergeJsons(string json1, string json2)
        {
            // Parse the JSON strings into JObject
            JObject obj1 = JObject.Parse(json1);
            JObject obj2 = JObject.Parse(json2);

            // Merge the properties of obj2 into obj1
            foreach (var property in obj2.Properties())
            {
                obj1[property.Name] = property.Value;
            }

            // Convert the merged object back to a JSON string
            string mergedJson = obj1.ToString(Formatting.Indented);

            return mergedJson;
        }
    }
}
