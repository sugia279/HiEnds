using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiEndsCore.Models
{

    public class RunResult
    {
        public RunResult()
        {
            InitDefaultValues();
        }

        private void InitDefaultValues()
        {
            RunStatus = RunStatus.Available;
            RunDateTime = string.Empty;
            RunTime = string.Empty;
            RunComments = string.Empty;
        }

        [JsonProperty(ConstString.RunStatus)]
        public RunStatus RunStatus { get; set; }

        [JsonProperty(ConstString.RunDateTime)]
        public string RunDateTime { get; set; }

        [JsonProperty(ConstString.RunTime)]
        public string RunTime { get; set; }

        [JsonProperty(ConstString.RunComments)]
        public string RunComments { get; set; }

        [JsonProperty(ConstString.RunCommandResults)]
        public RunCommandResult[] RunCommandResults { get; set; }
    }


    public class RunCommandResult
    {
        public RunCommandResult(RunCommand runCommand)
        {
            RunCommand = runCommand;
        }

        public RunCommand RunCommand { get; set; }
        
        public string RunResult { get; set; }

        public string RunDateTime { get; set; }

        public string RunTime { get; set; }

        public string SystemLog { get; set; }

        public string OutputLog { get; set; }

        public string Note { get; set; }
    }
}
