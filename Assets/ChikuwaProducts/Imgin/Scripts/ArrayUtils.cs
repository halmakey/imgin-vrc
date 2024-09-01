using System;

namespace Chikuwa.Imgin
{
    public class ArrayUtils
    {
        public static T[] Append<T>(T[] values, T value)
        {
            if (value == null) {
                return values;
            }
            T[] copy = new T[values.Length + 1];
            Array.Copy(values, copy, values.Length);
            copy[values.Length] = value;
            return copy;
        }

        public static T[] Remove<T>(T[] values, T value)
        {
            var c = 0;
            T[] copy = new T[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (Equals(values[i], value))
                {
                    continue;
                }
                copy[c] = values[i];
                c++;
            }
            T[] result = new T[c];
            Array.Copy(copy, result, c);
            return result;
        }

        public static T[] FilterNonNull<T>(T[] values)
        {
            int size = 0;
            T[] copy = new T[values.Length];
            foreach (var value in values)
            {
                if (value == null)
                {
                    continue;
                }
                copy[size] = value;
                size++;
            }
            T[] result = new T[size];
            Array.Copy(copy, result, size);
            return result;
        }

        public static T[] Resize<T>(T[] values, int size)
        {
            T[] copy = new T[size];
            Array.Copy(values, copy, Math.Min(values.Length, size));
            return copy;
        }
    }
}
