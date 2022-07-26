using System;

namespace MvkServer.Util
{
    /// <summary>
    /// Усовершенствованный лист от Мефистофель, работы без мусора, чтоб не пересоздавать
    /// Выделяем объём кеша, но увеличивать не может!!!
    /// </summary>
    public class ListMvk<T>
    {
        public int count;
        public T[] buffer;

        /// <summary>
        /// Создаём, с выделенным объёмом
        /// </summary>
        public ListMvk(int maxSize = 1000) => buffer = new T[maxSize];

        public T this[int index] => buffer[index];

        public void Add(T item) => buffer[count++] = item;

        public void AddNull(int count) => this.count += count;

        public void AddRange(T[] items)
        {
            int count = items.Length;
            for (int i = 0; i < count; i++)
            {
                buffer[this.count + i] = items[i];
            }
            this.count += count;
        }

        public void AddRange(ListMvk<T> items)
        {
            int count = items.count;
            for (int i = 0; i < count; i++)
            {
                buffer[this.count + i] = items[i];
            }
            this.count += count;
        }

        public T[] ToArray()
        {
            T[] result = new T[count];
            Array.Copy(buffer, result, count);
            return result;
        }

        public void Clear() => count = 0;

        //public void Sort() => Array.Sort(buffer);
    }
}
