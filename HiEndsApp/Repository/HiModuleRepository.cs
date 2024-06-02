using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using HiEndsApp.Model;
using HiEndsCore.Models;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace HiEndsApp.Repository
{
    public class HiModuleRepository: BaseRepository, IHiModuleRepository
    {
        private string _projectPath = string.Empty;
        private string _templatePath = string.Empty;

        public ObservableCollection<HiProjectFile> ProjectFiles { get; set; }

        public ObservableCollection<HiTemplateFile> TemplateFiles { get; set; }

        public HiModuleRepository(string projectsPath, string templatesPath)
        {
            _projectPath = projectsPath;
            _templatePath = templatesPath;
            ProjectFiles = new ObservableCollection<HiProjectFile>();
            TemplateFiles = new ObservableCollection<HiTemplateFile>();
        }

        public TreeNodeView GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNodeView> GetAll()
        {
            var modules = new ObservableCollection<TreeNodeView>
            {
                LoadExplorerTreeNode(_projectPath, typeof(HiProjectFile)),
                LoadExplorerTreeNode(_templatePath, typeof(HiTemplateFile), true)
            };
            return modules;
        }

        public void Add(TreeNodeView entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TreeNodeView entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(TreeNodeView entity)
        {
            throw new NotImplementedException();
        }

        private TreeNodeView LoadExplorerTreeNode(string rootDirectory, Type type,bool loadFileAtFirstLevel = false)
        {
            var rootDirInfo = new DirectoryInfo(rootDirectory);
            var rootNode = new TreeNodeView(rootDirInfo, rootDirInfo.Name, null, false);

            try
            {
                if (loadFileAtFirstLevel)
                {
                    LoadFileToTree(rootDirInfo, rootNode, type, "*.yaml*");
                }

                DirectoryInfo[] subDirs = rootDirInfo.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    TreeNodeView subDirNode = new TreeNodeView(dirInfo, dirInfo.Name, rootNode, false);
                    rootNode.Children.Add(subDirNode);

                    LoadFileToTree(dirInfo, subDirNode, type, "*.yaml*");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return rootNode;
        }

        private void LoadFileToTree(DirectoryInfo rootDirInfo, TreeNodeView rootNode, Type type,string patternFile)
        {
            var files = rootDirInfo.GetFiles(patternFile); // Include all files; you can customize the pattern.
            
            foreach (FileInfo fileInfo in files)
            {
                Object hiEndsFile = new object();

                if (type == typeof(HiProjectFile))
                {
                    hiEndsFile = new HiProjectFile(fileInfo);

                    ProjectFiles.Add((HiProjectFile)hiEndsFile);
                }
                if (type == typeof(HiTemplateFile))
                {
                    hiEndsFile = new HiTemplateFile(fileInfo);

                    TemplateFiles.Add((HiTemplateFile)hiEndsFile);
                }
                var fileNode = new TreeNodeView(hiEndsFile, fileInfo.Name, rootNode, false);
                rootNode.Children.Add(fileNode);
            }
        }

        public HiTemplateFile GetTemplateFileByFileName(string templateFile)
        {
            return TemplateFiles.FirstOrDefault(x => x.FileInfo.Name == Path.GetFileName(templateFile))!;
        }
    }
}
