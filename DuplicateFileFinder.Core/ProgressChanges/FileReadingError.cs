using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.ProgressChanges
{
    internal struct FileReadingError : IProgressChanged
    {
        private readonly IComparableFile _file;
        private readonly Exception _exception;

        public FileReadingError(Exception exception, IComparableFile file)
        {
            _file = file;
            _exception = exception;
        }

        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.CurrentAction =  string.Format(Resources.ErrorInFileReading,_file.FileName, _exception.Message);
            var list = progressInformation.Exceptions.GetOrAdd(_file, new List<Exception>());
            list.Add(_exception);
        }
    }
}
