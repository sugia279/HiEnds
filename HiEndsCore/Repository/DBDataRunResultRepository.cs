using HiEndsCore.Interface;
using HiEndsCore.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiEndsCore.Repository
{
    // Implement a database resultRepository (e.g., for MSSQL)
    public class DBDataRunResultRepository : IDataRunResultRepository
    {
        // Add dependencies as needed, e.g., connection string, ORM, etc.

        public void SaveDataRun(DataTable dataRun)
        {
            // Implement logic to save DataTable to a database
            // You can use an ORM (e.g., Entity Framework) or raw SQL queries here
            // Example using Entity Framework:
            //using (var dbContext = new YourDatabaseContext())
            //{
            //    // Map DataTable to your database model and save
            //    // dbContext.YourDataRuns.AddRange(MapDataTableToEntities(dataRun));
            //    // dbContext.SaveChanges();
            //}
        }
    }
}
