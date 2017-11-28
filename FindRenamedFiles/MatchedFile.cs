using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindRenamedFiles
{
    sealed class MatchedFile
    {
        public FileDetail Source { get; }
        public FileDetail Target { get; }
        public bool ProcessFile { get; set; }

        public string NewTargetFullPath { get; }

        public bool Processed { get; private set; }

        public MatchedFile(FileDetail source, FileDetail target, string newTargetFullPath)
        {
            ProcessFile = true;
            Source = source;
            Target = target;
            NewTargetFullPath = newTargetFullPath;
        }

        public bool Process()
        {
            var originalTargetFullPath = Target.FullPath;
            if (File.Exists(NewTargetFullPath)) // a file would be overwritten, skipping
                return Processed = false;

            Directory.CreateDirectory(Path.GetDirectoryName(NewTargetFullPath));
            File.Move(originalTargetFullPath, NewTargetFullPath);

            return Processed = true;
        }
    }
}
