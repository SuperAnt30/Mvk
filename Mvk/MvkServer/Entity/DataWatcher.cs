using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Network;
using MvkServer.Util;
using System;
using System.Collections;

namespace MvkServer.Entity
{
    /// <summary>
    /// Дополнительный объект данных сущности
    /// </summary>
    public class DataWatcher
    {
        /// <summary>
        /// Было ли изменение (HasObjectChanged)
        /// </summary>
        public bool IsChanged { get; private set; } = false;
        /// <summary>
        /// Если true то не обрабатываются данные
        /// </summary>
        public bool IsBlank { get; private set; } = true;

        /// <summary>
        /// Сущность чьи эти данные
        /// </summary>
        private readonly EntityBase entity;
        /// <summary>
        /// Массив данных
        /// </summary>
        private Hashtable watched = new Hashtable();

        public DataWatcher(EntityBase entity) => this.entity = entity;

        /// <summary>
        /// По типу данных получить id
        /// </summary>
        private int GetDataTypes(Type type)
        {
            if (type == typeof(byte)) return 0;
            if (type == typeof(short)) return 1;
            if (type == typeof(int)) return 2;
            if (type == typeof(float)) return 3;
            if (type == typeof(string)) return 4;
            if (type == typeof(ItemStack)) return 5;
            if (type == typeof(BlockPos)) return 6;
            if (type == typeof(vec3)) return 7;

            throw new Exception("Неопределённый тип данных");
        }

        /// <summary>
        /// Отменить изменение
        /// </summary>
        public void NotChanged() => IsChanged = false;

        /// <summary>
        /// Добавить параметр данных
        /// </summary>
        /// <param name="id">индекс 0 - 31</param>
        /// <param name="obj">объект с данными</param>
        public void Add(int id, object obj)
        {
            if (obj == null) throw new Exception("Неизвестный тип данных");
            else if (id > 31) throw new Exception("Идентификатор значения данных " + id + " слишком велик (Max 31)");
            else if (watched.ContainsKey(id)) throw new Exception("Повторяющееся значение идентификатора для " + id);
            else
            {
                watched.Add(id, new WatchableObject(GetDataTypes(obj.GetType()), id, obj));
                IsBlank = false;
            }
        }

        /// <summary>
        /// Добавьте новый объект для отслеживания DataWatcher, используя указанный тип данных
        /// </summary>
        /// <param name="id">индекс 0 - 31</param>
        /// <param name="type">индекс типа данных 0 - 7</param>
        public void AddByDataType(int id, int type)
        {
            watched.Add(id, new WatchableObject(type, id, null));
            IsBlank = false;
        }

        #region GetWatchableObject

        /// <summary>
        /// Получить байтовое значение по индексу
        /// </summary>
        public byte GetWatchableObjectByte(int id) => (byte)GetWatchedObject(id).GetObject();
        /// <summary>
        /// Получить 16-разрядное значение по индексу
        /// </summary>
        public short GetWatchableObjectShort(int id) => (short)GetWatchedObject(id).GetObject();
        /// <summary>
        /// Получить 32-разрядное значение по индексу
        /// </summary>
        public int GetWatchableObjectInt(int id) => (int)GetWatchedObject(id).GetObject();
        /// <summary>
        /// Получить число с плавающей запятой по индексу
        /// </summary>
        public float GetWatchableObjectFloat(int id) => (float)GetWatchedObject(id).GetObject();
        /// <summary>
        /// Получить строковое значение по индексу
        /// </summary>
        public string GetWatchableObjectString(int id) => (string)GetWatchedObject(id).GetObject();
        /// <summary>
        /// Получить объект ItemStack по индексу
        /// </summary>
        public ItemStack GetWatchableObjectItemStack(int id) => GetWatchedObject(id).GetObject() as ItemStack;
        /// <summary>
        /// Получить объект BlockPos по индексу
        /// </summary>
        public BlockPos GetWatchableObjectBlockPos(int id) => GetWatchedObject(id).GetObject() as BlockPos;
        /// <summary>
        /// Получить трёхмерный вектор с плавающей запятой по индексу
        /// </summary>
        public vec3 GetWatchableObjectVec3(int id) => (vec3)GetWatchedObject(id).GetObject();

        /// <summary>
        /// Получить объект по индексу, без проверок
        /// </summary>
        private WatchableObject GetWatchedObject(int id) => (WatchableObject)watched[id];

        #endregion

        /// <summary>
        /// Обновляет уже существующий объект
        /// </summary>
        public void UpdateObject(int id, object obj)
        {
            WatchableObject watchableObject = GetWatchedObject(id);

            if (!obj.Equals(watchableObject.GetObject()))
            {
                watchableObject.SetObject(obj);
                entity.UpdatedWatchedObjec(id);
                watchableObject.SetWatched(true);
                watched[id] = watchableObject;
                IsChanged = true;
            }
        }

        /// <summary>
        /// Задать значение что было изменено
        /// </summary>
        public void SetObjectWatched(int id)
        {
            WatchableObject watchableObject = GetWatchedObject(id);
            watchableObject.SetWatched(true);
            watched[id] = watchableObject;
            IsChanged = true;
        }

        /// <summary>
        /// Получить массив всех метданных которые были изменены
        /// </summary>
        public ArrayList GetChanged()
        {
            ArrayList watchables = new ArrayList();
            if (IsChanged)
            {
                foreach(WatchableObject watchableObject in watched.Values)
                {
                    if (watchableObject.IsWatched())
                    {
                        watchableObject.SetWatched(false);
                        watchables.Add(watchableObject);
                    }
                }
                IsChanged = false;
            }
            return watchables;
        }

