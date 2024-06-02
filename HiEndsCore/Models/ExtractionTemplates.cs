namespace HiEndsCore.Models
{
    public class ExtractionTemplates
    {
        public List<ExtractionTemplate> SourceTemplatesList { get; set; }
    }

    public class ExtractionTemplate
    {
        public ExtractionTemplate()
        {
            BrowseType = "";
            Type = "";
            DataDriver = "";
            FilterConditions = new FilterConditions();
            LoadedAttributes = new List<string>();
            Remap = new Dictionary<string, string>();
            AttributeNotExistString = "";
            NullValueString = "";
        }

        public ExtractionTemplate(string browseType, string type, string dataDriver, FilterConditions filterConditions, List<string> loadedAttributes, IDictionary<string, string> remap, string attributeNotExistString, string nullValueString)
        {
            BrowseType = browseType;
            Type = type;
            DataDriver = dataDriver;
            FilterConditions = filterConditions;
            LoadedAttributes = loadedAttributes;
            Remap = remap;
            AttributeNotExistString = attributeNotExistString;
            NullValueString = nullValueString;
        }

        public string BrowseType { get; set; }
        public string Type { get; set; }
        public string DataDriver { get; set; }
        public FilterConditions FilterConditions { get; set; }
        public List<string> LoadedAttributes { get; set; }
        public IDictionary<string, string> Remap { get; set; }
        public string AttributeNotExistString { get; set; }
        public string NullValueString { get; set; }
    }

    // FilterConditions.cs
    public class FilterConditions
    {
        public FilterConditions()
        {
            //Namespaces = new List<string>();
            //Classes = new List<string>();
            //Attributes = new List<string>();
            //JsonPath = "";
            //QueryCommand = "";
        }

        public List<string> Namespaces { get; set; }

        public List<string> Classes { get; set; }

        public List<string> Attributes { get; set; }
        
        public string JsonPath { get; set; }
        
        public string QueryCommand { get; set; }
    }

}
