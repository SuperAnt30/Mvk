using System.Collections;
using System.Net.Sockets;


namespace MvkServer.Network
{
    /// <summary>
    /// Колекция объектов ReceivingBytes
    /// </summary>
    internal class ReceivingBytesCollection : CollectionBase, IList, ICollection, IEnumerable
    {
        public ReceivingBytes this[int index]
        {
            get { return (ReceivingBytes)List[index]; }
            set { List[index] = value; }
        }

        /// <summary>
        /// Добавить в колекцию объект и вернуть общее количество узлов
        /// </summary>
        public int Add(ReceivingBytes value) => List.Add(value);

        /// <summary>
        /// Добавить несколько объект в колекцию
        /// </summary>
        public void AddRange(ReceivingBytes[] sockets)
        {
            if (sockets.Length < 1) { return; }
            for (int i = 0; i < sockets.Length; i++)
            {
                Add(sockets[i]);
            }
        }

        /// <summary>
        /// Вставить объект в конкретное место
        /// </summary>
        public void Insert(int index, ReceivingBytes value) => List.Insert(index, value);

        /// <summary>
        /// Найти объект по сокету
        /// </summary>
        public ReceivingBytes Search(Socket value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].WorkSocket == value)
                {
                    return this[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Удалить конкретный объект 
        /// </summary>
        public void Remove(ReceivingBytes value) => List.Remove(value);

        /// <summary>
        /// Удалить объект зная рабочий сокет
        /// </summary>
        public void Remove(Socket value)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].WorkSocket == value)
                {
                    List.Remove(this[i]);
                    return;
                }
            }
        }
    }
}
