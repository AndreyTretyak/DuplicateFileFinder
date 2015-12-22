using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;
using DuplicateFileFinder.Core.Providers;
using DuplicateFileFinder.UI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DuplicateFileFinder.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            Comparators = new ObservableCollection<ImplementationViewModel<IFileComparator>>
            (
                new DefaultComparatorsFactory().GetComparators().Select(c => new ImplementationViewModel<IFileComparator>(c, true))
            );
            SelectedComparator = Comparators.FirstOrDefault();

            Providers = new ObservableCollection<ImplementationViewModel<IFileProvider>>
            (
                new [] { new ImplementationViewModel<IFileProvider>(new LocalFileProvider(), true)}
            );
            SelectedProvider = Providers.FirstOrDefault();
        }

        public ObservableCollection<FileSystemItemViewModel> Items { get; set; }

        private List<IComparableFile> Files { get; } = new List<IComparableFile>();

        public string FilesFound => Files?.Count > 0 ? string.Format(Resource.Files, Files.Count) : string.Empty;

        private string _folderPath;

        public string FolderPath
        {
            get { return _folderPath; }
            set
            {
                _folderPath = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand OpenFolderSelectorCommand => new RelayCommand(OpenFolderSelector);

        private async void OpenFolderSelector()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FolderPath = dialog.SelectedPath;

                List<FileSystemItemViewModel> list = null;
                if (Items == null)
                    Items = new ObservableCollection<FileSystemItemViewModel>();
                else
                    Items.Clear();
                FileGroups?.Clear();
                ResultSummary = string.Empty;
                await Task.Run(async () =>
                {
                    var directory = await SelectedProvider.Implementation.GetDirectoryAsync(FolderPath);
                    Files.Clear();
                    list = await GetDirectoryContentAsync(directory);
                });
                if (list != null)
                {
                    foreach (var file in list)
                    {
                        Items.Add(file);
                    }
                }
                RaisePropertyChanged(() => Items);
                RaisePropertyChanged(() => FilesFound);
            }
        }

        public async Task<List<FileSystemItemViewModel>> GetDirectoryContentAsync(IDirectory dir)
        {
            var list = new List<FileSystemItemViewModel>();
            foreach (var subDir in await dir.GetDirectoriesAsync())
            {
                list.Add(new FileSystemItemViewModel(subDir,await GetDirectoryContentAsync(subDir)));
            }
            var files = await dir.GetFilesAsync();
            Files.AddRange(files);
            list.AddRange(files.Select(f => new FileSystemItemViewModel(f)));
            return list;
        }

        public RelayCommand ChangeComparatorStatusCommand => new RelayCommand(ChangeComparatorStatus);

        private void ChangeComparatorStatus()
        {
            SelectedComparator.IsEnabled = !SelectedComparator.IsEnabled;
            RaisePropertyChanged(() => ComparatorButtonText);
        }

        public string ComparatorButtonText => SelectedComparator != null && SelectedComparator.IsEnabled ? Resource.Disable : Resource.Enable;

        private ImplementationViewModel<IFileComparator> _selectedComparator;

        public ImplementationViewModel<IFileComparator> SelectedComparator
        {
            get { return _selectedComparator; }
            set
            {
                _selectedComparator = value;
                RaisePropertyChanged(() => ComparatorButtonText);
            }
        }

        private IEnumerable<TInterface> GetRequiredTypesFromAssambly<TInterface>()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".dll",
                Filter = Resource.AssemblyFileTypes
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var assambly = Assembly.LoadFrom(dialog.FileName);
                    return assambly.GetTypes()
                                    .Where(t => typeof(TInterface).IsAssignableFrom(t)
                                        && t.GetConstructor(Type.EmptyTypes) != null)
                                    .Select(t => (TInterface)assambly.CreateInstance(t.FullName))
                                    .ToArray();

                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show(exception.ToString(), Resource.FaildedToLoadAssembly, MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
            return Enumerable.Empty<TInterface>();
        }

        public RelayCommand AddProviderCommand => new RelayCommand(AddProvider);

        private void AddProvider()
        {
            foreach(var provider in GetRequiredTypesFromAssambly<IFileProvider>())
                Providers.Add(new ImplementationViewModel<IFileProvider>(provider, true));
        }

        public ObservableCollection<ImplementationViewModel<IFileComparator>> Comparators { get; set; }

        public RelayCommand AddComparatorCommand => new RelayCommand(AddComparator);

        private void AddComparator()
        {
            foreach (var comparator in GetRequiredTypesFromAssambly<IFileComparator>())
                Comparators.Add(new ImplementationViewModel<IFileComparator>(comparator, true));
        }

        public RelayCommand MoveUpCommand => new RelayCommand(MoveUp);

        private void MoveUp()
        {
            var index = Comparators.IndexOf(SelectedComparator);
            if (index == 0) return;
            Comparators.Move(index, index - 1);
        }

        public RelayCommand MoveDownCommand => new RelayCommand(MoveDown);

        private void MoveDown()
        {
            var index = Comparators.IndexOf(SelectedComparator);
            if (index == Comparators.Count - 1) return;
            Comparators.Move(index, index + 1);
        }

        public ImplementationViewModel<IFileProvider> SelectedProvider { get; set; }

        public ObservableCollection<ImplementationViewModel<IFileProvider>> Providers { get; set; }

        public RelayCommand FindDuplicatesCommand => new RelayCommand(FindDuplicates);

        public ObservableCollection<FileGroupViewModel> FileGroups { get; set; }

        private bool _inProgress = false;

        public bool InProgress
        {
            get { return _inProgress; }
            private set
            {
                _inProgress = value;
                RaisePropertyChanged(() => StartButtonText);
            }
        }

        public string StartButtonText => InProgress ? Resource.Cancel : Resource.FindDuplicates;

        private async void FindDuplicates()
        {
            if (!InProgress)
            {
                InProgress = true;
                if (Items == null)
                {
                    System.Windows.MessageBox.Show(Resource.PleaseSelectDirectory, Resource.NoDirectorySelected, MessageBoxButton.OK, MessageBoxImage.Information);
                    InProgress = false;
                    return;
                }
                _cancellationTokenSource = new CancellationTokenSource();
                _progressInformation = new ProgressInformation();
                var progress = new Progress<IProgressChanged>(ProgresChanged);

                var isCanceled = false;
                IEnumerable<FileGroup> fileGroups = null; 
                await Task.Run(async () =>
                {
                    try
                    {
                        var factory = new CustomComparatorFactory(Comparators.Where(c => c.IsEnabled).Select(c => c.Implementation));
                        fileGroups = await new ComparationManager(factory).FindDuplicatesAsync(Files, _cancellationTokenSource.Token, progress);
                    }
                    catch (OperationCanceledException)
                    {
                        isCanceled = true;
                        ProgressValue = 0;
                        ProgressMessage = Resource.OperationCanceled;
                    }
                });

                if (FileGroups == null)
                {
                    FileGroups = new ObservableCollection<FileGroupViewModel>();
                }
                else 
                    FileGroups.Clear();

                if (!isCanceled && fileGroups != null)
                {
                    foreach (var fileGroup in fileGroups.Where(g => g.Count > 1).OrderByDescending(g => g.Count))
                    {
                        FileGroups.Add(new FileGroupViewModel(fileGroup));
                    }
                    ResultSummary = _progressInformation.CurrentAction;
                }
                RaisePropertyChanged(() => FileGroups);
                InProgress = false;
            }
            else
            {
                _cancellationTokenSource.Cancel(true);
            }
        }
        
        private CancellationTokenSource _cancellationTokenSource;

        private ProgressInformation _progressInformation;
        private string _progressMessage;
        private double _progressValue;
        private string _resultSummary;

        private void ProgresChanged(IProgressChanged changes)
        {
            changes.ApplyChanges(_progressInformation);
            ProgressMessage = $"{_progressInformation.Percentege:P} - {_progressInformation.CurrentAction}";
            ProgressValue = _progressInformation.Percentege * 100;
        }

        public string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                RaisePropertyChanged();
            }
        }

        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                RaisePropertyChanged();
            }
        }

        public string ResultSummary
        {
            get { return _resultSummary; }
            set
            {
                _resultSummary = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SaveResultCommand => new RelayCommand(SaveResult);

        private void SaveResult()
        {

        }
    }

    public class CustomComparatorFactory : IComparatorsFactory
    {
        private readonly IReadOnlyList<IFileComparator> _comparators;

        public CustomComparatorFactory(IEnumerable<IFileComparator> comparators)
        {
            _comparators = comparators.ToList().AsReadOnly();
        }


        public IReadOnlyList<IFileComparator> GetComparators()
        {
            return _comparators;
        }
    }

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