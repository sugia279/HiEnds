using HiEndsApp.Model;
using HiEndsApp.Repository;
using HiEndsApp.View;
using HiEndsCore.Models;
using HiEndsCore.Repository;
using HiEndsCore.Services;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using YamlDotNet.Serialization;

namespace HiEndsApp.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        #region Properties
        private string _hiEndsExplorerPath;
        private string _projectsPath;
        private string _templatesPath;
        private HiProjectFileService hiProjectFileService;
        private HiTemplateFileService hiTemplateFileService;

        private HiModuleRepository _hiModuleRepository;

        private HiEndsAppSettings _appSettings;

        public HiEndsAppSettings AppSettings
        {
            get => _appSettings;
            set
            {
                _appSettings = value;
            }
        }

        private ObservableCollection<TreeNodeView> _hiendsModules;

        public ObservableCollection<TreeNodeView> HiEndsModules
        {
            get => _hiendsModules;
            set
            {
                _hiendsModules = value;
                OnPropertyChanged();
            }
        }

        private TreeNodeView _selectedNode;

        public TreeNodeView SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        private PropertyItemsView _propertyItemsView;

        public PropertyItemsView PropertyItemsView
        {
            get => _propertyItemsView;
            set
            {
                _propertyItemsView = value;
                OnPropertyChanged();
            }
        }

        private TabItem _selectedTabItem;

        public TabItem SelectedTabItem
        {
            get => _selectedTabItem;
            set
            {
                _selectedTabItem = value;
                if (_selectedTabItem is { Name: "YAML" })
                {
                    if (SelectedNode.ObjectReference is HiProjectFile file)
                    {
                        var service = new HiProjectFileService(new YamlHiProjectRepository());
                        YamlContent = new TextDocument(service.ConvertSourceObjectToString(file.SourceProject));
                    }
                    else if (SelectedNode.ObjectReference is HiTemplateFile tFile)
                    {
                        var service = new HiTemplateFileService(new YamlHiTemplateRepository());
                        YamlContent = new TextDocument(service.ConvertExtractionTemplateToString(tFile.ExTemplate));
                    }
                }
                OnPropertyChanged();
            }
        }

        private TextDocument _yamlContent;
        public TextDocument YamlContent
        {
            get => _yamlContent;
            set
            {
                _yamlContent = value;
                OnPropertyChanged();
            }
        }

        private object _currentUserControl;
        public object CurrentUserControl
        {
            get=> _currentUserControl;
            set
            {
                _currentUserControl = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage;}
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
        private HiEndsMainUserControl _hiEndsMainUserControl;
        private HiEndsMainUCViewModel _hiEndsMainUCViewModel;
        #endregion

        #region Command
        public RelayCommand YamlTextChangedCommand => new(YamlTextChangedHandler);

        private void YamlTextChangedHandler(object parameter)
        {
            try
            {
                if (SelectedNode.ObjectReference is HiProjectFile)
                {
                    hiProjectFileService.FetchToSourceObject(YamlContent.Text);

                }
                else if (SelectedNode.ObjectReference is HiTemplateFile)
                {
                    hiTemplateFileService.FetchToExtractionTemplate(YamlContent.Text);
                }

                if (ErrorMessage.StartsWith("Yaml Content: "))
                {
                    ErrorMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Yaml Content: {ex.Message}";
            }
        }
        public RelayCommand OpenExplorerCommand => new(OpenExplorerHandler);

        private void OpenExplorerHandler(object parameter)
        {
            string location = GetSelectedNodeLocation(out TreeNodeView folderNode);
            
            // opens the folder in explorer
            Process.Start("explorer.exe", location);
        }

        public RelayCommand RefreshCommand => new(RefreshHandler);

        private void RefreshHandler(object parameter)
        {
            if (SelectedNode.ObjectReference is HiProjectFile file)
            {
                SourceProject newSP = hiProjectFileService.FetchToSourceObject(file.ReadContent());
                file.SourceProject = newSP;
                PropertyItemsView = new PropertyItemsView(file.SourceProject);
            }
            else if (SelectedNode.ObjectReference is HiTemplateFile tFile)
            {
                ExtractionTemplate newET = hiTemplateFileService.FetchToExtractionTemplate(tFile.ReadContent());
                tFile.ExTemplate = newET;
            }
        }

        public RelayCommand SaveCommand => new(SaveHandler);
        
        private void SaveHandler(object parameter)
        {
            if (SelectedNode.ObjectReference is HiProjectFile file)
            {
                AppSettings.SelectedHiProjectFile = file.FileInfo.FullName.Replace(_projectsPath + "\\", "");
                UpdateHiEndsAppSetting();
                hiProjectFileService.Update(SelectedNode.ObjectReference as HiProjectFile);
                PropertyItemsView = new PropertyItemsView(file.SourceProject);
            }

            if (SelectedNode.ObjectReference is HiTemplateFile)
            {
                hiTemplateFileService.Update(SelectedNode.ObjectReference as HiTemplateFile);
            }
        }

        public RelayCommand NewFolderCommand => new RelayCommand(NewFolderHandler);

        private void NewFolderHandler(object parameter)
        {
            string location = GetSelectedNodeLocation(out TreeNodeView folderNode);

            string newFolderName = GenerateUniqueFolderName(location, "New Folder");

            string newFolderPath = Path.Combine(location, newFolderName);
            //Directory.CreateDirectory(newFolderPath);

            TreeNodeView newFolder = new TreeNodeView(new DirectoryInfo(newFolderPath), newFolderName, folderNode, true);
            folderNode.Children.Add(newFolder);

            newFolder.IsSelected = true;
            newFolder.IsEditing = true;

            OnPropertyChanged("HiEndsModules");
        }

        public RelayCommand NewFileCommand => new RelayCommand(NewFileHandler);

        private void NewFileHandler(object parameter)
        {
            string location = GetSelectedNodeLocation(out TreeNodeView folderNode);

            string newFileName = GenerateUniqueFileName(location, folderNode,"New Project File", ".yaml");

            string newProjectFilePath = Path.Combine(location, newFileName);
            //File.Create(newProjectFilePath).Close();

            HiProjectFile pFile = new HiProjectFile();
            pFile.SourceProject = new SourceProject();

            //File.WriteAllText(newProjectFilePath, hiProjectFileService.ConvertSourceObjectToString(pFile.SourceProject));

            TreeNodeView newProject = new TreeNodeView(pFile, newFileName, folderNode, true);
            folderNode.Children.Add(newProject);

            newProject.IsSelected = true;
            newProject.IsEditing = true;
        }


        public RelayCommand DeleteFileCommand => new(DeleteFileHandler);

        private void DeleteFileHandler(object parameter)
        {
            MessageBoxResult res = MessageBox.Show("Are you sure want to delete this file?","TigerRoar",MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                GetSelectedNodeLocation(out TreeNodeView folderNode);
                var item = parameter as TreeNodeView;
                if (item?.ObjectReference is HiProjectFile file)
                {
                    folderNode.Children.Remove(item);
                    File.Delete(file.FileInfo.FullName);
                }
            }
        }

        public RelayCommand AcceptEditNameCommand => new(AcceptEditNameHandler);

        private void AcceptEditNameHandler(object parameter)
        {
            var item = parameter as TreeNodeView;
            string location = GetSelectedNodeLocation(out TreeNodeView folderNode);
            if (!(item.ObjectReference is DirectoryInfo))
            {
                if (item.ObjectReference is HiProjectFile file)
                {
                    string newFileName = GenerateUniqueFileName(location, folderNode, Path.GetFileNameWithoutExtension(item.Label) ,".yaml");

                    string newProjectFilePath = Path.Combine(location, newFileName);
                    File.Create(newProjectFilePath).Close();
                    File.WriteAllText(newProjectFilePath, hiProjectFileService.ConvertSourceObjectToString(file.SourceProject));
                    file.FileInfo = new FileInfo(newProjectFilePath);
                    file.ReadContent();
                }
            }
            else
            {
                string newFolderName = GenerateUniqueFolderName(location, item.Label);
                string newFolderPath = Path.Combine(location, newFolderName);
                Directory.CreateDirectory(newFolderPath);
            }
            
        }

        public RelayCommand SaveRefreshYamlCommand => new(SaveRefreshYamlHandler);

        private void SaveRefreshYamlHandler(object parameter)
        {
            if (ErrorMessage.StartsWith("Yaml Content: "))
            {
                MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                if (SelectedNode.ObjectReference is HiProjectFile file)
                {
                    SourceProject newSP = hiProjectFileService.FetchToSourceObject(YamlContent.Text);
                    file.SourceProject = newSP;
                    PropertyItemsView = new PropertyItemsView(file.SourceProject);
                }
                else if (SelectedNode.ObjectReference is HiTemplateFile tFile)
                {
                    ExtractionTemplate newET = hiTemplateFileService.FetchToExtractionTemplate(YamlContent.Text);
                    tFile.ExTemplate = newET;
                }
                SaveHandler(null);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public RelayCommand OpenFileCommand => new(OpenFileHandler);
        private void OpenFileHandler(object parameter)
        {
            SelectedNode = parameter as TreeNodeView;
            var fileObject = SelectedNode.ObjectReference;
            //Handle opening the .yaml file
            if (fileObject is HiProjectFile file && !string.IsNullOrEmpty(file.Content))
            {
                file.SourceProject = hiProjectFileService.FetchToSourceObject(file.Content);

                YamlContent = new TextDocument(file.Content);
                _hiEndsMainUCViewModel.SelectedHiProject = file;
                if (file.SourceProject != null)
                {
                    PropertyItemsView = new PropertyItemsView(file.SourceProject);
                    var templateFile = Path.Combine(file.FolderPath, file.SourceProject.Extract.TemplateFile);
                    _hiEndsMainUCViewModel.DisplayDataTable.Clear();
                    if (File.Exists(templateFile))
                    {
                        _hiEndsMainUCViewModel.SelectedTemplateFile =
                            _hiModuleRepository.GetTemplateFileByFileName(templateFile);
                    }
                    else
                    {
                        ErrorMessage = "The extraction template file does not exist. Please specify another one.";
                    }
                }

                CurrentUserControl = _hiEndsMainUserControl;
            }

            if (fileObject is HiTemplateFile tFile)
            {
                YamlContent = new TextDocument(tFile.Content);
            }
        }

        

        #endregion

        public MainWindowViewModel()
        {
            try
            {
                hiProjectFileService = new HiProjectFileService(new YamlHiProjectRepository());
                hiTemplateFileService = new HiTemplateFileService(new YamlHiTemplateRepository());
                ErrorMessage = string.Empty;

                InitHiEndsPaths();

                InitModulesExplorerTreeView();

                InitExtractionTemplates();

                AppSettings = LoadHiEndsAppSetting();

                _hiEndsMainUserControl = new HiEndsMainUserControl();
                _hiEndsMainUCViewModel = (_hiEndsMainUserControl.DataContext as HiEndsMainUCViewModel)!;
                _hiEndsMainUCViewModel.TemplateFiles = _hiModuleRepository.TemplateFiles;
                SelectHiProjectFile(new HiProjectFile(Path.Combine(_projectsPath,AppSettings.SelectedHiProjectFile))
                    .FileInfo.Name);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"HiEndsApp - Error loading. {ex}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void InitHiEndsPaths()
        {
            _hiEndsExplorerPath = Path.Combine(Directory.GetCurrentDirectory(), HiEndsConstants.HiEndsExplorerDirName);
            _projectsPath = Path.Combine(_hiEndsExplorerPath, HiEndsConstants.ProjectsDirName);
            _templatesPath = Path.Combine(_hiEndsExplorerPath, HiEndsConstants.TemplatesDirName);
        }

        private void InitModulesExplorerTreeView()
        {
            // Specify the root directory of your project explorer
            _hiModuleRepository = new HiModuleRepository(_projectsPath, _templatesPath);

            // Get the projects from the ProjectExplorerViewModel
            HiEndsModules = (ObservableCollection<TreeNodeView>)_hiModuleRepository.GetAll();

            //TemplateFiles = _hiModuleRepository.TemplateFiles;
        }

        private void InitExtractionTemplates()
        {
            foreach (var template in _hiModuleRepository.TemplateFiles)
            {
                template.ExTemplate = hiTemplateFileService.FetchToExtractionTemplate(template.Content);
            }
        }

        private HiEndsAppSettings LoadHiEndsAppSetting()
        {
            var deserializer = new DeserializerBuilder().Build();
            string project = File.ReadAllText(HiEndsConstants.AppSettingsFile);
            return deserializer.Deserialize<HiEndsAppSettings>(project);
        }

        private void UpdateHiEndsAppSetting()
        {
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull) // Ignore null properties
                .Build();

            // Serialize the object to YAML
            var content = serializer.Serialize(AppSettings);
            File.WriteAllText(HiEndsConstants.AppSettingsFile, content);
        }

        private void SelectHiProjectFile(string fileName)
        {
            foreach (var node in HiEndsModules)
            {
                var selectedItem = node.FindChild(fileName);
                if (selectedItem != null)
                {
                    SelectedNode = selectedItem;
                    selectedItem.IsSelected = true;
                    break;
                }
            }
        }

        private string GetSelectedNodeLocation(out TreeNodeView folderNode)
        {
            string location;
            folderNode = SelectedNode;
            if (!(SelectedNode.ObjectReference is DirectoryInfo))
            {
                folderNode = SelectedNode.Parent;
                location = ((SelectedNode.Parent.ObjectReference as DirectoryInfo)!).FullName;
            }
            else
            {
                location = ((SelectedNode.ObjectReference as DirectoryInfo)!).FullName;
            }
            return location;
        }

        private string GenerateUniqueFolderName(string location, string baseFolderName)
        {
            string newFolderName = baseFolderName;
            int count = 1;
            while (Directory.Exists(Path.Combine(location, newFolderName)))
            {
                newFolderName = $"{baseFolderName} ({count++})";
            }
            return newFolderName;
        }

        private string GenerateUniqueFileName(string location, TreeNodeView folderNode , string baseFileName, string extension)
        {
            string newFileName = baseFileName + extension;
            int count = 1;
            while (File.Exists(Path.Combine(location, newFileName)) || folderNode.FindChild(newFileName) != null)
            {
                newFileName = $"{baseFileName} ({count++}){extension}";
            }
            return newFileName;
        }

    }
}
