using System.Collections;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект карта
    /// </summary>
    //public class Map
    //{
    //    protected Hashtable _ht = new Hashtable();

    //    /// <summary>
    //    /// Добавить или изменить
    //    /// </summary>
    //    public void Set(string key, object ob)
    //    {
    //        if (_ht.ContainsKey(key))
    //        {
    //            _ht[key] = ob;
    //        }
    //        else
    //        {
    //            _ht.Add(key, ob);
    //        }
    //    }

    //    /// <summary>
    //    /// Очистить
    //    /// </summary>
    //    public void Clear()
    //    {
    //        _ht.Clear();
    //    }

    //    /// <summary>
    //    /// Удалить
    //    /// </summary>
    //    public void Remove(string key)
    //    {
    //        if (_ht.ContainsKey(key))
    //        {
    //            _ht.Remove(key);
    //        }
    //    }

    //    /// <summary>
    //    /// Удалить
    //    /// </summary>
    //    public void Remove(int x, int y)
    //    {
    //        string key = Key(x, y);
    //        if (_ht.ContainsKey(key))
    //        {
    //            _ht.Remove(key);
    //        }
    //    }

    //    /// <summary>
    //    /// Получить количество
    //    /// </summary>
    //    public int Count { get { return _ht.Count; } }

    //    /// <summary>
    //    /// Проверить по наличию ключа
    //    /// </summary>
    //    public bool Contains(string key)
    //    {
    //        return _ht.ContainsKey(key);
    //    }

    //    /// <summary>
    //    /// Проверить по наличию ключа
    //    /// </summary>
    //    public bool Contains(int x, int y)
    //    {
    //        return _ht.ContainsKey(Key(x, y));
    //    }

    //    /// <summary>
    //    /// Получить первое значение по дистанции
    //    /// </summary>
    //    public object Get(string key)
    //    {
    //        if (Contains(key)) return _ht[key];
    //        return null;
    //    }

    //    /// <summary>
    //    /// Ключ для массива
    //    /// </summary>
    //    public static string Key(int x, int y)
    //    {
    //        return x.ToString() + ";" + y.ToString();
    //    }

    //    /// <summary>
    //    /// Вернуть коллекцию
    //    /// </summary>
    //    public virtual ICollection Values { get { return _ht.Values; } }

    //    /// <summary>
    //    /// Вернуть копию коллекции
    //    /// </summary>
    //    public Hashtable CloneHashtable() => _ht.Clone() as Hashtable;

    //}
}
