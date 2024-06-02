using HiEndsCore.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HiEndsCore.Models
{
    public enum RunStatus
    {
        Passed,
        Failed,
        Available,
        InQueue,
        Running
    }

    public class DataRunResult
    {
        public DataTable DataRun { get; set; }

        public RunArgument RunArgument { get; set; }

        public Dictionary<string, string> Vars { get; set; }

        public string HiProjectPath { get; set; }

        public Dictionary<int, RunResult[]> RunRowResults { get; set; }

        public DataRunResult(DataTable data, RunArgument runArguments, Dictionary<string, string> vars, string hiProjectPath)
        {
            DataRun = data;
            RunArgument = runArguments;
            Vars = vars;
            RunRowResults = new Dictionary<int, RunResult[]>();
            HiProjectPath = hiProjectPath;
            LoadDataRunResult(DataRun, HiProjectPath);
        }

        private void LoadDataRunResult(DataTable dataRun, string hiProjectPath)
        {
            InitRunResultColumns(dataRun);

            int id = 1;
            // Populate the RowNumber column with row indices
            foreach (DataRow row in dataRun.Rows)
            {
                row[ConstString.RowId] = id++;

                var logFileName = RefineOutputFile(row);
                var logFile = Path.Combine(hiProjectPath, RunArgument.OutputFolder, logFileName);
                if (File.Exists(logFile))
                {
                    // Read the JSON content from the file
                    string jsonContent = File.ReadAllText(logFile);

                    // Parse the JSON content
                    JObject jsonObject = JObject.Parse(jsonContent);

                    if (jsonObject.ContainsKey(ConstString.RunResults))
                    {
                        JArray runResultsJson = (JArray)jsonObject[ConstString.RunResults]!;
                        RunResult[] runResults = new RunResult[runResultsJson.Count];
                        for (int i = 0; i < runResultsJson.Count; i++)
                        {
                            runResults[i] = runResultsJson[i].ToObject<RunResult>();
                        }

                        RunRowResults.Add((int)row[ConstString.RowId], runResults);

                        // Populate DataRow with values from the JSON
                        row[ConstString.RunStatus] = runResults[0].RunStatus;
                        row[ConstString.RunDateTime] = runResults[0].RunDateTime;
                        row[ConstString.RunTime] = runResults[0].RunTime;
                        row[ConstString.RunResultFile] = logFileName;
                        row[ConstString.RunComments] = runResults[0].RunComments;
                    }
                }
                else
                {
                    InitRowData(row);
                }
            }

            dataRun.Columns[ConstString.RowId].SetOrdinal(dataRun.Columns.Count - 1);
            dataRun.PrimaryKey = new DataColumn[] { dataRun.Columns[ConstString.RowId]! };
        }

        public void InitRowData(DataRow row)
        {
            row[ConstString.RunStatus] = nameof(RunStatus.Available);
            row[ConstString.RunDateTime] = string.Empty;
            row[ConstString.RunTime] = string.Empty;
            row[ConstString.RunResultFile] = string.Empty;
            row[ConstString.RunComments] = string.Empty;
        }


        private void InitRunResultColumns(DataTable dt)
        {
            if (!dt.Columns.Contains(ConstString.RowId))
                dt.Columns.Add(ConstString.RowId, typeof(int));

            if (!dt.Columns.Contains(ConstString.RunStatus))
                dt.Columns.Add(ConstString.RunStatus, typeof(string));

            if (!dt.Columns.Contains(ConstString.RunDateTime))
                dt.Columns.Add(ConstString.RunDateTime, typeof(string));

            if (!dt.Columns.Contains(ConstString.RunTime))
                dt.Columns.Add(ConstString.RunTime, typeof(string));

            if (!dt.Columns.Contains(ConstString.RunResultFile))
                dt.Columns.Add(ConstString.RunResultFile, typeof(string));

            if (!dt.Columns.Contains(ConstString.RunComments))
                dt.Columns.Add(ConstString.RunComments, typeof(string));
        }

        public async Task RunCommandsForRow(int rowId)
        {
            var runStatus = RunStatus.Passed;

            var runResult = RunRowResults[rowId][0];
            var runCommandResults = runResult.RunCommandResults;

            DateTime startTime = DateTime.Now;
            runResult.RunDateTime = startTime.ToString("G");

            foreach (var runCmdResult in runCommandResults)
            {
                if (!runCmdResult.RunCommand.Active)
                {
                    runCmdResult.RunResult = "Inactive";
                    runCmdResult.Note = "Skipped by Active is false.";
                    continue;
                }

                if (!IsInRowIdsForRun(runCmdResult.RunCommand.RunAtRowIds, rowId))
                {
                    runCmdResult.RunResult = "NotInRange";
                    runCmdResult.Note = "Skipped by not in range of the specified row Ids.";
                    continue;
                }

                runCmdResult.RunDateTime = DateTime.Now.ToString("G");

                runCmdResult.RunResult = await RunCommandAsync(runCmdResult, HiProjectPath);

                runCmdResult.RunTime = (DateTime.Now - Convert.ToDateTime(runCmdResult.RunDateTime)).ToString();

                if (runCmdResult.RunResult != RunStatus.Passed.ToString())
                {
                    runStatus = RunStatus.Failed;
                    break;
                }
            }

            runResult.RunStatus = runStatus;
            runResult.RunTime = (DateTime.Now - startTime).ToString("g");
        }

        private async Task<string> RunCommandAsync(RunCommandResult commandResult, string hiProjectPath)
        {
            var command = commandResult.RunCommand;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Command executed: {string.Join(" ", command.RunnerPath, command.Arguments)}");
            Console.ForegroundColor = ConsoleColor.White;
            commandResult.SystemLog = $"Command executed: {string.Join(" ", command.RunnerPath, command.Arguments)}" + Environment.NewLine;
            string output = string.Empty;
            var outputFileName = $"output{DateTime.Now.ToFileTime()}.log";
            var teeArgs = $"{Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"tee\tee")} {outputFileName}";
            var fileName = !commandResult.RunCommand.UseWindowConsole ? Path.Combine(hiProjectPath, command.RunnerPath) : "cmd.exe";
            var argument = !commandResult.RunCommand.UseWindowConsole ? command.Arguments : $"/c \"{Path.Combine(hiProjectPath, command.RunnerPath)} {command.Arguments + " | " + teeArgs}\"";
            
            int indexMapping = 0;
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = fileName,
                        Arguments = argument,
                        RedirectStandardOutput = !commandResult.RunCommand.UseWindowConsole,
                        RedirectStandardInput = !commandResult.RunCommand.UseWindowConsole,
                        RedirectStandardError = !commandResult.RunCommand.UseWindowConsole,
                        UseShellExecute = false,
                        CreateNoWindow = !commandResult.RunCommand.UseWindowConsole,
                        Verb = "runas"
                    }
                };


                if (!commandResult.RunCommand.UseWindowConsole)
                {
                    process.OutputDataReceived += (_, e) =>
                    {
                        Console.WriteLine("out>> " + e.Data);
                        output = output + e.Data + Environment.NewLine;
                        commandResult.OutputLog = output;

                        if (e.Data != null && commandResult.RunCommand.InputMapping.Count > 0 && e.Data.Contains(commandResult.RunCommand.InputMapping[indexMapping].Item1))
                        {
                            // Write the corresponding value to the standard input stream
                            process.StandardInput.WriteLine(commandResult.RunCommand.InputMapping[indexMapping].Item2);
                            output = output + commandResult.RunCommand.InputMapping[indexMapping].Item2 + Environment.NewLine;
                            commandResult.OutputLog = output;
                            indexMapping++;
                        }
                    };

                    process.ErrorDataReceived += (_, e) =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("err>> " + e.Data);
                        output = output + e.Data + Environment.NewLine;
                        commandResult.OutputLog = output;
                    };
                }

                process.Start();
                
                //process.StandardInput.WriteLine("1");
                if (!commandResult.RunCommand.UseWindowConsole)
                {
                    // Start the asynchronous read of the sort output stream.
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                await process.WaitForExitAsync();

                output += Environment.NewLine;

                if (commandResult.RunCommand.UseWindowConsole)
                {
                    if (File.Exists(outputFileName))
                    {
                        output += await File.ReadAllTextAsync(outputFileName);
                        File.Delete(outputFileName);
                    }
                }

                commandResult.OutputLog = output;

                string outputResult = OutputResultLog(commandResult, process);

                return outputResult;
            }
            catch (Exception ex)
            {
                commandResult.SystemLog = "Command return: 1." + Environment.NewLine 
                                                         + $"Error executing command: {string.Join(" ", command.RunnerPath, command.Arguments)}"
                                                         + Environment.NewLine 
                                                         + ex.Message;
                return RunStatus.Failed.ToString();
            }
        }

        private static string OutputResultLog(RunCommandResult commandResult, Process process)
        {
            string outputResult = RunStatus.Passed.ToString();
            commandResult.SystemLog += $"Command return the exit code: {process.ExitCode}";
            if (process.ExitCode.ToString() != commandResult.RunCommand.ReturnExitCode)
            {
                commandResult.SystemLog += $"==> {RunStatus.Failed}";
                outputResult = RunStatus.Failed.ToString();
            }
            else
                commandResult.SystemLog += $"==> {RunStatus.Passed}";

            commandResult.SystemLog += Environment.NewLine + $"Output contains: '{commandResult.RunCommand.OutputContain}'";
            if (commandResult.RunCommand.OutputContain != string.Empty)
            {
                if (!Utilities.ContainsWildcard(commandResult.OutputLog, commandResult.RunCommand.OutputContain))
                {
                    commandResult.SystemLog += $" ==> {RunStatus.Failed}";
                    outputResult = RunStatus.Failed.ToString();
                }
                else
                    commandResult.SystemLog += $" {commandResult.RunCommand.OutputContain} ==> {RunStatus.Passed}";
            }

            commandResult.SystemLog += Environment.NewLine + $"Output does not contain: '{commandResult.RunCommand.OutputNotContain}'";
            if (commandResult.RunCommand.OutputNotContain != string.Empty)
            {
                if (commandResult.OutputLog.Contains(commandResult.RunCommand.OutputNotContain))
                {
                    commandResult.SystemLog += $" ==> {RunStatus.Failed}";
                    outputResult = RunStatus.Failed.ToString();
                }
                else
                    commandResult.SystemLog += $" ==> {RunStatus.Passed}";
            }

            return outputResult;
        }

        public void UpdateRunDateTime(int rowId, string runDateTime, string runTime)
        {
            var row = GetRowFromDataRun(rowId);
            row[ConstString.RunDateTime] = runDateTime;
            row[ConstString.RunTime] = runTime;
        }

        public void UpdateDataRunStatusById(int rowId, RunStatus newStatus)
        {
            var row = GetRowFromDataRun(rowId);
            row[ConstString.RunStatus] = newStatus;
        }

        public bool SaveDataResultToFile(int rowId, string resultFileName = "")
        {
            bool updateSuccessful = false;
            var row = GetRowFromDataRun(rowId);
            var runResults = RunRowResults[rowId];

            if(string.IsNullOrEmpty(resultFileName))
                resultFileName = (string)row[ConstString.RunResultFile];
            try
            {
                var resultContent = ConvertDataRunResultToJson(row, runResults);
                string resultFile = Path.Combine(HiProjectPath, RunArgument.OutputFolder, resultFileName);

                Utilities.CreateFoldersIfNotExists(Path.Combine(HiProjectPath, RunArgument.OutputFolder));
                File.WriteAllTextAsync(resultFile, resultContent);
                if(File.Exists(resultFile)) updateSuccessful = true;

                row[ConstString.RunResultFile] = resultFileName;
            }
            catch
            {
                updateSuccessful = false;
                row[ConstString.RunResultFile] = string.Empty;
            }

            return updateSuccessful;
        }

        private string ConvertDataRunResultToJson(DataRow row, RunResult[] runResults)
        {
            var rowData = new Dictionary<string, object>();
            
            foreach (DataColumn column in row.Table.Columns)
            {
                if (column.ColumnName == ConstString.RowId
                    || column.ColumnName == ConstString.RunStatus
                    || column.ColumnName == ConstString.RunDateTime
                    || column.ColumnName == ConstString.RunComments)
                    continue;

                rowData[column.ColumnName] = row[column];
            }
            string rowJson = JsonConvert.SerializeObject(rowData, Formatting.Indented);
            string runResultJson = JsonUtilities.CreateJsonString(ConstString.RunResults, runResults);

            return JsonUtilities.MergeJsons(rowJson, runResultJson);
        }

        public RunResult InsertRunResult(int rowId)
        {
            var row = GetRowFromDataRun(rowId);
            return InsertRunResult(row);
        }

        public RunResult InsertRunResult(DataRow row)
        {
            var rowId = (int)row[ConstString.RowId];
            var runCommandResults = RefineRunArgumentData(row);
            
            var runResult = new RunResult();
            Enum.TryParse<RunStatus>((string)row[ConstString.RunStatus], out var runStatus);
            runResult.RunStatus = runStatus;
            runResult.RunCommandResults = runCommandResults;
            runResult.RunComments = (string)row[ConstString.RunComments];

            var runResults = new[] { runResult };
            
            // 
            if (RunRowResults.ContainsKey(rowId))
            {
                runResults = RunRowResults[rowId];
                Array.Resize(ref runResults, runResults.Length + 1); // increase array length by 1
                Array.Copy(runResults, 0, runResults, 1, runResults.Length - 1); // shift elements right from the index
                runResults[0] = runResult; // insert element at the index
                RunRowResults[(int)row[ConstString.RowId]] = runResults;
            }
            else
            {
                RunRowResults.Add(rowId, runResults);
            }

            return runResult;
        }

        private RunCommandResult[] RefineRunArgumentData(DataRow row)
        {
            RunCommand[] runCommands = RunArgument.RunCommands;
            RunCommandResult[] runCommandResults = new RunCommandResult[runCommands.Length];
            RunCommand[] updatedCommands = runCommands.Select(x => new RunCommand(x.RunnerPath, x.Arguments,x.RunAtRowIds, x.UseWindowConsole,x.ReturnExitCode, x.OutputContain, x.OutputNotContain,x.InputMapping,x.Active )).ToArray();


            foreach (var pair in Vars)
            {
                for (int i = 0; i < runCommands.Length; i++)
                {
                    if (pair.Key != null && pair.Value != null)
                    {
                        updatedCommands[i].RunnerPath =
                            Regex.Replace(updatedCommands[i].RunnerPath, "{{" + pair.Key + "}}", pair.Value);
                        updatedCommands[i].Arguments =
                            Regex.Replace(updatedCommands[i].Arguments, "{{" + pair.Key + "}}", pair.Value);
                        updatedCommands[i].OutputContain =
                            Regex.Replace(updatedCommands[i].OutputContain, "{{" + pair.Key + "}}", pair.Value);
                        updatedCommands[i].OutputNotContain =
                            Regex.Replace(updatedCommands[i].OutputNotContain, "{{" + pair.Key + "}}", pair.Value);
                    }
                }
            }

            foreach (DataColumn column in DataRun.Columns)
            {
                string columnName = column.ColumnName;
                string? columnValue = row[columnName].ToString();

                for (int i = 0; i < runCommands.Length; i++)
                {
                    // Use regular expression to find and replace all occurrences of the column name as a placeholder
                    updatedCommands[i].RunnerPath =
                        Regex.Replace(updatedCommands[i].RunnerPath, "%" + columnName + "%", columnValue);
                    updatedCommands[i].Arguments =
                        Regex.Replace(updatedCommands[i].Arguments, "%" + columnName + "%", columnValue);

                    updatedCommands[i].OutputContain =
                        Regex.Replace(updatedCommands[i].OutputContain, "%" + columnName + "%", columnValue);

                    updatedCommands[i].OutputNotContain =
                        Regex.Replace(updatedCommands[i].OutputNotContain, "%" + columnName + "%", columnValue);

                    runCommandResults[i] = new RunCommandResult(updatedCommands[i]);
                }
            }

            return runCommandResults;
        }

        public string RefineOutputFile(int rowId)
        {
            var row = GetRowFromDataRun(rowId);
            row[ConstString.RunResultFile] = RefineOutputFile(row);
            return (string) row[ConstString.RunResultFile];
        }

        public string RefineOutputFile(DataRow row)
        {
            var logFileName = new string(RunArgument.OutputFile);

            foreach (DataColumn column in DataRun.Columns)
            {
                string columnName = column.ColumnName;
                string? columnValue = row[columnName].ToString();

                logFileName = Regex.Replace(logFileName, "%" + columnName + "%", columnValue);
            }

            return string.IsNullOrEmpty($"{Path.GetExtension(logFileName)}") ? $"{logFileName}.json" : logFileName;
        }
        
        public DataRow GetRowFromDataRun(int rowId)
        {
            return DataRun.Rows.Find(rowId);
        }

        public List<int> GetInQueueRowIds()
        {
            var inQueueRowIds = RunRowResults.Where(x => x.Value[0].RunStatus == RunStatus.InQueue)
                .Select(x => x.Key).ToList();
            return inQueueRowIds;
        }

        private bool IsInRowIdsForRun(string runAtRowIds, int rowId)
        {
            if (string.IsNullOrEmpty(runAtRowIds)) return true;

            var rowIdRanges = runAtRowIds.Split(',').Select(range => range.Trim()).ToList();
            foreach (var range in rowIdRanges)
            {
                if (range.Contains("-"))
                {
                    // Handle range pattern (e.g., "1-100")
                    var rangeParts = range.Split('-');
                    if (rangeParts.Length != 2) continue; // Invalid range format

                    if (int.TryParse(rangeParts[0], out int start) && int.TryParse(rangeParts[1], out int end))
                    {
                        if (rowId >= start && rowId <= end)
                            return true;
                    }
                }
                else
                {
                    // Handle single rowId (e.g., "42")
                    if (int.TryParse(range, out int singleRowId) && singleRowId == rowId)
                        return true;
                }
            }

            return false; // RowId not found in any specified range
        }

        public int GetNextRowId()
        {
            DataRow lastRow = DataRun.Rows[DataRun.Rows.Count - 1];
            return (int)lastRow[ConstString.RowId] + 1;
        }
    }


}
