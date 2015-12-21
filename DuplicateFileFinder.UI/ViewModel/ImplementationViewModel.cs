using GalaSoft.MvvmLight;

namespace DuplicateFileFinder.UI.ViewModel
{
    public class ImplementationViewModel<T> : ViewModelBase
    {
        private bool _isEnabled;
        public T Implementation { get; }

        public string Name => Implementation.GetType().FullName;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        public ImplementationViewModel(T implementation, bool isEnabled)
        {
            Implementation = implementation;
            IsEnabled = isEnabled;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}