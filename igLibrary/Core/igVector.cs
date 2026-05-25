/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Collections;

namespace igLibrary.Core
{
	public class igVector<T> : igVectorCommon, IEnumerable<T>, IList<T>
	{
		public long _count;
		public igMemory<T> _data;

		public igVector()
		{
			_data = new igMemory<T>();
		}
		public T this[int index]
		{
			get => _data[index];
			set => _data[index] = value;
		}

		public int Count => (int)_count;
		public bool IsReadOnly => false;

		public void SetCount(uint count)
		{
			_count = count;
		}
		public uint GetCount() => (uint)_count;
		public void SetData(IigMemory data)
		{
			_data = (igMemory<T>)data;
		}
		public void SetCapacity(int capacity)
		{
			_data.Realloc(capacity);
			_count = (_count < capacity) ? _count : capacity;
		}
		public void Append(T data)
		{
			if(_count == _data.Length)
			{
				_data.Realloc((int)_count + 1);
			}
			_data[(int)_count] = data;
			_count++;
		}
		public void Clear()
		{
			SetCount(0);
			SetCapacity(0);
		}
		public IigMemory GetData() => _data;
		public IEnumerator<T> GetEnumerator()
		{
			for(int i = 0; i < _count; i++)
			{
				yield return _data[i];
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public object? GetItem(int index) => this[index];

		public void SetItem(int index, object? item) => this[index] = (T)item!;

		public int GetCapacity() => _data.Length;

		public int IndexOf(T item)
		{
			return Array.IndexOf(_data.Buffer, item, 0, (int)_count);
		}

		public void Insert(int index, T item)
		{
			if (_count == _data.Length)
			{
				SetCapacity(_data.Length + 4);
			}

			Array.Copy(_data.Buffer, index, _data.Buffer, index+1, _count - index);
			_data.Buffer[index] = item;
		}

		public void RemoveAt(int index)
		{
			Array.Copy(_data.Buffer, index+1, _data.Buffer, index, _count - index);
		}

		public void Add(T item)
		{
			Append(item);
		}

		public void AddRange(IEnumerable<T> collection)
		{
			int theCount = collection.Count();
			if ((_data.Length - _count) < theCount)
			{
				SetCapacity(_data.Length + theCount);
			}

			long destIndex = _count;
			foreach (T item in collection)
			{
				_data.Buffer[destIndex] = item;
			}
		}

		public bool Contains(T item)
		{
			for (int i = 0; i < _count; i++)
			{
				T? cur = _data[i];
				if ((cur != null && cur.Equals(item)) || (cur == null && item == null))
				{
					return true;
				}
			}

			return false;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(_data.Buffer, 0, array, arrayIndex, _count);
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);

			if (index >= 0)
			{
				RemoveAt(index);
			}

			return index >= 0;
		}	}
	public interface igVectorCommon
	{
		public uint GetCount();
		public void SetCount(uint count);
		public object? GetItem(int index);
		public void SetItem(int index, object? item);
		public int GetCapacity();
		public void SetCapacity(int capacity);
		public IigMemory GetData();
		public void SetData(IigMemory data);
	}
}