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
	public class igVector<T> : igVectorCommon, IEnumerable<T>
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
	}
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