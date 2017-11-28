using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindRenamedFiles
{
    sealed class FileIndex
    {
        readonly DirectoryInfo _root;

        List<FileDetail> _files;
        IDictionary<string, FileDetail> _filesByRelativePath;
        ILookup<long, FileDetail> _filesByLength;

        public FileIndex(string path)
        {
            _root = new DirectoryInfo(path);

            _files = new List<FileDetail>();
            Explore(_root);
            _filesByRelativePath = _files.ToDictionary(x => x.RelativePath);
            _filesByLength = _files.ToLookup(x => x.Length);
        }

        void Explore(DirectoryInfo parent)
        {
            foreach (var file in parent.EnumerateFiles())
                _files.Add(new FileDetail(file, _root));

            foreach (var directory in parent.GetDirectories())
                Explore(directory);
        }

        public IEnumerable<MatchedFile> MatchAgainst(FileIndex targetFileIndex)
        {
            foreach (var sourceFile in _files)
            {
                {
                    FileDetail targetFile;

                    if (targetFileIndex._filesByRelativePath.TryGetValue(sourceFile.RelativePath, out targetFile)) // same path, likely to be the same file
                    {
                        if (sourceFile.Length == targetFile.Length) // same length, assume it's the same file
                        {
                            continue;
                        }
                    }
                }

                // there is no matching file at the same relative path, let's look for the same length

                var targetFilesWithTheSameLength = targetFileIndex._filesByLength[sourceFile.Length];
                foreach (var targetFile in targetFilesWithTheSameLength)
                {
                    var sourceFileChecksum = sourceFile.ComputeChecksum();
                    var targetFileChecksum = targetFile.ComputeChecksum();

                    if (sourceFileChecksum.Equals(targetFileChecksum)) // we have a match
                    {
                        var newTargetFullPath = Path.Combine(targetFileIndex._root.FullName, sourceFile.RelativePath);
                        yield return new MatchedFile(sourceFile, targetFile, newTargetFullPath);
                    }
                }
            }
        }
    }
}
