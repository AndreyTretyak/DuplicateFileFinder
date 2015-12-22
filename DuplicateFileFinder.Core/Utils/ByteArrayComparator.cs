using System;
using System.Runtime.InteropServices;

namespace DuplicateFileFinder.Core.Utils
{
    internal static class ByteArrayComparator
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool Compare(byte[] first, byte[] second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return first.Length == second.Length && memcmp(first, second, first.Length) == 0;
        }
    }
}
