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
    public class YamlHiProjectRepository : IHiProjectRepository
    {
        public void Add(HiProjectFile entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(HiProjectFile entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<HiProjectFile> GetAll()
        {
            throw new NotImplementedException();
        }

        public HiProjectFile GetById(int id)
        {
            throw new NotImplementedException();
        }

        public SourceProject FetchToSourceObject(string content)
        {
            var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            return deserializer.Deserialize<SourceProject>(content);
        }

        public void Update(HiProjectFile entity)
        {
            // Serialize the object to YAML
            entity.Content = ConvertSourceObjectToString(entity.SourceProject);
            File.WriteAllText(entity.FileInfo.FullName, entity.Content);
        }

        public string ConvertSourceObjectToString(SourceProject entity)
        {
            // Configure YAML serialization
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) // Ignore null properties
                .Build();

            // Serialize the object to YAML
            return serializer.Serialize(entity);
        }
    }
}
