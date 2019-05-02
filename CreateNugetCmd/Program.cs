using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace CreateNugetCmd
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 2)
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

                string version = versionElement.Value;
                string packageId = packageIdElement.Value;

                // Trim last 0 out of version
                //var parts = version.Split(new char[] { '.'});
                var real = new Version(version);

                var nugetCmdPath = ConfigurationManager.AppSettings["nugetCmdPath"];
                if (string.IsNullOrWhiteSpace(nugetCmdPath))
                {
                    nugetCmdPath = "nuget";
                }
                else if ((nugetCmdPath.IndexOf(' ') >= 0) && (nugetCmdPath.IndexOf('"') == -1))
                {
                    nugetCmdPath = string.Concat("\"", nugetCmdPath, "\"");
                }

                var source = ConfigurationManager.AppSettings["source"];
                var nupkgName = $"{packageId}.{real}.nupkg";

                //e.g.
                //nuget add My.Test.Legacy.Models.1.0.1808.12270.nupkg -source \\MyServer\test_packages
                var cmd = $"{nugetCmdPath} add {nupkgName} -source {source}";

                var path = Path.Combine(nugetDirectory, $"Create-{nupkgName}.cmd");

                File.WriteAllText(path, cmd);
                return 0;
            }
            return -11;
        }
    }
}
