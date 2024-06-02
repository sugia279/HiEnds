using HiEndsCore.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Helper;
using HiEndsCore.Models;

namespace HiEndsExtractor.DataDriver
{
    public class CsvDataExtractor : IDataExtractor
    {
        public string Name => "Csv";
        public T ExtractData<T>(string sourcePath, ExtractionTemplate template)
        {
            string csvAllText = File.ReadAllText(sourcePath);
            bool hasHeader = template.LoadedAttributes.Contains("HasHeader");
            DataTable dt = CsvUtilities.CsvToDataTable(csvAllText, hasHeader);
            return (T)(object)dt;
        }

        public T TransformData<T>(T data, ExtractionTemplate template, string filter)
        {
            DataTable dataTable = (data as DataTable)!;

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
            return (T)(object)data;
        }

        public string LoadDataAsString<T>(T data, DataType type)
        {
            throw new NotImplementedException();
        }
    }
}
