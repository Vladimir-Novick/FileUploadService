////////////////////////////////////////////////////////////////////////////
//	Copyright 2014 : Vladimir Novick    https://www.linkedin.com/in/vladimirnovick/  
//
//    NO WARRANTIES ARE EXTENDED. USE AT YOUR OWN RISK. 
//
//      Available under the BSD and MIT licenses
//
// To contact the author with suggestions or comments, use  :vlad.novick@gmail.com
//
////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SGCombo.Extensions.Utilites
{
    public class SafeDict<T> : IEnumerable<T>
	{
		ConcurrentDictionary<string, T> mList = new ConcurrentDictionary<string, T>();

		public SafeDict()
		{
		}

		public void Clear()
		{
			mList.Clear();
		}

		public T Get(string Key)
		{
            if (!Contains(Key)) return default(T);

			return mList[Key];
		}

		public int Count
		{
			get 
			{
				if (mList == null) return 0;

                try
                {
                    return mList.Count;
                }
                catch
                {
                    return 0;
                }
			}
		}

        public bool Empty
        {
            get
            {
                return (Count < 1);
            }
        }

		public void Add(T item)
		{
			Add(item.GetHashCode().ToString(), item);
		}

        public void Add(string Key, T item)
        {
            if (Contains(Key))
            {
                Utils.Error("TryAdd: 1-" + Key);
            }

            if (!mList.TryAdd(Key, item))
            {
                Utils.Error("TryAdd: 2-" + Key);
            }
        }

		public bool Addb(string Key, T item)
		{
            if (Contains(Key)) return false;
            if (!mList.TryAdd(Key, item)) return false;

            return true;
		}

		public bool Contains(string Key)        
        {
            if (Key == null) return false;
			return mList.ContainsKey(Key);
		}

		public void Remove(string Key)
		{
			if (!Contains(Key)) return;

			T dummy = default(T);

            mList.TryRemove(Key, out dummy);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return mList.Values.GetEnumerator();
		}
	}

	public class SafeList<T> : IEnumerable<T>
	{
		ConcurrentQueue<T> mList = new ConcurrentQueue<T>();

		public SafeList() { }

		public void Clear()
		{
			mList = new ConcurrentQueue<T>();
		}

		public int Count
		{
			get { return mList.Count; }
		}

		public void Add(T item)
		{
			mList.Enqueue(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return mList.GetEnumerator();
		}
	}
}

