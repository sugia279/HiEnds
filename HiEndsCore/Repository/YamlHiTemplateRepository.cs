using HiEndsCore.Interface;
using HiEndsCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Helper;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace HiEndsCore.Repository
{
    public class YamlHiTemplateRepository : IHiTemplateRepository
    {
        public void Add(HiTemplateFile entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(HiTemplateFile entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<HiTemplateFile> GetAll()
        {
            throw new NotImplementedException();
        }

        public HiTemplateFile GetById(int id)
        {
            throw new NotImplementedException();
        }
        
        public ExtractionTemplate FetchToExtractionTemplate(string content)
        {
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<ExtractionTemplate>(content);
        }


        public void Update(HiTemplateFile entity)
        {
            // Serialize the object to YAML
            var yaml = ConvertExtractionTemplateToString(entity.ExTemplate);
            File.WriteAllText(entity.FileInfo.FullName, yaml);
        }

        public string ConvertExtractionTemplateToString(ExtractionTemplate entity)
        {
            // Configure YAML serialization
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults) // Ignore null properties
                .Build();

            // Serialize the object to YAML
            return serializer.Serialize(entity);
        }
    }
}
