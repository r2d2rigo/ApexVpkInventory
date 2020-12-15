using CommandLine;

namespace ApexVpkInventory
{
    public class CommandLineOptions
    {
        [Option("path", Required = true, HelpText = "Path to Titanfall 2/Apex Legends game folder")]
        public string GameFolder { get; set; }

        [Option("outdir", Required = false, HelpText = "Output directory", Default = ".")]
        public string OutputDirectory { get; set; }
    }
}
