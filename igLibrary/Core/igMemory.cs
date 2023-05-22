using System.Collections;
using System.Numerics;

namespace igLibrary.Core
{
	public struct igMemory<T> : IEnumerable<T>, IigMemory
	{
		public igMemoryPool _memoryPool;
		private T[] _data;

		public T this[int index]
		{
			get => _data[index];
			set => _data[index] = value;
		}
		public int Length => _data.Length;
		public bool _implicitMemoryPool;
		public bool _optimalCPUReadWrite;
		public bool _optimalGPURead;
		public byte _alignmentMultiple;

		public igMemory()
		{
			_memoryPool = igMemoryContext.Singleton.GetMemoryPoolByName("Default");
 			_data = null;
			_implicitMemoryPool = true;
			_optimalCPUReadWrite = false;
			_optimalGPURead = false;
			_alignmentMultiple = 1;
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
		void IigMemory.SetData(Array data)
		{
			if(data.GetType().GetElementType().IsAssignableTo(typeof(T)))
			{
				_data = data.Cast<T>().ToArray();
			}
		}
		public void Realloc(int itemCount)
		{
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
			_alignmentMultiple = (byte)(alignment / memType.GetAlignment(platform));
			_data = new T[size / memType.GetSize(platform)];
		}
	}
	public interface IigMemory
	{
		igMemoryPool GetMemoryPool();
		void SetMemoryPool(igMemoryPool pool);
		Array GetData();
		void SetData(Array data);
		ulong GetFlags(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		ulong GetFlags(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
		uint GetPlatformAlignment(igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		uint GetPlatformAlignment(igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
		void SetFlags(ulong flags, igMemoryRefMetaField ioField, IG_CORE_PLATFORM platform);
		void SetFlags(ulong flags, igMemoryRefHandleMetaField ioField, IG_CORE_PLATFORM platform);
	}
}