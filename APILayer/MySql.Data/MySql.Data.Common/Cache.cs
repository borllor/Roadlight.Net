using System;
using System.Collections.Generic;

namespace MySql.Data.Common
{
	internal class Cache<KeyType, ValueType>
	{
		private int _capacity;

		private Queue<KeyType> _keyQ;

		private Dictionary<KeyType, ValueType> _contents;

		public ValueType this[KeyType key]
		{
			get
			{
				ValueType result;
				if (this._contents.TryGetValue(key, out result))
				{
					return result;
				}
				return default(ValueType);
			}
			set
			{
				this.InternalAdd(key, value);
			}
		}

		public Cache(int initialCapacity, int capacity)
		{
			this._capacity = capacity;
			this._contents = new Dictionary<KeyType, ValueType>(initialCapacity);
			if (capacity > 0)
			{
				this._keyQ = new Queue<KeyType>(initialCapacity);
			}
		}

		public void Add(KeyType key, ValueType value)
		{
			this.InternalAdd(key, value);
		}

		private void InternalAdd(KeyType key, ValueType value)
		{
			if (!this._contents.ContainsKey(key) && this._capacity > 0)
			{
				this._keyQ.Enqueue(key);
				if (this._keyQ.Count > this._capacity)
				{
					this._contents.Remove(this._keyQ.Dequeue());
				}
			}
			this._contents[key] = value;
		}
	}
}
