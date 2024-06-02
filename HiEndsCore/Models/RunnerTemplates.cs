namespace HiEndsCore.Models
{
    public class RunnerTemplates
    {
        public List<RunnerTemplate> RunerTemplatesList { get; set; }
    }

    public class RunnerTemplate
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<string> Arguments { get; set; }
    }
}
