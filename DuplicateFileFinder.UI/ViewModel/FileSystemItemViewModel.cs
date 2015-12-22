using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.UI.ViewModel
{
    public class FileSystemItemViewModel
    {
        public string Name { get;  }
        public ObservableCollection<FileSystemItemViewModel> Items { get; }

        public FileSystemItemViewModel(IDirectory directory, IEnumerable<FileSystemItemViewModel> content)
        {
            Name = directory.Name;
            Items = new ObservableCollection<FileSystemItemViewModel>();
            foreach (var item in content)
            {
                Items.Add(item);
            }
        }

        public FileSystemItemViewModel(IComparableFile file)
        {
            Name = Path.GetFileName(file.FileName);
            Items = new ObservableCollection<FileSystemItemViewModel>();
        }

    }
}