/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.DotNet
{
	public class DotNetTypeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong baseOffset = loader._stream.Tell64();
			DotNetType data = new DotNetType();
			object? baseMeta = igObjectRefMetaField.GetMetaField().ReadIGZField(loader);
			if (baseMeta != null && !baseMeta.GetType().IsAssignableTo(typeof(igBaseMeta)))
			{
				Logging.Warn("Got DotNetTypeMetaField that references _baseMeta that isn't an igBaseMeta. {0} @ byte 0x{1}", loader._dir._path, baseOffset.ToString("X08"));
			}
			else
			{
				data._baseMeta = (igBaseMeta?)baseMeta;
			}

			data._elementType = (ElementType)igIntMetaField._MetaField.ReadIGZField(loader)!;

			return data;
		}


		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			DotNetType data = (DotNetType)value!;
			igObjectRefMetaField.GetMetaField().WriteIGZField(saver, section, data._baseMeta);
			igIntMetaField._MetaField.WriteIGZField(saver, section, data._flags);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 8;
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform) * 2;
		public override Type GetOutputType() => typeof(DotNetType);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// This is incorrect
			DotNetType type = new DotNetType();
			if (!Enum.TryParse(typeof(ElementType), "kElementType" + input, out object? elementType))
			{
				return false;
			}
			type._elementType = (ElementType)elementType!;
			return true;
		}
	}
}