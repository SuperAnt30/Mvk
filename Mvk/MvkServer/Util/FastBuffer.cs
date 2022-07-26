using System;
using System.Runtime.CompilerServices;

namespace MvkServer.Util
{
    public class FastBuffer<T>
    {
        private T[] _Data;
        private int _Size;
        private int _Count;

        public int Count
            => _Count;

        public T this[int index]
        {
            get => _Data[index];
        }

        public FastBuffer(int size = 100)
        {
            _Size = size;
            _Data = new T[size];
        }

        //[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (_Size < _Count + 1)
            {
                _Size = (int)(_Size * 1.5f);
                Array.Resize(ref _Data, _Size);
            }

            _Data[_Count++] = item;
        }
        //[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void AddRange(T[] items)
        {
            int l = items.Length;
            if (_Size < _Count + l)
            {
                _Size = (int)(_Size + l + (_Size * 0.3f));
                Array.Resize(ref _Data, _Size);
            }
            for (int i = 0; i < l; i++)
                _Data[_Count + i] = items[i];

            _Count += l;
        }
        //[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public T[] CopyToArray()
        {
            T[] result = new T[_Count];
            Array.Copy(_Data, result, _Count);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
            => _Count = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int index)
        {
            return _Data[index];
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public T GetAndNull(int index)
        //{
        //    T item = _Data[index];
        //    _Data[index] = default;
        //    return item;
        //}
    }
}
