using System;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace HiEndsCore.Models
{
    // SourceProjects.cs
    public class SourceProjects
    {
        public List<SourceProject> SourceProjectsList { get; set; }
    }

    // SourceProject.cs
    public class SourceProject
    {
        public SourceProject()
        {
            Name = "";
            Extract = new ExtractArgument();
            Run = new RunArgument();
            Vars = new Dictionary<string, string>();
        }
        public string Name { get; set; }

        public ExtractArgument Extract { get; set; }

        public RunArgument Run { get; set; }

        public Dictionary<string, string> Vars { get; set; }

    }

    public class ExtractArgument
    {
        public ExtractArgument()
        {
            SourcePath = "";
            TemplateFile = "";
            OutputFile = "";
            SaveDataWithResult = true;
            AppendToOutputFile = null;
            Query = "";
        }
        public string SourcePath { get; set; }

        public string TemplateFile { get; set; }

        public string OutputFile { get; set; }

        public bool SaveDataWithResult { get; set; }

        public bool? AppendToOutputFile { get; set; }

        public string Query { get; set; }

        public List<string> Queries { get; set; }

        public void AddQuery(string query)
        {
            if (Queries == null)
            {
                Queries = new List<string>();
            }
            Queries.Add(query);
        }

    }

    public class RunArgument
    {
        public RunArgument()
        {
            InputFile = "";
            DataFormat = "";
            RunCommands = Array.Empty<RunCommand>();
            OutputFile = "";
            OutputFolder = "";
            ExitCodeMap = new Dictionary<string, string>();
            RunThreadNumber = 1;
            SaveDataWithResult = true;
            ShowResult = null;
        }
        public string InputFile { get; set; }

        public string DataFormat { get; set; }

        public string OutputFile { get; set; }

        public string OutputFolder { get; set; }


        public RunCommand[]? RunCommands { get; set; }

        public Dictionary<string, string> ExitCodeMap { get; set; }

        public int? RunThreadNumber { get; set; }

        public string Query { get; set; }

        public bool SaveDataWithResult { get; set; }

        public bool? ShowResult { get; set; }
    }

    public class RunCommand
    {
        public RunCommand()
        {
            RunnerPath = string.Empty;
            Arguments = string.Empty;
            Active = true;
            UseWindowConsole = true;
            RunAtRowIds = string.Empty;
            ReturnExitCode = "0";
            OutputContain = string.Empty;
            OutputNotContain = string.Empty;
            InputMapping= new List<Tuple<string, string>>();
        }

        public RunCommand(string runnerPath, string arguments, string runAtRowIds = "", bool useWindowConsole = true, string returnExitCode = "0", string outputContain = "", string outputNotContain = "", List<Tuple<string, string>> inputMappings = null, bool active = true)
        {
            RunnerPath = runnerPath;
            Arguments = arguments;
            RunAtRowIds = runAtRowIds;
            UseWindowConsole = useWindowConsole;
            Active = active;
            ReturnExitCode = returnExitCode;
            OutputContain = outputContain;
            OutputNotContain = outputNotContain;
            InputMapping = inputMappings != null?inputMappings: new List<Tuple<string, string>>();
        }

        public RunCommand(string runCommand)
        {
            RunnerPath = runCommand.Split(' ')[0];
            Arguments = runCommand.Substring(RunnerPath.Length + 1);
            RunAtRowIds = String.Empty;
            UseWindowConsole = true;
            Active = true;
            ReturnExitCode = string.Empty;
            OutputContain = string.Empty;
            OutputNotContain = string.Empty;
            InputMapping = new List<Tuple<string, string>>();
        }

        public string RunnerPath { get; set; }

        public string Arguments { get; set; }

        public bool Active { get; set; }

        public string ReturnExitCode { get; set; }

        public string OutputContain { get; set; }

        public string OutputNotContain { get; set; }

        public string RunAtRowIds{ get; set; }

        public bool UseWindowConsole { get; set; }

        public List<Tuple<string,string>> InputMapping { get; set; }


    }
}