using HiEndsCore.Interface;
using HiEndsCore.Models;

namespace HiEndsExtractor.DataDriver
{
    // DirectoryDataExtractor.cs
    public class DirectoryDataExtractor : IDataExtractor
    {
        public string Name => "Directory";
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
