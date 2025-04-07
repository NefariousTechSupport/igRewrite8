/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary.Core
{
	public class igMemoryRefMetaField : igRefMetaField
	{
		[Obsolete("This exists for the reflection system, do not use.")] public int _memSize;
		public igMetaField _memType;
		[Obsolete("This exists for the reflection system, do not use.")] public int _memTypeAlignment = -1;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaField? _memTypeRef = null;
		public bool _releaseOnCopy = true;
		public bool _releaseOnReset = true;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveMetaField(sh, _memType);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_memType = loader.ReadMetaField(sh);
		}
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			_memType.DumpDefault(saver, sh);
		}
		public override void UndumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			_memType.UndumpDefault(saver, sh);
		}
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong start = loader._stream.Tell64();
			ulong flags = loader.ReadRawOffset();
			ulong raw = loader.ReadRawOffset();
			ulong end = loader._stream.Tell64();

			ulong offset = loader.DeserializeOffset(raw);
			Type memoryType;
			try
			{
				memoryType = GetOutputType();
			}
			catch(NotImplementedException)
			{
				return null;
			}

			igMemoryPool pool;
			IigMemory memory = (IigMemory)Activator.CreateInstance(memoryType)!;

			if(loader._runtimeFields._poolIds.BinarySearch(start) >= 0)
			{
				pool = loader._loadedPools[flags & 0xFFFFFF];
			}
			else
			{
				memory.SetFlags(flags, this, loader._platform);

				pool = loader.GetMemoryPoolFromSerializedOffset(raw);

				if(_memType.GetType() == typeof(igUnsignedCharMetaField))
				{
					loader._stream.Seek(offset);
					memory.SetData(loader._stream.ReadBytes(memory.GetCount()));
				}
				else
				{
					Array objects = memory.GetData();
					uint memSize = _memType.GetSize(loader._platform);

					for(int i = 0; i < objects.Length; i++)
					{
						loader._stream.Seek((long)offset + memSize * i);
						objects.SetValue(_memType.ReadIGZField(loader), i);
					}

					memory.SetData(objects);
				}
			}

			memory.SetMemoryPool(pool);

			return memory;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			IigMemory memory = (IigMemory)value;
			igIGZSaver.SaverSection memorySection = saver.GetSaverSection(memory.GetMemoryPool());
			Array objects = memory.GetData();
			ulong start = section._sh.Tell64();
			uint memSize = _memType.GetSize(saver._platform);
			ulong flags = memory.GetFlags(this, saver._platform);
			ulong size = flags & 0x07FFFFFF;

			if(objects != null)
			{
				ulong memOffset = memorySection.MallocAligned((uint)size, (ushort)memory.GetPlatformAlignment(this, saver._platform));
				memorySection.PushAlignment(memory.GetPlatformAlignment(this, saver._platform));
				//this thing is a hack
				bool oldRefCounted = true;
				if(_memType is igRefMetaField)
				{
					oldRefCounted = ((igRefMetaField)_memType)._refCounted;
					((igRefMetaField)_memType)._refCounted = _refCounted;
				}
				for(int i = 0; i < objects.Length; i++)
				{
					memorySection._sh.Seek((long)memOffset + memSize * i);
					_memType.WriteIGZField(saver, memorySection, objects.GetValue(i));
				}
				if(_memType is igRefMetaField)
				{
					((igRefMetaField)_memType)._refCounted = oldRefCounted;
				}

				section._sh.Seek(start);
				saver.WriteRawOffset(memory.GetFlags(this, saver._platform), section);
				saver.WriteRawOffset(saver.SerializeOffset((uint)memOffset, memorySection), section);
				section._runtimeFields._offsets.Add(start + igAlchemyCore.GetPointerSize(saver._platform));
			}
			else
			{
				section._sh.Seek(start);
				section._sh.WriteUInt32(memorySection._index);
				if(igAlchemyCore.isPlatform64Bit(saver._platform)) section._sh.WriteUInt32(0);
				saver.WriteRawOffset(0, section);
				section._runtimeFields._poolIds.Add(start);
			}

		}
		public override Type GetOutputType()
		{
			return typeof(igMemory<>).MakeGenericType(_memType.GetOutputType());
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 2;
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);

		public override igMetaField CreateFieldCopy()
		{
			igMemoryRefMetaField field = (igMemoryRefMetaField)base.CreateFieldCopy();
			field._memType = field._memType.CreateFieldCopy();
			return field;
		}
		public override object? GetDefault(igMemoryPool pool)
		{
			IigMemory mem = (IigMemory)Activator.CreateInstance(GetOutputType());
			mem.SetMemoryPool(pool);
			return mem;
		}
	}
}