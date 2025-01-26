/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec2ucMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec2uc data = new igVec2uc();
			data._x = loader._stream.ReadByte();
			data._y = loader._stream.ReadByte();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec2uc data = (igVec2uc)value;
			section._sh.WriteByte(data._x);
			section._sh.WriteByte(data._y);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x01;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x02;
		public override Type GetOutputType() => typeof(igVec2uc);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(2);
			igVec2uc data = (igVec2uc)_default;
			sh.WriteByte(data._x);
			sh.WriteByte(data._y);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec2uc data = new igVec2uc();
			data._x = sh.ReadByte();
			data._y = sh.ReadByte();
			_default = data;
		}


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			string[] bytes = input.Split(',');
			if (bytes.Length != 2) return false;

			igVec2uc tempTarget;

			if (!byte.TryParse(bytes[0], out tempTarget._x)) return false;
			if (!byte.TryParse(bytes[1], out tempTarget._y)) return false;

			target = tempTarget;

			return true;
		}
	}
}