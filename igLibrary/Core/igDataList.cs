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
	public class igTDataList<T> : igContainer, IList<T>, IigDataList
	{
		//TODO: Modify the reflection system so we can put proper access modifiers on these
		public int _count;
		public int _capacity;
		public igMemory<T> _data = new igMemory<T>();

		public int Count => throw new NotImplementedException();

		public bool IsReadOnly => throw new NotImplementedException();

		public T this[int index]
		{
			get => _data[index];
			set => _data[index] = value;
		}

		public T Append(T item)
		{
			if(_count >= _capacity)
			{
				SetCapacity(_capacity + 4);
			}

			_data[_count] = item;
			_count++;
			return item;
		}
		public void Remove(int index)
		{
			if(index >= _count) throw new IndexOutOfRangeException($"Index {index} is out of bounds");
			if(index < 0) throw new IndexOutOfRangeException($"Index {index} is below zero");

			for(int i = index; i < _count-1; i++)
			{
				_data[i] = _data[i+1];
			}
			_count--;
			_data[_count] = default!;
		}

		public int GetCapacity() => _capacity;

		public int GetCount() => _count;

		public IigMemory GetData() => _data;
		public void SetData(IigMemory data) => _data = (igMemory<T>)data;

		public Type GetMemoryType()
		{
			return _data.GetType().GenericTypeArguments[0];
		}

		public object GetObject(int index)
		{
			return this[index];
		}

		public void SetCapacity(int capacity)
		{
			_data.Realloc(capacity);
			_capacity = capacity;
		}

		public void SetCount(int count)
		{
			_count = count;
			SetCapacity(((count + 3) / 4) * 4);
		}

		public void SetObject(int index, object data)
		{
			_data[index] = (T)data;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for(int i = 0; i < _count; i++)
			{
				yield return _data[i];
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int IndexOf(T value)
		{
			for (int i = 0; i < _count; i++)
			{
				if (Equals(_data[i], value))
				{
					return i;
				}
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			if (_capacity <= _count)
			{
				SetCapacity(_capacity + 1);
			}

			// Shift things back
			for (int i = _capacity - 1; i >= index+1; i--)
			{
				_data[i] = _data[i-1];
			}

			_data[index] = item;
		}

		public void RemoveAt(int index)
		{
			// Shift things forward
			for (int i = _capacity - 2; i >= index; i--)
			{
				_data[i] = _data[i+1];
			}
		}

		public void Add(T item) => Append(item);

		public void Clear()
		{
			_data.Dealloc();
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(_data.Buffer, arrayIndex, array, 0, _count - arrayIndex);
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
			}
			return index >= 0;
		}
	}

	public interface IigDataList
	{
		public int GetCount();
		public void SetCount(int count);
		public int GetCapacity();
		public void SetCapacity(int capacity);
		public Type GetMemoryType();
		public IigMemory GetData();
		public void SetData(IigMemory data);
		public object GetObject(int index);
		public void SetObject(int index, object data);
	}

	public class igDataList : igTDataList<byte> {}
}