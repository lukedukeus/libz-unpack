using System.IO.Compression;
using System.Reflection;

using CommandLine;
using CommandLine.Text;

namespace libz_unpacker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
               .WithParsed(Run)
               .WithNotParsed(HandleParsingErrors);
        }

        static void Run(Options options)
        {
            if (Directory.Exists(options.InputPath))
            {
                DirectoryInfo d = new DirectoryInfo(options.InputPath);

                foreach (var file in d.GetFiles())
                {
                    if (TryLoadAssembly(file.FullName, out Assembly? assembly))
                    {
                        SaveEmbeddedResources(assembly, options.OutputPath, options.Recursive);
                    }
                }
            }
            else if (TryLoadAssembly(options.InputPath, out Assembly? assembly))
            {
                SaveEmbeddedResources(assembly, options.OutputPath, options.Recursive);
            }
            else
            {
                Console.WriteLine($"{options.InputPath} is neither a directory nor a valid assembly.");
            }
        }

        static void HandleParsingErrors(IEnumerable<Error> errors)
        {
            Console.WriteLine("Failed to parse command line arguments.");
            Console.WriteLine(HelpText.AutoBuild(Parser.Default.ParseArguments<Options>(["--help"])));
        }

        private static void SaveEmbeddedResources(Assembly? assembly, string outFolder, bool recursive)
        {
            if (assembly == null)
            {
                return;
            }

            IEnumerable<byte[]> embeddedAssembliesBytes = GetEmbeddedAssemblies(assembly);

            foreach (var embeddedAssemblyBytes in embeddedAssembliesBytes)
            {
                SaveAssembly(embeddedAssemblyBytes, outFolder);

                if (recursive)
                {
                    Assembly subAssembly = Assembly.Load(embeddedAssemblyBytes);

                    SaveEmbeddedResources(subAssembly, outFolder, recursive);
                }

            }
        }

        private static IEnumerable<byte[]> GetEmbeddedAssemblies(Assembly assembly)
        {
            string[] resourceNames = assembly.GetManifestResourceNames();

            IEnumerable<string> asmzResources = resourceNames.Where(x => x.StartsWith("asmz://"));

            foreach (var asmzResource in asmzResources)
            {
                string[] resourceIdentifier = asmzResource.Replace("asmz://", "").Split("/");

                string flags = resourceIdentifier[2];

                bool compressed = flags.Contains("z");

                using (Stream? resourceStream = assembly.GetManifestResourceStream(asmzResource))
                {
                    if (resourceStream == null)
                    {
                        Console.WriteLine($"Resource '{asmzResource}' not found.");
                        continue;
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (Stream deflateStream = compressed ? new DeflateStream(resourceStream, CompressionMode.Decompress) : resourceStream)
                        {
                            deflateStream.CopyTo(memoryStream);
                        }

                        byte[] resourceBytes = memoryStream.ToArray();

                        yield return resourceBytes;
                    }
                }
            }
        }

        private static void SaveAssembly(byte[] bytes, string outFolder)
        {
            Assembly embeddedAssembly = Assembly.Load(bytes);

            string fileName = embeddedAssembly.GetName().Name ?? "UnknownAssembly";

            Console.WriteLine($"Saving {fileName}.dll");

            File.WriteAllBytes(outFolder + "\\" + fileName + ".dll", bytes);
        }

        private static bool TryLoadAssembly(string filePath, out Assembly? assembly)
        {
            // Check if the file has a .dll or .exe extension
            string extension = Path.GetExtension(filePath).ToLower();
            assembly = null;

            if (extension == ".dll" || extension == ".exe")
            {
                // Attempt to load the assembly to verify if it's valid
                try
                {
                    assembly = Assembly.LoadFrom(filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load {filePath}: {ex.Message}");
                    // Thrown if the file is not a valid assembly
                    return false;
                }
            }

            return false;
        }
    }
}
