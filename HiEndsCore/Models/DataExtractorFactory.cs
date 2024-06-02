using System.Reflection;
using HiEndsCore.Interface;

namespace HiEndsCore.Models
{
    // DataExtractorFactory.cs
    public static class DataExtractorFactory
    {
        public static IDataExtractor CreateDataExtractor(ExtractionTemplate template)
        {
            // Load the SourceDataExtractor.dll
            Assembly dataExtractorAssembly = Assembly.LoadFrom(template.DataDriver);

            // Get the type of the IDataExtractor based on the Type property from SourceProject
            Type dataExtractorType =
                dataExtractorAssembly.GetTypes().FirstOrDefault(type =>
                    type.GetInterfaces().Contains(typeof(IDataExtractor)) &&
                    type.Name == template.Type + "DataExtractor") ??
                throw new InvalidOperationException($"Data extractor for '{template.Type}' not found in {template.DataDriver}.");

            IDataExtractor dataExtractor = Activator.CreateInstance(dataExtractorType) as IDataExtractor ??
                                           throw new InvalidOperationException("Failed to create data extractor.");
            
            return dataExtractor;
        }
    }

}
