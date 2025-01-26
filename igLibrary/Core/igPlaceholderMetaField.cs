/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igPlaceHolderMetaField : igMetaField
	{
		public igMetaFieldPlatformInfo _platformInfo;
		public short _num;
		public List<igMetaField> _templateArgs = new List<igMetaField>();

		public override int ArrayNum => _num;
		public override bool IsArray => _num > 0;

		public override void SetTemplateParameterCount(uint count)
		{
			if(_templateArgs.Count < count)
			{
				while(_templateArgs.Count < count)
				{
					_templateArgs.Add(null);
				}
			}
			else if(_templateArgs.Count > count)
			{
				while(_templateArgs.Count > count)
				{
					_templateArgs.RemoveAt(_templateArgs.Count - 1);
				}
			}
		}
		public override void SetTemplateParameter(uint index, igMetaField meta)
		{
			_templateArgs[(int)index] = meta;
		}
		public override uint GetTemplateParameterCount()
		{
			return (uint)_templateArgs.Count;
		}
		public override igMetaField GetTemplateParameter(uint index)
		{
			return _templateArgs[(int)index];
		}

		public override object? ReadIGZField(igIGZLoader loader)
		{
			Logging.Warn("Using placeholder implementation of ReadIGZField of field type {0}, this is bad", GetType().Name);
			return loader._stream.ReadBytes(GetSize(loader._platform));
		}

		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			Logging.Warn("Using placeholder implementation of WriteIGZField of field type {0}, this is bad", GetType().Name);
			section._sh.WriteBytes((byte[])value!);
		}

		public override uint GetAlignment(IG_CORE_PLATFORM platform) => _platformInfo._alignments[platform];
		public override uint GetSize(IG_CORE_PLATFORM platform) => _platformInfo._sizes[platform];

		public override Type GetOutputType() => typeof(byte[]);

		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			Logging.Warn("Tried parsing placeholder field {0} string when unimplemented, returning success...", _platformInfo._name);
			return true;
		}
	}
}