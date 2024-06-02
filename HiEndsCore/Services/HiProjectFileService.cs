using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiEndsCore.Interface;
using HiEndsCore.Models;

namespace HiEndsCore.Services
{
    // Service that uses the repository, following Dependency Injection
    public class HiProjectFileService
    {
        private readonly IHiProjectRepository _repository;

        public HiProjectFileService(IHiProjectRepository repository)
        {
            _repository = repository;
        }

        public SourceProject FetchToSourceObject(string content)
        {
            return _repository.FetchToSourceObject(content);
        }

        public void Update(HiProjectFile hiProjectFile)
        {
            _repository.Update(hiProjectFile);
        }

        public string ConvertSourceObjectToString(SourceProject entity)
        {
            return _repository.ConvertSourceObjectToString(entity);
        }
    }
}
