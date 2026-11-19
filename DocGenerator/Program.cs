using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

class Program
{
    //Install tool via:
    //dotnet tool install -g XMLDoc2Markdown
    private static string docGen = "dotnet tool run xmldoc2md \"bin/Debug/net6.0/Starlight.dll\" --output ../XMLToMD/ --member-accessibility-level public";
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World! The converting has begun!");
        DirectoryInfo starlightWeb;
        DirectoryInfo apiDev;
        DirectoryInfo api;
        DirectoryInfo xmlToMD;
        string gitDir = "";
        string apiDir = "api-"+Starlight.BuildInfo.DisplayVersion;
        try
        {
            var rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.Parent.FullName;
            gitDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
            string starlightWebDir = Path.Combine(rootDir, "starlight-web");
            starlightWeb = new DirectoryInfo(starlightWebDir);
            string xmltomdDir = Path.Combine(gitDir, "XMLToMD");
            xmlToMD = new DirectoryInfo(xmltomdDir);
            string devDir = Path.Combine(starlightWebDir, "dev");
            string apiDevDir = Path.Combine(devDir, "api");
            apiDev = new DirectoryInfo(apiDevDir);
            api = new DirectoryInfo(Path.Combine(apiDevDir, apiDir));
        }
        catch
        {
            Console.WriteLine("Setup is wrong");
            Console.WriteLine("Are you running this program in the ide?");
            Console.WriteLine("Is the starlight-web cloned next to this git repo?");
            Console.WriteLine("Does dev/api exist in starlight-web?");
            Console.WriteLine("Is the XMLToMD folder in this git repo?");
            Console.WriteLine("Please check everything?");
            return;
        }
        if (!starlightWeb.Exists) { Console.WriteLine("starlight-web missing?"); return; }
        if (!xmlToMD.Exists) { Console.WriteLine("XMLToMD missing?"); xmlToMD.Create(); return; }
        if (!apiDev.Exists) { Console.WriteLine("dev/api missing?"); return; }
        if (!api.Exists) { api.Create();}

        string sourceDir = xmlToMD.FullName;
        string workingDir = api.FullName;
        if(!ExecuteCommand("dotnet tool restore", gitDir+"/Essentials")) return;
        if(!ExecuteCommand(docGen, gitDir+"/Essentials")) return;
        
        if (!Directory.Exists(workingDir)) { Directory.CreateDirectory(workingDir); }
        
