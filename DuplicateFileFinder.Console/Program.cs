using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Azure;
using DuplicateFileFinder.Core.Comparators;
using DuplicateFileFinder.Core.Providers;

namespace DuplicateFileFinder.Console
{
    internal class Program
    {
        private static bool _hasArguments;

        static void Main(string[] args)
        {
            var provider = new AzureFileProvider();
            provider.GetFileAsync("");

            _hasArguments = args.Length > 0;
            
            //select directory
            string path;
            if (_hasArguments)
            {
                path = args[0];
                if (args.Length > 1)
                {
                    System.Console.SetOut(new StreamWriter(args[1]));
                }
            }
            else
            {
                System.Console.Write(Resources.EnterPathToFolder);
                path = System.Console.ReadLine();
            }

            //find dublicates
            new ConsoleDuplicateFinder().FindAsync(path).ContinueWith(t => OutputResult(t.Result));
        }

        //output result
        private static void OutputResult(IEnumerable<FileGroup> result)
        {
            foreach (var fileGroup in result)
            {
                if (fileGroup.Count > 1)
                {
                    System.Console.WriteLine(Resources.DuplicateFiles);
                    foreach (var file in fileGroup)
                    {
                        System.Console.WriteLine(Resources.DuplicateFileFormat, file.FileName);
                    }
                }
                else
                {
                    System.Console.WriteLine(Resources.UniqueFileFormat, fileGroup.Single().FileName);
                }
            }

            if (_hasArguments) return;
            System.Console.WriteLine(Resources.PressAnyKey);
            System.Console.ReadKey();
        }
    }

    internal class ConsoleDuplicateFinder
    {
        private readonly ProgressInformation _progressInfo;

        private readonly IFileProvider _fileProvider;

        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsoleDuplicateFinder()
        {
            _progressInfo = new ProgressInformation();
            _fileProvider = new LocalFileProvider();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<IEnumerable<FileGroup>> FindAsync(string path)
        {
            var progressTracker = new Progress<IProgressChanged>(ProgressInformationChanged);
            return await new ComparationManager().FindDuplicatesAsync(await _fileProvider.GetDirectoryFilesAsync(path), _cancellationTokenSource.Token, progressTracker);
        }

        private void ProgressInformationChanged(IProgressChanged progressChanged)
        {
            progressChanged.ApplyChanges(_progressInfo);
            System.Console.WriteLine(Resources.ProgressChangedFormat, _progressInfo.Percentege, _progressInfo.CurrentAction);
        }
    }
}