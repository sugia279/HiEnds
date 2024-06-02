using HiEndsCore.Models;
using System.Data;

namespace HiEndsCore.Interface;

public interface IHiProjectRepository : IRepository<HiProjectFile>
{
    string ConvertSourceObjectToString(SourceProject entity);

    SourceProject FetchToSourceObject(string content);
}