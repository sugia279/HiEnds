using System.Data;
using HiEndsCore.Models;

namespace HiEndsCore.Interface
{
    public enum DataType
    {
        json,
        xml,
        csv,
        text
    }

    public interface IDataExtractor
    {
        string Name { get; }

        T ExtractData<T>(string sourcePath, ExtractionTemplate template);

        T TransformData<T>(T data, ExtractionTemplate template, string filter);

        T MergeToExistingData<T>(T data, string existingData, DataType type);

        string LoadDataAsString<T>(T data, DataType type);

    }
}
