using CommandLine;

namespace libz_unpacker
{
    public class Options
    {
        [Value(0, MetaName = "inpath", Required = true, HelpText = "Input path (file or directory).")]
        public string InputPath { get; set; } = ".";

        [Value(1, MetaName = "outpath", Required = true, HelpText = "Output folder.")]
        public string OutputPath { get; set; } = ".";

        [Option('r', "recursive", Required = false, HelpText = "Search loaded assemblies for more embedded resource assemblies")]
        public bool Recursive { get; set; }
    }
}
