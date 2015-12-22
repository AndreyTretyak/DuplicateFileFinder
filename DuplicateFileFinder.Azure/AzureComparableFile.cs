using System;
using System.CodeDom;
using System.IO;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.Core.Azure
{
    internal class AzureComparableFile : IComparableFile
    {
        public string FileName
        {
            get { throw new NotImplementedException(); }
        }

        public Task<ulong> GetFileSizeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Stream> GetFileStreamAsync()
        {
            throw new NotImplementedException();
        }
    }
}