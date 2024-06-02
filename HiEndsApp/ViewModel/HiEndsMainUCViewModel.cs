using HiEndsCore.Helper;
using HiEndsCore.Interface;
using HiEndsCore.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace HiEndsApp.ViewModel
{
    public class HiEndsMainUCViewModel : BaseViewModel
    {
        #region Properties
        
        private ConcurrentQueue<int> _inQueueDataRows; // Queue for in-queue rows
        private int _runningTasksCount = -1; // Keep track of the number of running tasks
        private int _threadsNumber = 0; // Keep track of the number of running tasks
        private readonly object _inQueueLock;
        private readonly DispatcherTimer _updateLogResultTimer = new();

        private DataRunResult _dataRunResult;

        #region Search Properties

        private List<string> _searchProperties;
        public List<string> SearchProperties
        {
            get => _searchProperties;
            set
            {
                _searchProperties = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _searchQuery;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value; 
                OnPropertyChanged();
            }
        }

        private List<string> _searchQueries;

        public List<string> SearchQueries
        {
            get => _searchQueries;
            set
            {
                _searchQueries = value;
                OnPropertyChanged();
            }
        }

        private string _selectedLogicSearch;

        public string SelectedLogicSearch
        {
            get => _selectedLogicSearch;
            set
            {
                _selectedLogicSearch = value;
                OnPropertyChanged();
            }
        }

        private string _selectedSearchProperty;

        public string SelectedSearchProperty
        {
            get => _selectedSearchProperty;
            set
            {
                _selectedSearchProperty = value;
                OnPropertyChanged();
            }
        }

        private string _selectedOperatorSearch;

        public string SelectedOperatorSearch
        {
            get => _selectedOperatorSearch;
            set
            {
                _selectedOperatorSearch = value;
                OnPropertyChanged();
            }
        }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                _selectedTabIndex = value;
                FindHandler(_selectedTabIndex);
                OnPropertyChanged();
            }
        }
        #endregion

        private HiProjectFile _selectedHiProject;
        public HiProjectFile SelectedHiProject
        {
            get => _selectedHiProject;
            set
            {
                _selectedHiProject = value;

                SourceProjectObject = _selectedHiProject.SourceProject;
                if (SourceProjectObject != null)
                {
                    SearchQuery = SourceProjectObject.Extract.Query;
                    SearchQueries = SourceProjectObject.Extract.Queries;
                    LoadResultFolders(Path.GetDirectoryName(SourceProjectObject.Run.OutputFolder)!);
                    SelectedResultFolder = Path.GetFileName(SourceProjectObject.Run.OutputFolder);
                }

                OnPropertyChanged();
            }
        }


        private SourceProject _sourceProject;
        public SourceProject SourceProjectObject
        {
            get => _sourceProject;
            set
            {
                _sourceProject = value;
                if (SourceProjectObject != null)
                {
                    SourceProjectPath = _sourceProject.Extract.SourcePath;
                    _threadsNumber =
                        _sourceProject.Run.RunThreadNumber == null || _sourceProject.Run.RunThreadNumber == 0
                            ? 1
                            : _sourceProject.Run.RunThreadNumber ?? 1;
                }

                //_threadsNumber = 2;
                OnPropertyChanged();
            }
        }

        private string _sourceProjectPath;
        public string SourceProjectPath
        {
            get => _sourceProjectPath;
            set
            {
                _sourceProjectPath = value;
                if (!Utilities.IsAbsolutePath(_sourceProjectPath))
                {
                    _sourceProjectPath = Path.GetFullPath(_sourceProjectPath);
                    SelectedHiProject.SourceProject.Vars[ConstString.SourceFolderPathVariable] =
                        Path.GetDirectoryName(_sourceProjectPath);
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<HiTemplateFile> _templateFiles;
        public ObservableCollection<HiTemplateFile> TemplateFiles
        {
            get => _templateFiles;
            set
            {
                _templateFiles = value;
                OnPropertyChanged();
            }
        }

        private HiTemplateFile _selectedTemplateFile;
        public HiTemplateFile SelectedTemplateFile
        {
            get => _selectedTemplateFile;
            set
            {
                if (value != null)
                {
                    _selectedTemplateFile = value;
                    _selectedTemplateFile.ExTemplate.DataDriver ??= "HiEndsExtractor.DataDriver.dll";
                    _sourceProject.Extract.TemplateFile = Utilities.ConvertAbsolutePathToRelative(_selectedTemplateFile.FileInfo.FullName, _selectedHiProject.FolderPath);
                    RefreshHandler(null);
                    OnPropertyChanged();
                }
            }
        }

        public DataTable DisplayDataTable { get; set; }

        public List<DataRow> SelectedRows
        {
            get;
            set;
        }

        private DataRow _selectedRow;

        public DataRow SelectedRow
        {
            get => _selectedRow;
            set
            {
                _selectedRow = value;
                UpdateRunResultView();
                OnPropertyChanged();
            }
        }

        private int _selectedRowIndex;

        public int SelectedRowIndex
        {
            get => _selectedRowIndex;
            set
            {
                _selectedRowIndex = value;
                OnPropertyChanged();
            }
        }

        private bool _browseRunCommandEnable;
        public bool BrowseRunCommandEnable
        {
            get => _browseRunCommandEnable;
            set
            {
                _browseRunCommandEnable = value;
                OnPropertyChanged();
            }
        }

        private RunCommand _selectedRunCommand;

        public RunCommand SelectedRunCommand
        {
            get => _selectedRunCommand;
            set
            {
                _selectedRunCommand = value;
                BrowseRunCommandEnable = _selectedRunCommand != null;
                OnPropertyChanged();
            }
        }

        private int _selectedRunIndex;

        public int SelectedRunIndex
        {
            get => _selectedRunIndex;
            set
            {
                if (_selectedRunIndex != value)
                {
                    _selectedRunIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Result
        private string _selectedOutputFile;
        public string SelectedOutputFile
        {
            get => _selectedOutputFile;
            set
            {
                _selectedOutputFile = value;
                OnPropertyChanged();
            }
        }

        private RunResult[] _runResults;
        public RunResult[] RunResults
        {
            get => _runResults;
            set
            {
                _runResults = value;
                if(_runResults.Length > 0)
                    SelectedRunResult = _runResults[0];
                OnPropertyChanged();
            }
        }

        private RunResult _selectedRunResult;
        public RunResult SelectedRunResult
        {
            get => _selectedRunResult;
            set
            {
                _selectedRunResult = value ?? new RunResult();
                RunCommandResults = _selectedRunResult.RunCommandResults;
                OnPropertyChanged();
            }
        }

        private RunCommandResult[] _runCommandResults;
        public RunCommandResult[] RunCommandResults
        {
            get => _runCommandResults;
            set
            {
                _runCommandResults = value;
                if (_runCommandResults?.Length > 0)
                    SelectedRunCommandResult = _runCommandResults[0];
                OnPropertyChanged();
            }
        }

        private RunCommandResult _selectedRunCommandResult;
        public RunCommandResult SelectedRunCommandResult
        {
            get => _selectedRunCommandResult;
            set
            {
                _selectedRunCommandResult = value;
                OnPropertyChanged();
            }
        }

        private string[] _resultFolders;

        public string[] ResultFolders
        {
            get => _resultFolders;
            set
            {
                _resultFolders = value;
                OnPropertyChanged();
            }
        }

        private string _selectedResultFolders;

        public string SelectedResultFolder
        {
            get => _selectedResultFolders;
            set
            {
                _selectedResultFolders = value;
                OnPropertyChanged();
            }
        }

        private string _showAllCount;
        public string ShowAllCount
        {
            get => _showAllCount;
            set
            {
                _showAllCount = value;
                OnPropertyChanged();
            }
        }

        private string _passedCount;
        public string PassedCount
        {
            get => _passedCount;
            set
            {
                _passedCount = value;
                OnPropertyChanged();
            }
        }

        private string _failedCount;
        public string FailedCount
        {
            get => _failedCount;
            set
            {
                _failedCount = value;
                OnPropertyChanged();
            }
        }

        private string _inQueueCount;

        public string InQueueCount
        {
            get => _inQueueCount;
            set
            {
                _inQueueCount = value;
                OnPropertyChanged();
            }
        }

        private string _runningCount;

        public string RunningCount
        {
            get => _runningCount;
            set
            {
                _runningCount = value;
                OnPropertyChanged();
            }
        }

        private string _runTotalCount;

        public string RunTotalCount
        {
            get => _runTotalCount;
            set
            {
                _runTotalCount = value;
                OnPropertyChanged();
            }
        }
        #endregion
        #endregion

        public HiEndsMainUCViewModel()
        {
            _inQueueDataRows = new();
            _inQueueLock = new();
            SelectedRows = new List<DataRow>();
            DisplayDataTable = new DataTable();
            _updateLogResultTimer.Tick += UpdateLogForSelectedRow_Tick;
            _updateLogResultTimer.Interval = new TimeSpan(0, 0, 5);
        }

        #region Browse Feature
        public RelayCommand BrowseCommand => new(BrowseHandler);

        private void BrowseHandler(object parameter)
        {
            try
            {
                var browseType = SelectedTemplateFile.ExTemplate.BrowseType;
                var filter = browseType.Replace("File:", "");
                if (browseType.StartsWith("File:"))
                {
                    var fileDialog = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = filter,
                        InitialDirectory = Path.GetFullPath(SelectedHiProject.SourceProject.Extract.SourcePath)
                    };

                    fileDialog.ShowDialog();
                    if (!string.IsNullOrEmpty(fileDialog.FileName))
                    {
                        SelectedHiProject.SourceProject.Extract.SourcePath = fileDialog.FileName;
                        if (SelectedHiProject.SourceProject.Vars.ContainsKey(ConstString.SourceFolderPathVariable))
                        {
                            SelectedHiProject.SourceProject.Vars[ConstString.SourceFolderPathVariable] =
                                Path.GetDirectoryName(fileDialog.FileName)!;
                        }
                        else
                        {
                            SelectedHiProject.SourceProject.Vars.Add(ConstString.SourceFolderPathVariable,
                                Path.GetDirectoryName(fileDialog.FileName)!);
                        }

                        if (SelectedHiProject.SourceProject.Vars.ContainsKey(ConstString.SourceFileNameVariable))
                        {
                            SelectedHiProject.SourceProject.Vars[ConstString.SourceFileNameVariable] =
                                Path.GetFileName(fileDialog.FileName)!;
                        }
                        else
                        {
                            SelectedHiProject.SourceProject.Vars.Add(ConstString.SourceFileNameVariable,
                                Path.GetFileName(fileDialog.FileName)!);
                        }
                    }
                }
                else if (browseType.StartsWith("Folder:"))
                {
                    var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                    folderDialog.SelectedPath = SelectedTemplateFile.ExTemplate.BrowseType.Replace("Folder:", "");
                    folderDialog.ShowDialog();
                    if (!string.IsNullOrEmpty(folderDialog.SelectedPath))
                    {
                        SelectedHiProject.SourceProject.Extract.SourcePath = $"{folderDialog.SelectedPath}";
                        //SelectedTemplateFile.SaveToFile();
                    }
                }

                RefreshHandler(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: Can not browse the file or folder: {ex.Message}.", "HiEndsApp", MessageBoxButton.OK);
            }
        }

        public RelayCommand RefreshCommand => new(RefreshHandler);

        private void RefreshHandler(object parameter)
        {
            DisplayDataTable.Clear();
            ExtractDataByTemplate();
            OnPropertyChanged(nameof(SourceProjectObject));
            SourceProjectPath = SourceProjectObject.Extract.SourcePath;
            SelectedTabIndex = 0;
        }
        #endregion


        #region Find Feature
        public RelayCommand FindCommand => new(FindHandler);

        private void FindHandler(object parameter)
        {
            if (parameter !=null && (int)parameter == -1) return;

            
            if (!string.IsNullOrEmpty(SearchText))
            {
                bool validSearchQuery = true;
                //var operatorSearch = SelectedOperatorSearch.Equals("Contains") ? "Like" : SelectedOperatorSearch;
                //var searchText = SelectedOperatorSearch.Equals("Contains") ? $"%{SearchText}%" : SearchText;
                var operatorSearch = "Like";
                var searchText = $"%{SearchText}%";

                var query = $"{SelectedSearchProperty} {operatorSearch} '{searchText}'";

                if (SelectedLogicSearch != "----")
                {
                    if (string.IsNullOrEmpty(SearchQuery))
                        SelectedLogicSearch = "----";

                    if (!string.IsNullOrEmpty(SearchQuery))
                    {
                        if(!SearchQuery.Contains(query))
                            query = $"{SearchQuery} {SelectedLogicSearch} {query}";
                        else
                        {
                            DataRow[] foundRows = DisplayDataTable.Select(query);
                            if (foundRows.Length > 0)
                            {
                                SelectedRowIndex = DisplayDataTable.Rows.IndexOf(foundRows[0]);
                                return;
                            }
                        }
                    }
                    else
                    {
                        validSearchQuery = false;
                    }
                }

                if (validSearchQuery)
                    SearchQuery = query;
            }

            try
            {
                if (_dataRunResult == null) return;

                // Assuming dataTable is your DataTable
                DataView dataView = new DataView(_dataRunResult.DataRun);
                dataView.RowFilter = SearchQuery;

                // Access the filtered rows
                DataRowView[] filteredRows = dataView.Cast<DataRowView>().ToArray();

                //DataRow[] filteredRows = _dataRunResult.DataRun.Select(SearchQuery);
                DisplayDataTable.Clear();
                DisplayDataTable = _dataRunResult.DataRun.Clone();

                foreach (DataRowView rowView in filteredRows)
                {
                    var row = rowView.Row;
                    switch (SelectedTabIndex)
                    {
                        case 1:
                            if ((string)row[ConstString.RunStatus] == nameof(RunStatus.Passed))
                                DisplayDataTable.ImportRow(row);
                            break;
                        case 2:
                            if ((string)row[ConstString.RunStatus] == nameof(RunStatus.Failed))
                                DisplayDataTable.ImportRow(row);
                            break;
                        case 3:
                            if ((string)row[ConstString.RunStatus] == nameof(RunStatus.InQueue))
                                DisplayDataTable.ImportRow(row);
                            break;
                        case 4:
                            if ((string)row[ConstString.RunStatus] == nameof(RunStatus.Running))
                                DisplayDataTable.ImportRow(row);
                            break;
                        default:
                            DisplayDataTable.ImportRow(row);
                            break;
                    }
                    
                }

                OnPropertyChanged(nameof(DisplayDataTable));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public RelayCommand ClearQueryCommand => new(ClearQueryHandler);
        private void ClearQueryHandler(object parameter)
        {
            SearchQuery = string.Empty;
            SearchText = string.Empty;
            SelectedLogicSearch = "----";
            FindHandler(SelectedTabIndex);
        }

        public RelayCommand SaveQueryCommand => new(SaveQueryHandler);
        private void SaveQueryHandler(object parameter)
        {
            SourceProjectObject.Extract.AddQuery(SearchQuery);
            SearchQueries = SourceProjectObject.Extract.Queries;
        }

        #endregion
        #region Data Editor Feature
        public RelayCommand CellEditEndingCommand => new(CellEditEndingHandler);

        private void CellEditEndingHandler(object parameter)
        {
            DataGridCellEditEndingEventArgs dataGridCellEditEndingEventArgs = (DataGridCellEditEndingEventArgs)parameter;
            if (dataGridCellEditEndingEventArgs.EditAction == DataGridEditAction.Commit)
            {
                var row = (DataRowView)dataGridCellEditEndingEventArgs.Row.Item;
                var column = dataGridCellEditEndingEventArgs.Column.DisplayIndex;
                var value = ((TextBox)dataGridCellEditEndingEventArgs.EditingElement).Text;
                var rowId = (int)row.Row[ConstString.RowId];
                if (dataGridCellEditEndingEventArgs.Column.Header.Equals(ConstString.RowId))
                {
                    int newRowId = int.Parse(value);
                    if (newRowId == rowId || _dataRunResult.GetRowFromDataRun(newRowId) != null)
                    {
                        MessageBox.Show("Row ID already exists", "HiEndsApp", MessageBoxButton.OK);
                        return;
                    }
                }

                _dataRunResult.GetRowFromDataRun(rowId)[column] = value;
                row.Row[column] = value;
                OnPropertyChanged(nameof(DisplayDataTable));
            }

        }

        public RelayCommand AddDataRowCommand => new(AddDataRowHandler);

        private void AddDataRowHandler(object parameter)
        {
            int i = _dataRunResult.GetNextRowId();
            var newRow = _dataRunResult.DataRun.NewRow();
            newRow[ConstString.RowId] = i;
            _dataRunResult.InitRowData(newRow);
            _dataRunResult.DataRun.Rows.Add(newRow);
            var newRow1 = DisplayDataTable.NewRow();
            newRow1[ConstString.RowId] = i;
            _dataRunResult.InitRowData(newRow1);
            DisplayDataTable.Rows.Add(newRow1);

            OnPropertyChanged(nameof(DisplayDataTable));
            SelectedRowIndex = DisplayDataTable.Rows.Count - 1;
        }

        public RelayCommand DeleteDataRowCommand => new(DeleteDataRowHandler);

        private void DeleteDataRowHandler(object parameter)
        {
            if (SelectedRow != null)
            {
                var rowId = (int)SelectedRow[ConstString.RowId];
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this row '{ConstString.RowId + " - "+ rowId}'?", "HiEndsApp", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _dataRunResult.DataRun.Rows.Remove(_dataRunResult.GetRowFromDataRun(rowId));
                    DisplayDataTable.Rows.Remove(SelectedRow);
                    OnPropertyChanged(nameof(DisplayDataTable));
                }
            }
        }

        public RelayCommand UpDataRowCommand => new(UpDataRowHandler);

        private void UpDataRowHandler(object parameter)
        {
            if (SelectedRow != null)
            {
                // Move the selected item up
                if (SelectedRowIndex > 0)
                {
                    var selectedRowIndex = SelectedRowIndex;
                    var selectedRowId = (int)DisplayDataTable.Rows[SelectedRowIndex][ConstString.RowId];
                    var prevSelectedRowId = (int)DisplayDataTable.Rows[SelectedRowIndex - 1][ConstString.RowId];

                    var selectedRow = _dataRunResult.GetRowFromDataRun(selectedRowId);
                    var prevSelectedRow = _dataRunResult.GetRowFromDataRun(prevSelectedRowId);

                    DisplayDataTable.Rows[SelectedRowIndex - 1][ConstString.RowId] = -1;
                    DisplayDataTable.Rows[SelectedRowIndex][ConstString.RowId] = prevSelectedRowId;
                    DisplayDataTable.Rows[SelectedRowIndex - 1][ConstString.RowId] = selectedRowId;

                    prevSelectedRow[ConstString.RowId] = -1;
                    selectedRow[ConstString.RowId] = prevSelectedRowId;
                    prevSelectedRow[ConstString.RowId] = selectedRowId;


                    // Sort the DataTable by Id column
                    DisplayDataTable.DefaultView.Sort = $"{ConstString.RowId} ASC";
                    DisplayDataTable = DisplayDataTable.DefaultView.ToTable();
                    
                    OnPropertyChanged(nameof(DisplayDataTable));

                    SelectedRowIndex = selectedRowIndex - 1;
                    OnPropertyChanged(nameof(SelectedRowIndex));
                }
            }
        }

        public RelayCommand DownDataRowCommand => new(DownDataRowHandler);

        private void DownDataRowHandler(object parameter)
        {
            if (SelectedRow != null)
            {
                // Move the selected item down
                if (SelectedRowIndex < DisplayDataTable.Rows.Count - 1)
                {
                    var selectedRowIndex = SelectedRowIndex;
                    var selectedRowId = (int)DisplayDataTable.Rows[SelectedRowIndex][ConstString.RowId];
                    var nextSelectedRowId = (int)DisplayDataTable.Rows[SelectedRowIndex + 1][ConstString.RowId];

                    var selectedRow = _dataRunResult.GetRowFromDataRun(selectedRowId);
                    var nextSelectedRow = _dataRunResult.GetRowFromDataRun(nextSelectedRowId);

                    DisplayDataTable.Rows[SelectedRowIndex + 1][ConstString.RowId] = -1;
                    DisplayDataTable.Rows[SelectedRowIndex][ConstString.RowId] = nextSelectedRowId;
                    DisplayDataTable.Rows[SelectedRowIndex + 1][ConstString.RowId] = selectedRowId;

                    nextSelectedRow[ConstString.RowId] = -1;
                    selectedRow[ConstString.RowId] = nextSelectedRowId;
                    nextSelectedRow[ConstString.RowId] = selectedRowId;

                    // Sort the DataTable by Id column
                    DisplayDataTable.DefaultView.Sort = $"{ConstString.RowId} ASC";
                    DisplayDataTable = DisplayDataTable.DefaultView.ToTable();
                    OnPropertyChanged(nameof(DisplayDataTable));

                    SelectedRowIndex = selectedRowIndex + 1;
                    OnPropertyChanged(nameof(SelectedRowIndex));
                }
            }
        }

        public RelayCommand SaveDataToFileCommand => new(SaveDataToFileHandler);

        private void SaveDataToFileHandler(object parameter)
        {
            var dataFormat = Path.GetExtension(SourceProjectObject.Extract.OutputFile).Remove(0, 1);
            bool validType = Enum.TryParse(dataFormat, true, out DataType dataType);
            if (!validType)
            {
                MessageBox.Show("Invalid data format", "HiEndsApp", MessageBoxButton.OK);
                return;
            }
            var file = Path.Combine(SelectedHiProject.FolderPath, SourceProjectObject.Extract.OutputFile);
            if (File.Exists(file))
            {
                MessageBoxResult result = MessageBox.Show("Do you want to overwrite the existing file?", "HiEndsApp", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            var dataTable = DisplayDataTable.Copy();
            if (!SourceProjectObject.Extract.SaveDataWithResult)
            {
                dataTable.PrimaryKey = null;
                dataTable.Columns.Remove(ConstString.RowId);
                dataTable.Columns.Remove(ConstString.RunStatus);
                dataTable.Columns.Remove(ConstString.RunDateTime);
                dataTable.Columns.Remove(ConstString.RunTime);
                dataTable.Columns.Remove(ConstString.RunResultFile);
                dataTable.Columns.Remove(ConstString.RunComments);
            }
            
            var content = CsvUtilities.DataTableToCsv(dataTable);
            File.WriteAllText(file, content);
        }

        public RelayCommand AppendDataToFileCommand => new(AppendDataToFileHandler);

        private void AppendDataToFileHandler(object parameter)
        {
            var dataFormat = Path.GetExtension(SourceProjectObject.Extract.OutputFile).Remove(0, 1);
            bool validType = Enum.TryParse(dataFormat, true, out DataType dataType);
            if (!validType)
            {
                MessageBox.Show("Invalid data format", "HiEndsApp", MessageBoxButton.OK);
                return;
            }

            if (!File.Exists(Path.Combine(SelectedHiProject.FolderPath, SourceProjectObject.Extract.OutputFile)))
            {
                MessageBox.Show("File does not exist", "HiEndsApp", MessageBoxButton.OK);
                return;
            }
            var file = Path.Combine(SelectedHiProject.FolderPath, SourceProjectObject.Extract.OutputFile);
            var existingData = File.ReadAllText(file);
            var dataTable = DisplayDataTable.Copy();
            if (!SourceProjectObject.Extract.SaveDataWithResult)
            {
                dataTable.PrimaryKey = null;
                dataTable.Columns.Remove(ConstString.RowId);
                dataTable.Columns.Remove(ConstString.RunStatus);
                dataTable.Columns.Remove(ConstString.RunDateTime);
                dataTable.Columns.Remove(ConstString.RunTime);
                dataTable.Columns.Remove(ConstString.RunResultFile);
                dataTable.Columns.Remove(ConstString.RunComments);
            }
            var mergeData = MergeToExistingData(dataTable, existingData, dataType);
            var finalString = LoadDataAsString(mergeData, dataType);
            File.WriteAllText(SourceProjectObject.Extract.OutputFile, finalString);
        }

        #endregion
        #region Runner Editor Feature
        public RelayCommand BrowseRunnerCommand => new(BrowseRunnerHandler);

        private void BrowseRunnerHandler(object parameter)
        {
            if (SelectedRunCommand == null)
            {
                MessageBox.Show("Please select a command.");
            }

            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                //Query = "Batch Files (*.bat)|*.bat",
                //InitialDirectory = SelectedRunCommand.FolderPath
            };
            fileDialog.ShowDialog();

            if (!string.IsNullOrEmpty(fileDialog.FileName))
            {
                SelectedRunCommand.RunnerPath = Utilities.ConvertAbsolutePathToRelative(fileDialog.FileName, SelectedHiProject.FolderPath);
                OnPropertyChanged(nameof(SelectedRunCommand));
                SourceProjectObject.Run.RunCommands = SourceProjectObject.Run.RunCommands.ToArray();
                OnPropertyChanged(nameof(SourceProjectObject));
            }
        }

        public RelayCommand AddRunnerCommand => new(AddRunnerHandler);

        private void AddRunnerHandler(object parameter)
        {
            RunCommand runCommand = new RunCommand();
            SourceProjectObject.Run.RunCommands ??= new RunCommand[1];
            SourceProjectObject.Run.RunCommands = SourceProjectObject.Run.RunCommands.Append(runCommand).ToArray();
            OnPropertyChanged(nameof(SourceProjectObject));
            SelectedRunCommand = runCommand;
        }

        public RelayCommand DeleteRunnerCommand => new(DeleteRunnerHandler);

        private void DeleteRunnerHandler(object parameter)
        {
            if (SelectedRunCommand != null)
            {
                SourceProjectObject.Run.RunCommands = SourceProjectObject.Run.RunCommands.Where(val => val != SelectedRunCommand).ToArray();
                SelectedRunCommand = null;
                OnPropertyChanged(nameof(SourceProjectObject));
            }
        }

        public RelayCommand DownRunnerCommand => new(DownRunnerHandler);

        private void DownRunnerHandler(object parameter)
        {
            if (SelectedRunCommand != null)
            {
                // Move the selected item down
                SourceProjectObject.Run.RunCommands.MoveItemDown(SelectedRunIndex);
                SourceProjectObject.Run.RunCommands = SourceProjectObject.Run.RunCommands.ToArray();
                OnPropertyChanged(nameof(SourceProjectObject));
            }
        }

        public RelayCommand UpRunnerCommand => new(UpRunnerHandler);

        private void UpRunnerHandler(object parameter)
        {
            if (SelectedRunCommand != null)
            {
                // Move the selected item down
                SourceProjectObject.Run.RunCommands.MoveItemUp(SelectedRunIndex);
                SourceProjectObject.Run.RunCommands = SourceProjectObject.Run.RunCommands.ToArray();
                OnPropertyChanged(nameof(SourceProjectObject));
            }
        }
        #endregion

        #region Run Feature

        public RelayCommand RunCommand => new(RunHandlerAsync);
        private async void RunHandlerAsync(object parameter)
        {
            _updateLogResultTimer.Start();
            // Add the selected rows to the in-queue queue (avoid duplicates)
            foreach (var row in SelectedRows)
            {
                if (ReferenceEquals((string)row[ConstString.RunStatus], nameof(RunStatus.Running)))
                    continue;
                int rowId = (int)row[ConstString.RowId];

                lock (_inQueueLock)
                {
                    if (!_inQueueDataRows.Contains(rowId))
                    {
                        _inQueueDataRows.Enqueue(rowId);
                        _dataRunResult.InsertRunResult(rowId);
                        _dataRunResult.SaveDataResultToFile(rowId);
                    }
                }

                UpdateRunStatus(rowId, RunStatus.InQueue, false);
                UpdateRunResultFile(rowId, false);
                if (SelectedRow.Equals(row)) UpdateRunResultView();
            }
            OnPropertyChanged(nameof(DisplayDataTable));

            // If there are no in-queue rows and no tasks are currently running => return
            lock (_inQueueLock)
            {
                if (_runningTasksCount > 0) return;
                _runningTasksCount = 0;
            }
            // Start the task to run in-queue rows
            await RunInQueueRowsAsync();
            MessageBox.Show("All tasks completed!");
            _updateLogResultTimer.Stop();
        }

        private async Task RunInQueueRowsAsync()
        {
            List<Task> tasks = new List<Task>();

            // Create tasks to execute the bat file for each selected row
            while (_inQueueDataRows.Count > 0)
            {
                if (_runningTasksCount >= _threadsNumber)
                {
                    // If the maximum number of running tasks is reached, wait
                    await Task.Delay(2000); // Adjust the delay time as needed
                    continue;
                }

                while (_runningTasksCount < _threadsNumber)
                {
                    _runningTasksCount++;
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            int rowId;
                            lock (_inQueueLock)
                            {
                                // Dequeue the next row from the in-queue queue
                                _inQueueDataRows.TryDequeue(out int inQueueRowId);
                                if (inQueueRowId == 0) return;
                                if (!_dataRunResult.RunRowResults.ContainsKey(inQueueRowId)) return;
                                rowId = inQueueRowId;
                                UpdateRunStatus(rowId, RunStatus.Running);
                            }

                            // Run
                            await _dataRunResult.RunCommandsForRow(rowId);

                            var runResults = _dataRunResult.RunRowResults[rowId];
                            lock (_inQueueLock)
                            {
                                UpdateRunResult(rowId, runResults[0]);
                                _dataRunResult.SaveDataResultToFile(rowId);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Handle exceptions
                            MessageBox.Show($"Error: {ex.Message}");
                        }
                        finally
                        {
                            _runningTasksCount--;
                        }
                    }));
                }

                // Wait for all tasks to complete
                await Task.WhenAll(tasks);
                
            }
        }

        public RelayCommand ResetStatusCommand => new(ResetStatusHandler);

        private void ResetStatusHandler(object parameter)
        {
            MessageBoxResult res = MessageBox.Show("This will delete the result of these tests. Are you sure you want to do it?", "HiEndsApp",MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                foreach (var row in SelectedRows)
                {
                    var rowId = (int)row[ConstString.RowId];
                    UpdateRunStatus(rowId, RunStatus.Available, false);
                    
                    if (_dataRunResult.RunRowResults.ContainsKey(rowId))
                    {
                        var resultFile = (string)row[ConstString.RunResultFile];
                        var filePath = Path.Combine(_selectedHiProject.FolderPath,
                            _selectedHiProject.SourceProject.Run.OutputFolder, resultFile);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        _dataRunResult.RunRowResults.Remove(rowId);
                    }
                }
                OnPropertyChanged(nameof(DisplayDataTable));
                UpdateResultCount();
                UpdateInQueueCount();
            }
        }
        #endregion

        #region Run Result Feature

        private void UpdateRunResult(int rowId, RunResult runResult, bool updateUI = true)
        {
            UpdateRunStatus(rowId, runResult.RunStatus, false);
            UpdateRunDateTime(rowId, runResult.RunDateTime, runResult.RunTime, false);

            if(updateUI)
                OnPropertyChanged(nameof(DisplayDataTable));
        }

        private void UpdateRunResultFile(int rowId, bool updateUI = true)
        {
            var resultFileName = _dataRunResult.RefineOutputFile(rowId);
            var displayingRow = GetRowFromDisplayDataTable(rowId);
            if(displayingRow != null)
                displayingRow[ConstString.RunResultFile] = resultFileName;
            
            if (updateUI)
                OnPropertyChanged(nameof(DisplayDataTable));
        }

        private void UpdateRunDateTime(int rowId, string runDateTime, string runTime, bool updateUI = true)
        {
            _dataRunResult.UpdateRunDateTime(rowId, runDateTime, runTime);

            var row = GetRowFromDisplayDataTable(rowId);
            if (row != null)
            {
                row[ConstString.RunDateTime] = runDateTime;
                row[ConstString.RunTime] = runTime;
            }

            if (updateUI)
            {
                OnPropertyChanged(nameof(DisplayDataTable));
            }
        }

        private void UpdateRunStatus(int rowId, RunStatus newStatus, bool updateUI = true)
        {
            _dataRunResult.UpdateDataRunStatusById(rowId, newStatus);

            var row = GetRowFromDisplayDataTable(rowId);
            if (row != null)
                row[ConstString.RunStatus] = newStatus;

            if (_dataRunResult.RunRowResults.ContainsKey(rowId))
            {
                if(_dataRunResult.RunRowResults[rowId][0].RunStatus != newStatus)
                    _dataRunResult.RunRowResults[rowId][0].RunStatus = newStatus;
            }

            if (updateUI)
            {
                OnPropertyChanged(nameof(DisplayDataTable));
            }

            UpdateInQueueCount();
            UpdateRunningCount();
            UpdateResultCount();
            UpdateRunResultView();
        }

        private void UpdateShowAllCount()
        {
            ShowAllCount = _dataRunResult.DataRun.Rows.Count.ToString();
        }

        private void UpdateResultCount()
        {
            var passedCount = _dataRunResult.DataRun.Select($"{ConstString.RunStatus}='{RunStatus.Passed}'").Length;
            var failedCount = _dataRunResult.DataRun.Select($"{ConstString.RunStatus}='{RunStatus.Failed}'").Length;
            PassedCount = passedCount.ToString();
            FailedCount = failedCount.ToString();
            RunTotalCount = (passedCount + failedCount).ToString();
        }

        private void UpdateInQueueCount()
        {
            var inQueueCount = _dataRunResult.DataRun.Select($"{ConstString.RunStatus}='{RunStatus.InQueue}'").Length;
            InQueueCount = inQueueCount.ToString();
        }

        private void UpdateRunningCount()
        {
            var runningCount = _dataRunResult.DataRun.Select($"{ConstString.RunStatus}='{RunStatus.Running}'").Length;
            RunningCount = runningCount.ToString();
        }

        private void LoadResultFolders(string resultFolder = "RunResult")
        {
            if (resultFolder == null) return;
            var resultFolderPath = Path.Combine(_selectedHiProject.FolderPath, resultFolder);
            Utilities.CreateFoldersIfNotExists(resultFolderPath);
            
            var dirs = Directory.GetDirectories(resultFolderPath);
            ResultFolders = new string[dirs.Length];
            for (int i = 0; i < dirs.Length; i++)
            {
                ResultFolders[i] = Path.GetFileName(dirs[i]);
            }
        }

        private void UpdateRunResultView()
        {
            if (SelectedRow != null && SelectedRow.RowState != DataRowState.Detached)
            {
                SelectedOutputFile = (string)SelectedRow[ConstString.RunResultFile];

                var rowId = (int)SelectedRow[ConstString.RowId];
                RunResults = (_dataRunResult.RunRowResults.ContainsKey(rowId)
                    ? _dataRunResult.RunRowResults[rowId]
                    : new RunResult[1])!;

                //Comments = (string)SelectedRow[ConstString.RunComments];
            }
        }

        #endregion

        #region Common Methods

        public DataRow GetRowFromDisplayDataTable(int rowId)
        {
            return DisplayDataTable.Rows.Find(rowId);
        }

        private void ExtractDataByTemplate()
        {
            try
            {
                if (_selectedTemplateFile.ExTemplate.BrowseType.Contains("File:"))
                {
                    if (!_selectedTemplateFile.ExTemplate.BrowseType.Contains(Path.GetExtension(_sourceProject.Extract.SourcePath)))
                    {
                        MessageBox.Show($"Error: The source file is not the extraction definition type of '{_selectedTemplateFile.ExTemplate.BrowseType}'.", "HiEndsApp", MessageBoxButton.OK);
                        return;
                    }
                }
                IDataExtractor dataExtractor =
                    DataExtractorFactory.CreateDataExtractor(_selectedTemplateFile.ExTemplate);

                object extractedData = dataExtractor.ExtractData<object>(_sourceProject.Extract.SourcePath,
                    _selectedTemplateFile.ExTemplate);

                object transformedData =
                    dataExtractor.TransformData(extractedData, _selectedTemplateFile.ExTemplate, "");

                _dataRunResult = new DataRunResult((DataTable)transformedData, SourceProjectObject.Run,
                    SourceProjectObject.Vars, SelectedHiProject.FolderPath);

                _inQueueDataRows = new ConcurrentQueue<int>(_dataRunResult.GetInQueueRowIds());
                UpdateShowAllCount();
                UpdateInQueueCount();
                UpdateRunningCount();
                UpdateResultCount();

                SearchProperties = _dataRunResult.DataRun.Columns.Cast<DataColumn>()
                    .Select(x => x.ColumnName)
                    .ToList();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error: Can not extract data from the source file: {ex.Message}.", "HiEndsApp", MessageBoxButton.OK);
            }
        }

        private void UpdateLogForSelectedRow_Tick(object? sender, EventArgs e)
        {
            if (SelectedRunResult != null && SelectedRunResult.RunStatus == RunStatus.Running)
            {
                var runCommand = SelectedRunCommandResult;
                RunCommandResults = _selectedRunResult.RunCommandResults.ToArray();
                OnPropertyChanged(nameof(RunCommandResults));

                SelectedRunCommandResult = runCommand;
                OnPropertyChanged(nameof(SelectedRunCommandResult));
            }
        }

        private DataTable MergeToExistingData(DataTable dataTable, string existingData, DataType type)
        {
            // Call the ExtractData method to get the data
            DataTable currentData = new DataTable();
            switch (type)
            {
                case DataType.json:
                    currentData = JsonUtilities.JsonToDataTable(existingData);
                    currentData.Merge(dataTable, false, MissingSchemaAction.Add);
                    break;
                case DataType.csv:
                    currentData = CsvUtilities.CsvToDataTable(existingData);
                    currentData.Merge(dataTable, false, MissingSchemaAction.Add);
                    break;
            }
            return currentData;
        }

        private string LoadDataAsString(DataTable dataTable, DataType type)
        {
            // Call the ExtractData method to get the data
            string result = string.Empty;
            switch (type)
            {
                case DataType.json:
                    result = JsonUtilities.DataTableToJson(dataTable);
                    break;
                case DataType.csv:
                    result = CsvUtilities.DataTableToCsv(dataTable);
                    break;
            }
            return result;
        }

        #endregion
    }
}
