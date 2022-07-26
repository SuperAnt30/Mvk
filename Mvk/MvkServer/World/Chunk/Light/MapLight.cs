//using MvkServer.Glm;
//using System.Collections;
//using System.Collections.Generic;

//namespace MvkServer.World.Chunk.Light
//{
//    /// <summary>
//    /// Карта блоков освещения
//    /// </summary>
//    public class MapLight
//    {
//        private struct Key
//        {
//            public vec3i pos;
//            public bool sky;
//            public Key(vec3i pos, bool sky)
//            {
//                this.pos = pos;
//                this.sky = sky;
//            }
//            public override bool Equals(object obj)
//            {
//                if (obj.GetType() == typeof(Key))
//                {
//                    var key = (Key)obj;
//                    return pos == key.pos && sky == key.sky;
//                }
//                return false;
//            }
//            public override int GetHashCode() => pos.GetHashCode() ^ sky.GetHashCode();
//        }

//        Dictionary<Key, LightStruct> pairs = new Dictionary<Key, LightStruct>();

//        /// <summary>
//        /// Вернуть коллекцию
//        /// </summary>
//        public ICollection Values { get { return pairs.Values; } }

//        /// <summary>
//        /// Добавить или изменить
//        /// </summary>
//        public void Set(LightStruct light)
//        {
//            Key key = new Key(light.Pos, light.Sky);
//            if (pairs.ContainsKey(key)) pairs[key] = light;
//            else pairs.Add(key, light);
//        }

//        /// <summary>
//        /// Получить значение по ключу
//        /// </summary>
//        public LightStruct Get(vec3i pos, bool sky)
//        {
//            Key key = new Key(pos, sky);
//            if (pairs.ContainsKey(key)) return pairs[key];
//            return new LightStruct();
//        }
//    }
//}
