using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FindRenamedFiles
{
    sealed class FileDetail
    {
        const long ChecksumBlockSize = 2048;
        const long HalfChecksumBlockSize = ChecksumBlockSize / 2;
        const long MaximumLengthForFullCheksum = 4 * ChecksumBlockSize;

        readonly FileInfo _fileInfo;

        string _checksum;

        public long Length { get; }

        public string RelativePath { get; }

        public string FullPath => _fileInfo.FullName;

        public FileDetail(FileInfo fileInfo, DirectoryInfo rootDirectory)
        {
            _fileInfo = fileInfo;
            Length = fileInfo.Length;
            RelativePath = fileInfo.FullName.Substring(rootDirectory.FullName.Length + 1);
        }

        public string ComputeChecksum()
        {
            if (_checksum == null)
            {
                using (var md5 = MD5.Create())
                {
                    byte[] hash;
                    if (Length <= MaximumLengthForFullCheksum)
                    {
                        hash = md5.ComputeHash(File.ReadAllBytes(_fileInfo.FullName));
                    }
                    else
                    {
                        using (var stream = _fileInfo.OpenRead())
                        {
                            var readBuffer = new byte[ChecksumBlockSize];
                            int readLength;

                            // read some from the beginning
                            readLength = stream.Read(readBuffer, 0, readBuffer.Length);
                            md5.TransformBlock(readBuffer, 0, readLength, readBuffer, 0);

                            // read some from the middle
                            stream.Seek(Length / 2 - HalfChecksumBlockSize, SeekOrigin.Begin);
                            readLength = stream.Read(readBuffer, 0, readBuffer.Length);
                            md5.TransformBlock(readBuffer, 0, readLength, readBuffer, 0);

                            // read some from the end
                            stream.Seek(ChecksumBlockSize, SeekOrigin.End);
                            readLength = stream.Read(readBuffer, 0, readBuffer.Length);
                            md5.TransformFinalBlock(readBuffer, 0, readLength);

                            hash = md5.Hash;
                        }
                    }

                    _checksum = BitConverter.ToString(hash);
                }
            }

            return _checksum;
        }
    }
}
