using System.Collections.ObjectModel;
using System.Linq;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.UI.ViewModel
{
    public class FileGroupViewModel
    {
        private readonly FileGroup _fileGroup;

        public string Name { get; }
        public bool IsError => _fileGroup.IsError;
        public ObservableCollection<FileGroupViewModel> Files { get; }

        public FileGroupViewModel(FileGroup fileGroup)
        {
            _fileGroup = fileGroup;
            Name = (_fileGroup.IsError ? $"({Resource.FailedToLoad}) " : string.Empty) + _fileGroup.First().FileName;

            Files = new ObservableCollection<FileGroupViewModel>();
            if (_fileGroup.Count > 1)
            {
                foreach (var file in _fileGroup.Skip(1))
                {
                    Files.Add(new FileGroupViewModel(file));
                }
            }
        }

        public FileGroupViewModel(IComparableFile file)
        {
            Name = file.FileName;
        }
    }
}