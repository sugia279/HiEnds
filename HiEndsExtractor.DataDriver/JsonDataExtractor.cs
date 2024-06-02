using HiEndsCore.Interface;
using HiEndsCore.Models;
using System.Data;

namespace HiEndsExtractor.DataDriver
{
    internal class JsonDataExtractor:IDataExtractor
    {
        public string Name => "Json";
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
