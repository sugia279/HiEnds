using System.Collections.ObjectModel;
using System.Net;
using HiEndsCore.Interface;
using YamlDotNet.Serialization;

namespace HiEndsCore.Models
{
    public abstract class HiEndsFile<T>
    {
        protected IRepository<T> Repository { get; set; }

        public FileInfo FileInfo { get; set; }

        public string Content { get; set; }

        public string ReadContent()
        {
            if (FileInfo != null && File.Exists(FileInfo.FullName))
                Content = File.ReadAllText(FileInfo.FullName);
            else
            {
                Content = string.Empty;
            }
            return Content;
        }

    }

    public class HiProjectFile : HiEndsFile<SourceProject>
    {
        public SourceProject SourceProject { get; set; }

        public string FolderPath => FileInfo.Directory!.FullName;
        
        public HiProjectFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            ReadContent();
        }

        public HiProjectFile(string filePath)
        {
            FileInfo = new FileInfo(filePath);
            ReadContent();    
        }

        public HiProjectFile(){}
    }

    public class HiTemplateFile : HiEndsFile<ExtractionTemplate>
    {
        public ExtractionTemplate ExTemplate { get; set; }

        public HiTemplateFile(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            ReadContent();
        }

    }

}