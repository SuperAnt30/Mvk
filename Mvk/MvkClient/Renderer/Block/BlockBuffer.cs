using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Блок буфера, с дистанцией до камеры, нужен для сортировки альфа блоков
    /// </summary>
    public struct BlockBuffer : IComparable
    {
        /// <summary>
        /// Буфер сетки VBO
        /// </summary>
        public byte[] buffer;
        /// <summary>
        /// Дистанция
        /// </summary>
        public float distance;

        public int CompareTo(object obj)
        {
            if (obj is BlockBuffer v) return distance.CompareTo(v.distance);
            else throw new Exception("Невозможно сравнить два объекта");
        }
    }
}
