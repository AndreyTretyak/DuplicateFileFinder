using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.Common.ProgressInformations;

namespace DuplicateFileFinder.Core
{
    public class FilesComparationManager
    {
        private readonly IList<IFileComparator> _comparators;

        public FilesComparationManager(IList<IFileComparator> comparators)
        {
            _comparators = comparators;
        }

        public async Task<IEnumerable<FileGroup>> FindDuplicatesAsync(IEnumerable<IComparableFile> files, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
        {
            return await FindDuplicatesRecursiveStepAsync(new FileGroup(files), 0, cancellationToken, progress);
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesRecursiveStepAsync(FileGroup filesGroup, int comparatorIndex, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
        {
            var result = new List<FileGroup>();
            if (comparatorIndex >= _comparators.Count() || !filesGroup.CanContainDuplicates)
            {
                result.Add(filesGroup);
            }
            else
            {
                var fileGroups = await FindDuplicatesUsingComparatorAsync(_comparators[comparatorIndex], filesGroup, cancellationToken, progress);
                foreach (var fileGroup in fileGroups)
                {
                    result.AddRange(await FindDuplicatesRecursiveStepAsync(fileGroup, comparatorIndex + 1, cancellationToken, progress));
                }
            }
            return result;
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesUsingComparatorAsync(IFileComparator comparator, IEnumerable<IComparableFile> files, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
        {
            var analizer = new ComparationCriteriaAnalizer(comparator);
            await Task.WhenAll(files.Select(f => CompareFileAsync(comparator, f, analizer, cancellationToken, progress)).ToArray());
            return analizer.ResultFileGroups();
        }

        private async Task CompareFileAsync(IFileComparator comparator, IComparableFile file, ComparationCriteriaAnalizer analizer, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
        {
            var criteria = await comparator.GetComparationCriteriaAsync(file, cancellationToken, progress);
            await analizer.AddFileAsync(criteria, file, cancellationToken, progress);
        }

        public class ComparationTask : IComparationTask
        {
            private readonly IFileComparator _comparator;

            private readonly ComparationCriteriaAnalizer _analizer;

            private readonly IComparableFile _file;

            public ComparationTask(IFileComparator comparator, ComparationCriteriaAnalizer analizer, IComparableFile file)
            {
                _comparator = comparator;
                _analizer = analizer;
                _file = file;
            }

            public async Task ExecuteAsync(CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
            {
                var criteria = await _comparator.GetComparationCriteriaAsync(_file, cancellationToken, progress);
                await _analizer.AddFileAsync(criteria, _file, cancellationToken, progress);
            }
        }

        public class ComparationCriteriaAnalizer
        {
            private readonly BlockingCollection<FileGroup> _fileGroups;

            private readonly IFileComparator _comparator;

            public ComparationCriteriaAnalizer(IFileComparator comparator)
            {
                _comparator = comparator;
                _fileGroups = new BlockingCollection<FileGroup>();
            }

            public async Task AddFileAsync(ComparationCriteria criteria, IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
            {
                foreach (var fileGroup in _fileGroups)
                {
                    if (!await _comparator.CompareAsync(criteria, fileGroup.Key, cancellationToken, progress))
                        continue;

                    fileGroup.AddFile(file);
                    break;
                }
                _fileGroups.Add(new FileGroup(criteria, file), cancellationToken);
            }

            public IEnumerable<FileGroup> ResultFileGroups()
            {
                return _fileGroups;
            }
        }
    }

    public interface IComparationTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress);
    }

    public struct FileComparationProgressChanged
    {
        public double Percentage { get; }

        public string Message { get; }

        public FileComparationProgressChanged(double percentage, string message)
        {
            Percentage = percentage;
            Message = message;
        }
    }

    public class ComparationTaskExecutor
    {
        public async Task ExecuteAsync(IList<IComparationTask> tasks, CancellationToken cancellationToken, IProgress<IProgressInformationChanged> progress)
        {
            await Task.WhenAll(tasks.Select(t => t.ExecuteAsync(cancellationToken, progress)).ToArray());
        }

        //private async Task ExecuteTask(IComparationTask task, CancellationToken cancellationToken, IProgress<FileComparationProgressChanged> progress)
        //{
        //    await task.ExecuteAsync(cancellationToken, progress);
        //}
    }
}