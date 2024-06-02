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
    public class HiTemplateFileService
    {
        private readonly IHiTemplateRepository _repository;

        public HiTemplateFileService(IHiTemplateRepository repository)
        {
            _repository = repository;
        }

        public ExtractionTemplate FetchToExtractionTemplate(string content)
        {
            return _repository.FetchToExtractionTemplate(content);
        }

        public void Update(HiTemplateFile hiProjectFile)
        {
            _repository.Update(hiProjectFile);
        }

        public string ConvertExtractionTemplateToString(ExtractionTemplate entity)
        {
            return _repository.ConvertExtractionTemplateToString(entity);
        }
    }
}
