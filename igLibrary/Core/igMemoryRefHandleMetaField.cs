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
	public class igMemoryRefHandleMetaField : igRefMetaField
	{
		public igMetaField _memType;

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
		public override object? ReadIGZField(igIGZLoader loader)
		{
			if(loader._runtimeFields._memoryHandles.BinarySearch(loader._stream.Tell()) < 0) return null;

			ulong thumbnailIndex = loader.ReadRawOffset();
			Tuple<ulong, ulong> thumbnail = loader._thumbnails[(int)thumbnailIndex];

			igMemoryPool pool = loader.GetMemoryPoolFromSerializedOffset(thumbnail.Item2);
			uint offset = (uint)loader.DeserializeOffset(thumbnail.Item2);
			Type memoryType;
			try
			{
				memoryType = GetOutputType();
			}
			catch(NotImplementedException)
			{
				return null;
			}
			IigMemory memory = (IigMemory)Activator.CreateInstance(memoryType);
			memory.SetMemoryPool(pool);
			memory.SetFlags(thumbnail.Item1, this, loader._platform);

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
			ulong memOffset = memorySection.Malloc((uint)size);

			memorySection.PushAlignment(memory.GetPlatformAlignment(this, saver._platform));

			for(int i = 0; i < objects.Length; i++)
			{
				memorySection._sh.Seek((long)memOffset + memSize * i);
				_memType.WriteIGZField(saver, memorySection, objects.GetValue(i));
			}

			section._sh.Seek(start);
			saver.WriteRawOffset((ulong)saver._thumbnails.Count, section);

			saver._thumbnails.Add(new Tuple<ulong, ulong>(memory.GetFlags(this, saver._platform), (memOffset | (memorySection._index << (saver._version >= 7 ? 0x1B : 0x18)))));
			section._runtimeFields._memoryHandles.Add(start);
		}

		public override Type GetOutputType()
		{
			return typeof(igMemory<>).MakeGenericType(_memType.GetOutputType());
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override igMetaField CreateFieldCopy()
		{
			igMemoryRefMetaField field = (igMemoryRefMetaField)base.CreateFieldCopy();
			field._memType = field._memType.CreateFieldCopy();
			return field;
		}
	}
}