        /// <summary>
        /// Получить массив всех метданных
        /// </summary>
        public ArrayList GetAllWatched()
        {
            ArrayList watchables = new ArrayList();
            foreach (WatchableObject watchableObject in watched.Values)
            {
                if (watchableObject.IsWatched())
                {
                    watchableObject.SetWatched(false);
                    watchables.Add(watchableObject);
                }
            }
            return watchables;
        }

        /// <summary>
        /// Передать пакеты
        /// </summary>
        public void WriteTo(StreamBase stream)
        {
            foreach (WatchableObject watchableObject in watched.Values)
            {
                WriteWatchableObjectToPacketBuffer(stream, watchableObject);
            }
            stream.WriteByte(127);
        }

        /// <summary>
        /// Записывает наблюдаемый объект (атрибут объекта типа 
        /// {byte, short, int, float, string, ItemStack, BlockPos, vec3}) в указанный StreamBase
        /// </summary>
        private static void WriteWatchableObjectToPacketBuffer(StreamBase stream, WatchableObject watchableObject)
        {
            int key = (watchableObject.GetObjectType() << 5 | watchableObject.GetDataValueId() & 31) & 255;
            stream.WriteByte((byte)key);

            switch (watchableObject.GetObjectType())
            {
                case 0: stream.WriteByte((byte)watchableObject.GetObject()); break;
                case 1: stream.WriteShort((short)watchableObject.GetObject()); break;
                case 2: stream.WriteInt((int)watchableObject.GetObject()); break;
                case 3: stream.WriteFloat((float)watchableObject.GetObject()); break;
                case 4: stream.WriteString((string)watchableObject.GetObject()); break;
                case 5: ItemStack.WriteStream(watchableObject.GetObject() as ItemStack, stream); break;
                case 6:
                    BlockPos blockPos = watchableObject.GetObject() as BlockPos;
                    stream.WriteInt(blockPos.X);
                    stream.WriteInt(blockPos.Y);
                    stream.WriteInt(blockPos.Z);
                    break;
                case 7:
                    vec3 vec = (vec3)watchableObject.GetObject();
                    stream.WriteFloat(vec.x);
                    stream.WriteFloat(vec.y);
                    stream.WriteFloat(vec.z);
                    break;
            }
        }

        /// <summary>
        /// Читает список отслеживаемых объектов (атрибут объекта типа 
        /// {byte, short, int, float, string, ItemStack, BlockPos, vec3}) из предоставленного StreamBase
        /// </summary>
        public static ArrayList ReadWatchedListFromPacketBuffer(StreamBase stream)
        {
            ArrayList watchables = new ArrayList();
            for (byte key = stream.ReadByte(); key != 127; key = stream.ReadByte())
            {

                int type = (key & 224) >> 5;
                int id = key & 31;
                switch (type)
                {
                    case 0: watchables.Add(new WatchableObject(type, id, stream.ReadByte())); break;
                    case 1: watchables.Add(new WatchableObject(type, id, stream.ReadShort())); break;
                    case 2: watchables.Add(new WatchableObject(type, id, stream.ReadInt())); break;
                    case 3: watchables.Add(new WatchableObject(type, id, stream.ReadFloat())); break;
                    case 4: watchables.Add(new WatchableObject(type, id, stream.ReadString())); break;
                    case 5: watchables.Add(new WatchableObject(type, id, ItemStack.ReadStream(stream))); break;
                    case 6:
                        watchables.Add(new WatchableObject(type, id, 
                            new BlockPos(stream.ReadInt(), stream.ReadInt(), stream.ReadInt())));
                        break;
                    case 7:
                        watchables.Add(new WatchableObject(type, id, 
                            new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat())));
                        break;
                }
            }
            return watchables;
        }

        /// <summary>
        /// Записывает список наблюдаемых объектов (атрибут объекта типа 
        /// {byte, short, int, float, string, ItemStack, BlockPos, vec3}) из предоставленного StreamBase
        /// </summary>
        public static void WriteWatchedListToPacketBuffer(ArrayList objectsList, StreamBase stream)
        {
            if (objectsList != null && objectsList.Count > 0)
            {
                foreach (WatchableObject watchableObject in objectsList)
                {
                    WriteWatchableObjectToPacketBuffer(stream, watchableObject);
                }
            }
            stream.WriteByte(127);
        }

        /// <summary>
        /// Обновить данные со списка
        /// </summary>
        public void UpdateWatchedObjectsFromList(ArrayList watchables)
        {
            foreach (WatchableObject watchableObject in watchables)
            {
                int id = watchableObject.GetDataValueId();
                if (watched.ContainsKey(id))
                {
                    WatchableObject wo = GetWatchedObject(id);
                    wo.SetObject(watchableObject.GetObject());
                    watched[id] = watchableObject;
                    entity.UpdatedWatchedObjec(id);
                }
            }
            IsChanged = true;
        }

        /// <summary>
        /// Приватная структура данных
        /// </summary>
        private struct WatchableObject
        {
            private readonly int objectType;
            private readonly int dataValueId;
            private object watchedObject;
            private bool watched;

            public WatchableObject(int type, int id, object obj)
            {
                dataValueId = id;
                watchedObject = obj;
                objectType = type;
                watched = true;
            }

            public int GetDataValueId() => dataValueId;
            public void SetObject(object obj) => watchedObject = obj;
            public object GetObject() => watchedObject;
            public int GetObjectType() => objectType;
            public bool IsWatched() => watched;
            public void SetWatched(bool watched) => this.watched = watched;
        }
    }
}
