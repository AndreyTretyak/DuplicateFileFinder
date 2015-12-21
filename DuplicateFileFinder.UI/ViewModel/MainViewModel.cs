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

        public ObservableCollection<IComparableFile> Files { get; set; }

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
                await Task.Run(async () =>
                {
                    Files = new ObservableCollection<IComparableFile>();
                    foreach (var file in await SelectedProvider.Implementation.GetDirectoryFilesAsync(FolderPath))
                    {
                        Files.Add(file);
                    }
                });
                RaisePropertyChanged(() => Files);
                RaisePropertyChanged(() => FilesFound);
            }
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

        public ObservableCollection<FileGroup> FileGroups { get; set; }

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
                if (Files == null)
                {
                    System.Windows.MessageBox.Show(Resource.PleaseSelectDirectory, Resource.NoDirectorySelected, MessageBoxButton.OK, MessageBoxImage.Information);
                    InProgress = false;
                    return;
                }
                _cancellationTokenSource = new CancellationTokenSource();
                _progressInformation = new ProgressInformation();
                var progress = new Progress<IProgressChanged>(ProgresChanged);

                await Task.Run(async () =>
                {
                    try
                    {
                        FileGroups = new ObservableCollection<FileGroup>();
                        foreach (var fileGroup in await new ComparationManager().FindDuplicatesAsync(Files, _cancellationTokenSource.Token, progress))
                        {
                            FileGroups.Add(fileGroup);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        ProgressValue = 0;
                        ProgressMessage = Resource.OperationCanceled;
                    }
                });
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
}