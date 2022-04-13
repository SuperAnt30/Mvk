using MvkServer.Glm;
using MvkServer.World.Block;
using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Блок буфера, с дистанцией до камеры, нужен для сортировки альфа блоков
    /// </summary>
    public struct BlockBuffer : IComparable
    {
        private readonly EnumBlock eBlock;
        private readonly vec3i pos;
        private readonly byte[] buffer;
        private float distance;
        private readonly bool body;

        public BlockBuffer(EnumBlock eBlock, vec3i pos, byte[] buffer, float distance)
        {
            this.eBlock = eBlock;
            this.pos = pos;
            this.buffer = buffer;
            this.distance = distance;
            body = true;
        }

        /// <summary>
        /// Получить новый объект с новой дистанцией
        /// </summary>
        //public BlockBuffer NewDistance(float distance) => new BlockBuffer(eBlock, pos, buffer, distance);

        /// <summary>
        /// Тип блока
        /// </summary>
        public EnumBlock EBlock() => eBlock;
        /// <summary>
        /// Позиция блока в этом чанке, скорее всего 0..15
        /// </summary>
        public vec3i Position() => pos;
        /// <summary>
        /// Буфер сетки VBO
        /// </summary>
        public byte[] Buffer() => buffer;
        /// <summary>
        /// Дистанция
        /// </summary>
        public float Distance() => distance;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !body;

        public int CompareTo(object obj)
        {
            if (obj is BlockBuffer v) return distance.CompareTo(v.Distance());
            else throw new Exception("Невозможно сравнить два объекта");
        }

        public override string ToString() => string.Format("{0} ({1}) {2:0.00}", eBlock, pos, distance);
    }
}
