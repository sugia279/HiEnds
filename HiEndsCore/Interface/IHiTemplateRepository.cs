using HiEndsCore.Models;
using System.Data;

namespace HiEndsCore.Interface;

public interface IHiTemplateRepository : IRepository<HiTemplateFile>
{
    string ConvertExtractionTemplateToString(ExtractionTemplate entity);

    ExtractionTemplate FetchToExtractionTemplate(string content);
}