/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.Utils
{
	public class igVariantMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong baseOffset = loader._stream.Tell64();
			igVariant data = new igVariant();
			loader._stream.Seek(baseOffset + 0x10);
			object? storageField = igObjectRefMetaField.GetMetaField().ReadIGZField(loader);
			if (storageField == null || !storageField.GetType().IsAssignableTo(typeof(igMetaField)))
			{
				Logging.Warn("Got igVariantMetaField that references _storageField that isn't an igMetaField. {0} @ byte 0x{1}", loader._dir._path, baseOffset.ToString("X08"));
			}
			else
			{
				data._storageField = (igMetaField)storageField;
				loader._stream.Seek(baseOffset);
				data._data = data._storageField.ReadIGZField(loader);
			}
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			ulong baseOffset = section._sh.Tell64();
			igVariant data = (igVariant)value!;
			section._sh.Seek(baseOffset + 0x10);
			igObjectRefMetaField.GetMetaField().WriteIGZField(saver, section, data._storageField);
			if (data._storageField != null)
			{
				section._sh.Seek(baseOffset);
				data._storageField.WriteIGZField(saver, section, data._data);
			}
			else
			{
				Logging.Warn("Writing out igVariantMetaField that references _storageField that is null, doing nothing...");
			}
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igVariant);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input) => false;
	}
}