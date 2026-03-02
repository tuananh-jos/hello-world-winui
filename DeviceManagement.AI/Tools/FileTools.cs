using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace DeviceManagement.AI.Tools
{
    public class FileTools
    {
        private readonly string _projectRoot;

        public FileTools(string projectRoot)
        {
            _projectRoot = projectRoot;
        }

        private string SafePath(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(_projectRoot, relativePath));

            if (!fullPath.StartsWith(_projectRoot))
                throw new UnauthorizedAccessException("Access outside project root is not allowed.");

            return fullPath;
        }

        [KernelFunction]
        public async Task<string> ReadFile(string path)
        {
            var fullPath = SafePath(path);

            if (!File.Exists(fullPath))
                return "File does not exist.";

            return await File.ReadAllTextAsync(fullPath);
        }

        [KernelFunction]
        public async Task<string> WriteFile(string path, string content)
        {
            var fullPath = SafePath(path);

            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            await File.WriteAllTextAsync(fullPath, content);

            return "File written successfully.";
        }

        [KernelFunction]
        public string ListFiles(string folder)
        {
            var fullPath = SafePath(folder);

            if (!Directory.Exists(fullPath))
                return "Directory does not exist.";

            var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);

            return string.Join(Environment.NewLine,
                files.Select(f => Path.GetRelativePath(_projectRoot, f)));
        }

        [KernelFunction]
        public async Task<string> ReplaceInFile(string path, string oldText, string newText)
        {
            var fullPath = SafePath(path);

            if (!File.Exists(fullPath))
                return "File does not exist.";

            var content = await File.ReadAllTextAsync(fullPath);

            if (!content.Contains(oldText))
                return "Old text not found.";

            content = content.Replace(oldText, newText);

            await File.WriteAllTextAsync(fullPath, content);

            return "Replacement successful.";
        }
    }
}
