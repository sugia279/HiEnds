using HiEndsCore.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Models;

namespace HiEndsCore.Repository
{
    // Implement a JSON resultRepository
    public class JsonDataRunResultRepository : IDataRunResultRepository
    {
        public void SaveDataRun(DataTable dataRun)
        {
            // Implement logic to save DataTable to a JSON file
            string jsonData = ConvertDataTableToJson(dataRun);
            // Save jsonData to a file or another storage
            File.WriteAllText("DataRun.json", jsonData);
        }

        private string ConvertDataTableToJson(DataTable dataTable)
        {
            // Implement logic to convert DataTable to JSON
            // You can use a library like Newtonsoft.Json for this
            string jsonData = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            return jsonData;
        }
        
    }

}
