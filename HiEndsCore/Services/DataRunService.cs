using HiEndsCore.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Models;

namespace HiEndsCore.Services
{
    // Service that uses the resultRepository, following Dependency Injection
    public class DataRunService
    {
        private readonly IDataRunResultRepository _resultRepository;

        public DataRunService(IDataRunResultRepository resultRepository)
        {
            _resultRepository = resultRepository;
        }

        public void SaveDataRun(Dictionary<DataRow, RunCommand[]> dataRun)
        {
            //_resultRepository.Add(dataRunResult);
        }
    }
}
