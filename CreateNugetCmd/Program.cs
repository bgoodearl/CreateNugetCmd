using CreateNugetCmd.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CreateNugetCmd
{
    class Program
    {
        static AppSettings appSettings = new AppSettings();

        static int Main(string[] args)
        {
            string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //string currentDirectory = Directory.GetCurrentDirectory();
            //if (assemblyDirectory != currentDirectory)
            //{
            //    Console.WriteLine($"assemblyDirectory = [{assemblyDirectory}]");
            //    Console.WriteLine($"currentDirectory = [{currentDirectory}]");
            //}
            var builder = new ConfigurationBuilder()
                .SetBasePath(assemblyDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.localdev.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();
            ConfigurationBinder.Bind(configuration.GetSection("AppSettings"), appSettings);

            if ((args.Length == 2) && !string.IsNullOrEmpty(appSettings.nugetCmdPath)
                && !string.IsNullOrWhiteSpace(appSettings.source))
            {
                var targetDir = args[0];
                var csprojPath = args[1];

                var directory = Directory.GetParent(targetDir);
                var nugetDirectory = directory.Parent.FullName;

                var xml = XDocument.Load(new StreamReader(csprojPath));
                XElement versionElement = xml.Root.Descendants().Where(n => n.Name == "Version").FirstOrDefault();
                XElement packageIdElement = xml.Root.Descendants().Where(n => n.Name == "PackageId").FirstOrDefault();
                if (versionElement == null)
                {
                    Console.Error.Write("version element not found");
                    return -1;
                }
                if (packageIdElement == null)
                {
                    Console.Error.Write("package id element not found");
                    return -2;
                }
                //Console.WriteLine($"symbols: [{appSettings.symbols}]");

                string version = versionElement.Value;
                string packageId = packageIdElement.Value;

                // Trim last 0 out of version
                //var parts = version.Split(new char[] { '.'});
                var real = new Version(version);

                var nugetCmdPath = appSettings.nugetCmdPath;
                if (string.IsNullOrWhiteSpace(nugetCmdPath))
                {
                    nugetCmdPath = "nuget";
                }
                else if ((nugetCmdPath.IndexOf(' ') >= 0) && (nugetCmdPath.IndexOf('"') == -1))
                {
                    nugetCmdPath = string.Concat("\"", nugetCmdPath, "\"");
                }

                var source = appSettings.source;
                var nupkgName = $"{packageId}.{real}.nupkg";
                var snupkgName = $"{packageId}.{real}.snupkg";

                //e.g.
                //nuget add My.Test.Legacy.Models.1.0.1808.12270.nupkg -source \\MyServer\test_packages
                StringBuilder cmd = new StringBuilder();
                cmd.AppendLine($"{nugetCmdPath} add {nupkgName} -source {source}");
                if (!string.IsNullOrWhiteSpace(appSettings.symbols))
                {
                    if (Directory.Exists(appSettings.symbols))
                    {
                        cmd.AppendLine($"IF EXIST \"{snupkgName}\" copy \"{snupkgName}\" \"{appSettings.symbols}\"");
                    }
                    else
                    {
                        cmd.AppendLine($"@rem symbols path: \"{appSettings.symbols}\"");
                    }
                }

                var path = Path.Combine(nugetDirectory, $"Create-{nupkgName}.cmd");

                File.WriteAllText(path, cmd.ToString());
                return 0;
            }
            Console.WriteLine($"args.Length = {args.Length}, [{appSettings.nugetCmdPath}], [{appSettings.source}], [{appSettings.symbols}]");
            return -11;
        }
    }
}
