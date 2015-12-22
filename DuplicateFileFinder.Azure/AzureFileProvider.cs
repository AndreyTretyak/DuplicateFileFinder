using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DuplicateFileFinder.Core.Azure
{
    public class AzureFileProvider : IFileProvider
    {
        private readonly CloudBlobContainer _blobContainer;


        public AzureFileProvider() 
            : this("DefaultEndpointsProtocol=https;AccountName=andreystoragetest;AccountKey=pIY4th9YCSbOETYZR0jG0Y0R8uyw3IBvMHjrOOuswZZ9Gh7Msmgp3CVLSS38vhBMUspfSVmBlz9DDdwPNWjUXQ==", 
                  "duplicatefiles")
        {
        }

        public AzureFileProvider(string connnectionString, string containerName)
        {
            var account = CloudStorageAccount.Parse(connnectionString);
            var blobClient = account.CreateCloudBlobClient();
            _blobContainer = blobClient.GetContainerReference(containerName);
        }

        public Task<IComparableFile> GetFileAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IDirectory> GetDirectoryAsync(string path)
        {
            var file = _blobContainer.ListBlobs(prefix: path, useFlatBlobListing: true).First();


            throw new NotImplementedException();
        }
    }

    public class AzureDirectory : IDirectory
    {
        public string Name { get; }

        public Task<IList<IDirectory>> GetDirectoriesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IList<IComparableFile>> GetFilesAsync()
        {
            throw new NotImplementedException();
        }
    }
}