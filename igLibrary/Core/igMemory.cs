/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Collections;
using System.Numerics;

namespace igLibrary.Core
{
	public struct igMemory<T> : IEnumerable<T>, IigMemory
	{
		public igMemoryPool _memoryPool;
		private T[]? _data;

		public T this[uint index]
		{
			get => _data[index];
			set => _data[index] = value;
		}
		public T this[int index]
		{
			get => _data[index];
			set => _data[index] = value;
		}
		public int Length => _data == null ? 0 : _data.Length;
		public T[] Buffer => _data;
		public bool _implicitMemoryPool;
		public bool _optimalCPUReadWrite;
		public bool _optimalGPURead;
		public uint _alignmentMultiple;

		public igMemory()
		{
			_memoryPool = igMemoryContext.Singleton.GetMemoryPoolByName("Default");
 			_data = null;
			_implicitMemoryPool = true;
			_optimalCPUReadWrite = true;
			_optimalGPURead = false;
			_alignmentMultiple = 1;
 		}
		public igMemory(igMemoryPool pool, uint size)
		{
			_memoryPool = pool;
			_data = new T[size];
			_implicitMemoryPool = true;
			_optimalCPUReadWrite = true;
			_optimalGPURead = false;
			_alignmentMultiple = 1;
		}
		public igMemory<T> CreateCopy()
		{
			igMemory<T> copy = (igMemory<T>)this.MemberwiseClone();
			copy._data = _data == null ? null : _data.ToArray();
			return copy;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if(_data == null) return Enumerable.Empty<T>().GetEnumerator();
			else return ((IEnumerable<T>)this._data).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if(_data == null) return Enumerable.Empty<T>().GetEnumerator();
			else return ((IEnumerable<T>)this._data).GetEnumerator();
		}
		igMemoryPool IigMemory.GetMemoryPool() => _memoryPool;
		void IigMemory.SetMemoryPool(igMemoryPool pool) => _memoryPool = pool;
		Array IigMemory.GetData() => _data;
		uint IigMemory.GetCount() => (uint)Length;
		void IigMemory.SetData(Array data)
		{
			if(data.GetType().GetElementType().IsAssignableTo(typeof(T)))
			{
				_data = data.Cast<T>().ToArray();
			}
		}
		object? IigMemory.GetItem(int i) => this[i];
		void IigMemory.SetItem(int i, object? obj) => this[i] = (T)obj;
		public void Alloc(int itemCount)
		{
			_data = new T[itemCount];
		}
		public void Realloc(int itemCount)
		{
			if(_data != null && itemCount == _data!.Length) return;
			Array.Resize<T>(ref _data, itemCount);
		}
		public ulong GetFlags(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform) => GetFlagsInternal(ioField._memType, platform);
		public ulong GetFlags(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform) => GetFlagsInternal(ioField._memType, platform);
		private ulong GetFlagsInternal(igMetaField memType, IG_CORE_PLATFORM platform)
		{
			ulong flags = (uint)(_data == null ? 0 : _data.Length) * memType.GetSize(platform);
			uint codedAlignment = memType.GetAlignment(platform) * _alignmentMultiple;
			
			if(codedAlignment < 4) codedAlignment = 4;

			for(int i = 0; i < 32; i++)
			{
				if((1u << i) == codedAlignment)
				{
					codedAlignment = (uint)i;
					break;
				}
			}


			codedAlignment = (codedAlignment - 2u);
			//The following isn't valid on crash nst/ctrnf
			if(igAlchemyCore.isPlatform64Bit(platform))
			{
				flags |= codedAlignment << 0x3B;
				flags |= (_optimalCPUReadWrite ? 1u : 0u) << 0x3F;
			}
			else
			{
				flags |= codedAlignment << 0x1B;
				flags |= (_optimalCPUReadWrite ? 1u : 0u) << 0x1F;
			}
			return flags;
		}
		public uint GetPlatformAlignment(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform) => GetPlatformAlignmentInternal(ioField._memType, platform);
		public uint GetPlatformAlignment(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform) => GetPlatformAlignmentInternal(ioField._memType, platform);
		private uint GetPlatformAlignmentInternal(igMetaField memType, IG_CORE_PLATFORM platform) => memType.GetAlignment(platform) * _alignmentMultiple;
		public void SetFlags(ulong flags, igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform) => SetFlagsInternal(flags, ioField._memType, platform);
		public void SetFlags(ulong flags, igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform) => SetFlagsInternal(flags, ioField._memType, platform);
		private void SetFlagsInternal(ulong flags, igMetaField memType, IG_CORE_PLATFORM platform)
		{
			uint alignment = 0;
			ulong size = 0;
			if(igAlchemyCore.isPlatform64Bit(platform))
			{
				alignment = 1u << (int)(((flags >> 0x3B) & 0xF) + 2);
				_optimalCPUReadWrite = (flags >> 0x3F) != 0;
				size = flags & 0x07FFFFFFFFFFFFFF;
			}
			else
			{
				alignment = 1u << (int)(((flags >> 0x1B) & 0xF) + 2);
				_optimalCPUReadWrite = (flags >> 0x1F) != 0;
				size = flags & 0x07FFFFFF;
			}
			_alignmentMultiple = alignment / memType.GetAlignment(platform);
			_data = new T[size / memType.GetSize(platform)];
		}
	}
	public interface IigMemory
	{
		public int Length { get; }
		igMemoryPool GetMemoryPool();
		void SetMemoryPool(igMemoryPool pool);
		Array GetData();
		uint GetCount();
		void SetData(Array data);
		object? GetItem(int i);
		void SetItem(int i, object? obj);
		ulong GetFlags(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		ulong GetFlags(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
		uint GetPlatformAlignment(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		uint GetPlatformAlignment(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
		void SetFlags(ulong flags, igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		void SetFlags(ulong flags, igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
		void Alloc(int itemCount);
		void Realloc(int itemCount);
	}
}