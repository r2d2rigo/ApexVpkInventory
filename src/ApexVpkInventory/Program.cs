using CommandLine;
using Newtonsoft.Json;
using RespawnVpk;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace ApexVpkInventory
{
    class Program
    {
        private static readonly string TITANFALL2_EXE_FILE = "Titanfall2.exe";
        private static readonly string APEX_EXE_FILE = "r5apex.exe";
        private static readonly string GAME_VERSION_FILE = "gameversion.txt";
        private static readonly string VPK_FOLDER = "vpk";

        static void Main(string[] args)
        {
            Parser
                .Default
                .ParseArguments<CommandLineOptions>(args)
                .WithParsed<CommandLineOptions>(opt =>
                {
                    var inventory = new VpkInventory();
                    var tf2Exe = new FileInfo(Path.Combine(opt.GameFolder, TITANFALL2_EXE_FILE));
                    var apexExe = new FileInfo(Path.Combine(opt.GameFolder, APEX_EXE_FILE));
                    var gameVersionTxt = new FileInfo(Path.Combine(opt.GameFolder, GAME_VERSION_FILE));
                    var vpkDir = new DirectoryInfo(Path.Combine(opt.GameFolder, VPK_FOLDER));

                    if (!tf2Exe.Exists && !apexExe.Exists)
                    {
                        Console.WriteLine("ERROR: specified directory does not contain Titanfall 2/Apex Legends game data.");

                        return;
                    }

                    if (!gameVersionTxt.Exists)
                    {
                        Console.WriteLine($"ERROR: {gameVersionTxt} not found.");

                        return;
                    }

                    if (!vpkDir.Exists)
                    {
                        Console.WriteLine($"ERROR: VPK folder not found.");

                        return;
                    }

                    inventory.GameInfo.Name = tf2Exe.Exists ? "Titanfall 2" : "Apex Legends";

                    using (var versionReader = gameVersionTxt.OpenText())
                    {
                        inventory.GameInfo.Version = versionReader.ReadLine().Trim();
                    }

                    var vpkFiles = vpkDir.GetFiles("*_dir.vpk");

                    using (var sha256 = SHA256.Create())
                    {
                        foreach (var vpk in vpkFiles)
                        {
                            Console.WriteLine($"Processing '{vpk.Name}'...");

                            using (var package = new Package())
                            {
                                var vpkFileData = new VpkFile()
                                {
                                    Filename = vpk.Name,
                                };

                                package.Read(vpk.FullName);

                                var allEntries = package
                                    .Entries
                                    .SelectMany(e => e.Value)
                                    .ToList();

                                foreach (var entry in allEntries)
                                {
                                    package.ReadEntry(entry, out var entryData);

                                    var entryInfo = new VpkEntryInfo()
                                    {
                                        Filename = entry.GetFullPath(),
                                        Size = entryData.Length,
                                        Sha256 = BitConverter.ToString(sha256.ComputeHash(entryData)).Replace("-", string.Empty)
                                    };

                                    vpkFileData.Entries.Add(entryInfo);
                                }

                                inventory.Packages.Add(vpkFileData);
                            }
                        }
                    }

                    Console.WriteLine($"Writing '{inventory.GameInfo.Name} {inventory.GameInfo.Version}.json'...");

                    using (var outputFile = File.Create(Path.Combine(opt.OutputDirectory, $"{inventory.GameInfo.Name} {inventory.GameInfo.Version}.json")))
                    {
                        using (var streamWriter = new StreamWriter(outputFile))
                        {
                            using (var jsonWriter = new JsonTextWriter(streamWriter))
                            {
                                var serializer = new JsonSerializer();
                                serializer.Serialize(jsonWriter, inventory);
                            }
                        }
                    }
                });
        }
    }
}
