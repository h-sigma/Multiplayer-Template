using System;
using System.Collections;
using System.Collections.Generic;

public class CachedList<T> : ICollection<T>, IEnumerable<T> where T : new()
{
    private List<T> _list;
    private int     _validItemCount;

    public CachedList()
    {
        _validItemCount = 0;
        _list = new List<T>();
    }

    private void Swap(int a, int b)
    {
        var temp = _list[a];
        _list[a] = _list[b];
        _list[b] = temp;
    }

    #region ICollection

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0, n = _validItemCount; i < n; i++)
        {
            yield return _list[i];
        }

        yield break;
    }

    public void Add(T item)
    {
        if (HasCache)
        {
            _list[_validItemCount] = default;
            _list[_validItemCount] = item;
        }
        else
        {
            _list.Add(item);
        }
        _validItemCount++;
    }

    public void Clear()
    {
        _validItemCount = 0;
    }

    public bool Contains(T item)
    {
        var index = _list.IndexOf(item);
        return index != -1 && HasCache;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        
    }

    public bool Remove(T item)
    {
        var index = _list.IndexOf(item);
        if (index >= _validItemCount || index == -1) // index is cached (beyond valid list) or doesn't exist in list
        {
            return false;
        }

        Swap(index, _validItemCount - 1);
        _validItemCount--;

        return true;
    }

    public int CachedCapacity => _list.Count;
    public int  Count      => _validItemCount;
    public bool IsReadOnly => false;

    #endregion

    #region CachedList Interface

    public void ClearCache()
    {
        _list.RemoveRange(_validItemCount, _list.Count - _validItemCount);
    }

    public bool RemoveAt(int index)
    {
        if (index >= 0 && index < _validItemCount)
        {
            Swap(index, _validItemCount - 1);
            _validItemCount--;
            return true;
        }

        return false;
    }

    public bool HasCache => _validItemCount < _list.Count;

    public T GetCachedOrAdd()
    {
        if (HasCache)
        {
            var temp = _list[_validItemCount - 1];
            _validItemCount++;
            return temp;
        }
        else
        {
            var temp = new T();
            Add(temp);
            return temp;
        }
    }
    
    public void RemoveAll(Func<T, bool> func)
    {
        for (int i = _validItemCount - 1; i >= 0; i--)
        {
            if (func(_list[i]))
            {
                RemoveAt(i);
            }
        }
    }

    public T this[int index] => _list[index];

    public bool TryAddCached(out T result)
    {
        if (HasCache)
        {
            result = _list[_validItemCount - 1];
            _validItemCount++;
            return true;
        }

        result = default(T);
        return false;
    }

    #endregion
}