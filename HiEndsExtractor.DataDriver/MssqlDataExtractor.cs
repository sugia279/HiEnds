using HiEndsCore.Interface;
using HiEndsCore.Models;
using System.Data;

namespace HiEndsExtractor.DataDriver
{
    // MssqlDataExtractor.cs

    public class MssqlDataExtractor : IDataExtractor
    {
        public string Name => "MSSQL";
        public T ExtractData<T>(string sourcePath, ExtractionTemplate template)
        {
            throw new NotImplementedException();
        }

        public T TransformData<T>(T data, ExtractionTemplate template, string filter)
        {
            throw new NotImplementedException();
        }

        public T MergeToExistingData<T>(T data, string existingData, DataType type)
        {
            throw new NotImplementedException();
        }

        public string LoadDataAsString<T>(T data, DataType type)
        {
            throw new NotImplementedException();
        }
    }

}
