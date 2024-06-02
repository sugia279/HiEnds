using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Models;

namespace HiEndsCore.Interface
{
    public interface IDataRunResultRepository
    {
        public void SaveDataRun(DataTable dataRun);
    }
}
