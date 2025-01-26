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
	/// <summary>
	/// Defines the metafield representing an <c>igVector<c/>, basically this engine's version of std::vector
	/// </summary>
	public class igVectorMetaField : igRefMetaField
	{
		public igMetaField _memType;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaField? _memTypeRef;
		[Obsolete("This exists for the reflection system, do not use.")] public int _memTypeAlignment;
		[Obsolete("This exists for the reflection system, do not use.")] public igMetaObject? _elementType;
		public int _initialCapacity;		//Todo: Dump this from the game


		/// <summary>
		/// Set a template parameter for this field
		/// </summary>
		/// <param name="index">Which template parameter to set, must be 0</param>
		/// <param name="meta">The metafield to set it to</param>
		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			if(index != 0) throw new IndexOutOfRangeException("igVector only has 1 template argument");

			_memType = meta;
		}


		/// <summary>
		/// Set the number of template parameters
		/// </summary>
		/// <param name="count">The number of template parameters, must be 1</param>
		public override void SetTemplateParameterCount(uint count)
		{
			if(count != 1) throw new IndexOutOfRangeException("igVector can only has 1 template argument");
		}


		/// <summary>
		/// Get a template parameter
		/// </summary>
		/// <param name="index">Which template parameter to get, must be 0</param>
		/// <returns>The desired template parameter</returns>
		public override igMetaField GetTemplateParameter(uint index)
		{
			if(index != 0) throw new IndexOutOfRangeException("igVector only has 1 template argument");

			return _memType;
		}


		/// <summary>
		/// Get the number of template parameters
		/// </summary>
		/// <returns>The number of template parameters</returns>
		public override uint GetTemplateParameterCount() => 1;


		/// <summary>
		/// Reads a field from an IGZ file
		/// </summary>
		/// <param name="loader">the IGZ to read the data from, at the correct offset</param>
		/// <returns>The value of the field</returns>
		public override object? ReadIGZField(igIGZLoader loader)
		{
			uint count;
			if(loader._version == 0x09 && igAlchemyCore.isPlatform64Bit(loader._platform))
			{
				count = (uint)loader._stream.ReadUInt64();
			}
			else
			{
				count = loader._stream.ReadUInt32();
			}

			igVectorCommon vector = (igVectorCommon)Activator.CreateInstance(GetOutputType())!;

			IigMemory memory;

			if(count != 0)
			{
				igMemoryRefMetaField memoryRefMetaField = new igMemoryRefMetaField();
				memoryRefMetaField._memType = _memType;
				memory = (IigMemory)memoryRefMetaField.ReadIGZField(loader)!;
				vector.SetData(memory);
				vector.SetCount(count);
			}

			return vector;
		}


		/// <summary>
		/// Writes a field to an IGZ file
		/// </summary>
		/// <param name="saver">The IGZ to write the data to</param>
		/// <param name="section">The section of the igz to write the data to, at the correct offset</param>
		/// <param name="value">The value to write</param>
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVectorCommon vector = (igVectorCommon)value!;

			if(saver._version == 0x09 && igAlchemyCore.isPlatform64Bit(saver._platform))
			{
				section._sh.WriteUInt64(vector.GetCount());
			}
			else
			{
				section._sh.WriteUInt32(vector.GetCount());
			}

			igMemoryRefMetaField memoryRefMetaField = new igMemoryRefMetaField();
			memoryRefMetaField._memType = _memType;
			memoryRefMetaField.WriteIGZField(saver, section, vector.GetData());
		}


		/// <summary>
		/// The dotnet type of the field
		/// </summary>
		/// <returns>The type of the field</returns>
		public override Type GetOutputType()
		{
			return typeof(igVector<>).MakeGenericType(_memType.GetOutputType());
		}


		/// <summary>
		/// Constructs a default value for this field
		/// </summary>
		/// <param name="pool">The memory pool to construct the field for</param>
		/// <returns>The default value</returns>
		public override object? GetDefault(igMemoryPool pool)
		{
			igVectorCommon vector = (igVectorCommon)Activator.CreateInstance(GetOutputType())!;
			if(_initialCapacity > 0)
			{
				vector.SetCapacity(_initialCapacity);
			}
			IigMemory memory = vector.GetData();
			memory.SetMemoryPool(pool);
			vector.SetData(memory);
			return vector;
		}


		/// <summary>
		/// The size of the field (in bytes) for a given platform
		/// </summary>
		/// <param name="platform">The platform in question</param>
		/// <returns>An unsigned integer representing how big the field is in bytes</returns>
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 3;	//May cause issues for 64 bit platforms


		/// <summary>
		/// The alignment of the field (in bytes) for a given platform
		/// </summary>
		/// <param name="platform">The platfomr in question</param>
		/// <returns>An unsigned integer represnting the alignment of the field in bytes</returns>
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if (!_properties._implicitAlignment)
			{
				return _properties._requiredAlignment;
			}

			return base.GetAlignment(platform);
		}
	}
}