        ClearDirectory(workingDir);
        CopyFilesAndDirectories(sourceDir, workingDir);
        CreateCategoryJson(workingDir);
        MoveFiles(workingDir);
        ProcessDirectories(workingDir);
        FixBrTags(workingDir);
        FixMarkDownLinks(workingDir,"/dev/api/"+apiDir);
        DeleteMdFilesInWorkingDir(workingDir);
        //Cleanup
        ClearDirectory(sourceDir);
    }

    static bool ExecuteCommand(string command, string runningDir)
    {
        try
        {
            string shell;
            string shellArgs;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                shell = "cmd.exe";
                shellArgs = $"/c {command}";
            }
            else
            {
                shell = "/bin/bash";
                shellArgs = $"-c \"{command}\"";
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shell, Arguments = shellArgs,
                    RedirectStandardOutput = false, RedirectStandardError = false,
                    UseShellExecute = true, CreateNoWindow = true,
                    WorkingDirectory = runningDir
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0) return true;
            Console.WriteLine($"The command exited with code {process.ExitCode}. Something went wrong.");
            return false;
            
        }
        catch (Exception ex) { Console.WriteLine($"An error occurred while executing the command: {ex.Message}"); }
        return false;
    }
    static void ClearDirectory(string directory)
    {
        foreach (var file in Directory.GetFiles(directory))
        {
            File.Delete(file);
            Console.WriteLine($"Deleted file {file}");
        }

        foreach (var subdir in Directory.GetDirectories(directory))
        {
            Directory.Delete(subdir, true);
            Console.WriteLine($"Deleted directory {subdir}");
        }
    }

    static void CopyFilesAndDirectories(string sourceDir, string workingDir)
    {
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destinationFile = Path.Combine(workingDir, Path.GetFileName(file));
            File.Copy(file, destinationFile, true);
            Console.WriteLine($"Copied {file} to {destinationFile}");
        }
        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            string destinationDir = Path.Combine(workingDir, Path.GetFileName(directory));
            Directory.CreateDirectory(destinationDir);
            CopyFilesAndDirectories(directory, destinationDir);
        }
    }

    static void CreateCategoryJson(string workingDir)
    {
        string categoryContent =
            "{" + "\n" +
            "\"label\": \"Starlight "+Starlight.BuildInfo.DisplayVersion+"\"," + "\n" +
            "\"link\": {" + "\n" +
            "\"type\": \"generated-index\"," + "\n" +
            "\"description\": \"An API of Starlight and its expansions!\"" + "\n" +
            "}" + "\n" +
            "}";

        File.WriteAllText(Path.Combine(workingDir, "_category_.json"), categoryContent);
        Console.WriteLine("_category_.json file created.");
    }

    static void MoveFiles(string workingDir)
    {
        foreach (var file in Directory.GetFiles(workingDir))
        {
            if (!file.EndsWith(".md") || file == "index.md")
                continue;
            string namespaceName = ExtractNamespaceFromFile(file);
            if (string.IsNullOrEmpty(namespaceName))
                continue;
            string folderStructure = namespaceName.Replace('.', Path.DirectorySeparatorChar);
            string folderPath = Path.Combine(workingDir, folderStructure);
            Directory.CreateDirectory(folderPath);
            string newFilename = RemoveNamespacePrefixFromFilename(file, namespaceName);
            string destinationPath = Path.Combine(folderPath, newFilename);
            File.Move(file, destinationPath);
            Console.WriteLine($"Moved {file} to {destinationPath}");
        }
    }

    static string ExtractNamespaceFromFile(string file)
    {
        var content = File.ReadAllText(file);
        var match = Regex.Match(content, @"^Namespace: (.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value : null;
    }

    static string RemoveNamespacePrefixFromFilename(string filename, string namespaceValue)
    {
        
        string[] filenameParts = filename.Split('/');
        string[] namespaceParts = namespaceValue.Split('.');
        string fileNameOnly = filenameParts[filenameParts.Length - 1];
        string newFileName = fileNameOnly;
        foreach (var part in namespaceParts)
        {
            newFileName = newFileName.Replace($"{part}.", "", StringComparison.OrdinalIgnoreCase);
        }
        
        string newFilePath = string.Join("/", filenameParts.Take(filenameParts.Length - 1)) + "/" +
                             string.Join("/", namespaceParts) + "/" + newFileName;
        return newFilePath;
    }

    static void ProcessDirectories(string workingDir)
    {
        foreach (var dir in Directory.GetDirectories(workingDir, "*",SearchOption.AllDirectories))
        {
            var mdFiles = Directory.GetFiles(dir, "*.md");
            var classNames = new List<string>();
            foreach (var file in mdFiles)
                classNames.AddRange(ExtractClassEnumStructInterfaceNames(file));
            var subdirectories = Directory.GetDirectories(dir);
            CreateIndexMd(dir, classNames, subdirectories, workingDir);
        }
    }

    static List<string> ExtractClassEnumStructInterfaceNames(string filePath)
    {
        var names = new List<string>();
        var content = File.ReadAllText(filePath);
        var pattern = @"public (class|enum|struct|interface) (\w+)";
        var matches = Regex.Matches(content, pattern);
        foreach (Match match in matches)
            names.Add(match.Groups[2].Value);
        return names;
    }

    static void CreateIndexMd(string directory, List<string> classNames, string[] subdirectories, string workingDir)
    {
        string folderName = Path.GetFileName(directory);
        string indexMdPath = Path.Combine(directory, "index.md");
        using (var writer = new StreamWriter(indexMdPath))
        {
            writer.WriteLine($"# {folderName}\n");
            if (classNames.Any())
            {
                writer.WriteLine("## Files:\n");
                foreach (var name in classNames.OrderBy(n => n))
                {
                    string link = $"[{name}](./{folderName}/{name.ToLower()})";
                    writer.WriteLine($"- {link}");
                }
            }
            if (subdirectories.Any())
            {
                writer.WriteLine("\n## Namespaces:\n");
                foreach (var subdir in subdirectories.OrderBy(d => d))
                {
                    string subdirName = Path.GetFileName(subdir);
                    string subdirLink = $"[{subdirName}]({subdirName}/index.md)";
                    writer.WriteLine($"- {subdirLink}");
                }
            }
        }
        Console.WriteLine($"Created index.md in {directory}");
    }
    
    static void FixMarkDownLinks(string workingDir, string baseDir)
    {
        foreach (var file in Directory.GetFiles(workingDir, "*.md", SearchOption.AllDirectories))
        {
            if (file.EndsWith("index.md")) continue;
            File.WriteAllText(file, ReplaceMarkdownLinks(File.ReadAllText(file),baseDir));
            Console.WriteLine($"Fixed markdownlinks in  {file}");
        }
    }

    static string ReplaceMarkdownLinks(string input, string basedir)
    {
        
        string pattern = @"\((\./[^\)]*\.md)\)";
        string result = Regex.Replace(input, pattern, match =>
        {
            string originalLink = match.Groups[1].Value;
            
            string prefix = "./";
            string linkWithoutPrefix = originalLink.Substring(prefix.Length);
            
            int lastDotIndex = linkWithoutPrefix.LastIndexOf('.');
            int secondLastDotIndex = linkWithoutPrefix.LastIndexOf('.', lastDotIndex - 1);

            if (secondLastDotIndex == -1)
            {
                string transformed = linkWithoutPrefix.Replace('.', '/');
                return $"({basedir}/{transformed.Substring(0, transformed.Length - 3)})";
            }
            
            string transformedPrefix = linkWithoutPrefix.Substring(0, secondLastDotIndex).Replace('.', '/');
            string suffix = linkWithoutPrefix.Substring(secondLastDotIndex + 1, lastDotIndex - secondLastDotIndex - 1);
            
            return $"({basedir}/{transformedPrefix}/{suffix})";
        });

        return result;
    }

    static void FixBrTags(string workingDir)
    {
        foreach (var file in Directory.GetFiles(workingDir, "*.md", SearchOption.AllDirectories))
        {
            if (file == "index.md") continue;
            string content = File.ReadAllText(file);
            string fixedContent = Regex.Replace(content, @"<br>(?!<\/br>)", "<br />");
            File.WriteAllText(file, fixedContent);
            Console.WriteLine($"Fixed <br> tags in {file}");
        }
    }

    static void DeleteMdFilesInWorkingDir(string workingDir)
    {
        var mdFiles = Directory.GetFiles(workingDir, "*.md", SearchOption.TopDirectoryOnly);
        foreach (var file in mdFiles)
        {
            File.Delete(file);
            Console.WriteLine($"Deleted {file}");
        }
    }
}