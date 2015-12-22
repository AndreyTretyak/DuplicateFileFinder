namespace DuplicateFileFinder.Core.ProgressChanges
{
    public struct CurrentActionChanged : IProgressChanged
    {
        private readonly string _actonName;

        private readonly IComparableFile _firstFile;

        private readonly IComparableFile _secondFile;

        public CurrentActionChanged(string actionName, IComparableFile firstFile, IComparableFile secondFile = null)
        {
            _actonName = actionName;
            _firstFile = firstFile;
            _secondFile = secondFile;
        }
        
        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.CurrentAction = _secondFile == null 
                ? string.Format(Resources.CurrentActionForFileMessage, _actonName, _firstFile.FileName) 
                : string.Format(Resources.CurrentActionForFilesMessage, _actonName, _firstFile.FileName, _secondFile.FileName);
        }
    }
}