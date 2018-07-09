/*

Copyright (C) 2014-2018 by Vladimir Novick http://www.linkedin.com/in/vladimirnovick ,

    vlad.novick@gmail.com , http://www.sgcombo.com , https://github.com/Vladimir-Novick
	

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/
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

