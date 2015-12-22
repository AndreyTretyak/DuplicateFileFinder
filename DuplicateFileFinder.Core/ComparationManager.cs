using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.Comparators;
using DuplicateFileFinder.Core.ProgressChanges;

namespace DuplicateFileFinder.Core
{
    public class ComparationManager
    {
        private readonly IReadOnlyList<IFileComparator> _comparators;

        public ComparationManager() : this(new DefaultComparatorsFactory())
        {
        }

        public ComparationManager(IComparatorsFactory comparatorsFactory)
        {
            _comparators = comparatorsFactory.GetComparators();
        }

        public async Task<IEnumerable<FileGroup>> FindDuplicatesAsync(IEnumerable<IComparableFile> files)
        {
            return await FindDuplicatesAsync(files, CancellationToken.None, new Progress<IProgressChanged>());
        }

        public async Task<IEnumerable<FileGroup>> FindDuplicatesAsync(IEnumerable<IComparableFile> files, CancellationToken cancellationToken)
        {
            return await FindDuplicatesAsync(files, cancellationToken, new Progress<IProgressChanged>());
        }

        public async Task<IEnumerable<FileGroup>> FindDuplicatesAsync(IEnumerable<IComparableFile> files, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            var fileGroup = new FileGroup(files);
            progress?.Report(new FilesAndComparatorsCountInfo(fileGroup.Count, _comparators.Count));
            var result = await FindDuplicatesRecursiveStepAsync(0, fileGroup, cancellationToken, progress);
            progress.Report(new ProcessingFinished(result.Where(g => g.Count > 1).Sum(g => g.Count - 1)));
            return result;
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesRecursiveStepAsync(int comparatorIndex, FileGroup files, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            var result = new List<FileGroup>();
            if (comparatorIndex >= _comparators.Count || !files.CanContainDuplicates)
            {
                result.Add(files);
                progress.Report(new FileProcessingFinished(comparatorIndex, files.Count));
            }
            else
            {
                var fileGroups = await FindDuplicatesUsingComparatorAsync(_comparators[comparatorIndex], files, cancellationToken, progress);
                progress.Report(new FilesProcessedByComparator(comparatorIndex, files.Count));
                foreach (var fileGroup in fileGroups)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    result.AddRange(await FindDuplicatesRecursiveStepAsync(comparatorIndex + 1, fileGroup, cancellationToken, progress));
                }
            }
            return result;
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesUsingComparatorAsync(IFileComparator comparator, IEnumerable<IComparableFile> files, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            var tasks = await Task.WhenAll(files.Select(f => GetComparationCriteriaAsyn(comparator, f, cancellationToken, progress)));
            var result = new List<FileGroup>();
            foreach (var task in tasks)
            {
                if (task.Exception != null)
                {
                    progress.Report(new FileReadingError(task.Exception, task.File));
                    result.Add(new FileGroup(task.Exception, task.File));
                    continue;
                }

                try
                {
                    FileGroup? corespondingGroup = null;
                    foreach (var fileGroup in result.Where(g => !g.IsError))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!await comparator.CompareAsync(fileGroup.Key, task.Criteria, cancellationToken, progress))
                            continue;

                        corespondingGroup = fileGroup;
                        break;
                    }
                    if (corespondingGroup.HasValue)
                        corespondingGroup.Value.AddFile(task.File);
                    else
                        result.Add(new FileGroup(task.Criteria, task.File));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    progress.Report(new FileReadingError(task.Exception, task.File));
                    result.Add(new FileGroup(exception, task.File));
                }
            }
            return result;
        }

        private async Task<CriteriaCalculationResult> GetComparationCriteriaAsyn(IFileComparator comparator, IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            try
            {
                var result = await comparator.GetComparationCriteriaAsync(file, cancellationToken, progress);
                return new CriteriaCalculationResult(result, file);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                progress.Report(new FileReadingError(exception, file));
                return new CriteriaCalculationResult(exception, file);
            }
        }

        private struct CriteriaCalculationResult
        {
            public IComparableFile File { get; }
            public ComparationCriteria Criteria { get;}
            public Exception Exception { get; }

            public CriteriaCalculationResult(Exception exception, IComparableFile file) : this()
            {
                File = file;
                Exception = exception;
            }

            public CriteriaCalculationResult(ComparationCriteria criteria, IComparableFile file) : this()
            {
                File = file;
                Criteria = criteria;
            }
        }
    }
}