namespace DuplicateFileFinder.Core
{
    public struct ComparationCriteria
    {
        public object Value { get; }

        public T GetValue<T>() => (T)Value;

        public ComparationCriteria(object value)
        {
            Value = value;
        }
    }
}
