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
	public class igVec4ucMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4uc data = new igVec4uc();
			data._r = loader._stream.ReadByte();
			data._g = loader._stream.ReadByte();
			data._b = loader._stream.ReadByte();
			data._a = loader._stream.ReadByte();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec4uc data = (igVec4uc)value;
			section._sh.WriteByte(data._r);
			section._sh.WriteByte(data._g);
			section._sh.WriteByte(data._b);
			section._sh.WriteByte(data._a);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x01;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x04;
		public override Type GetOutputType() => typeof(igVec4uc);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x10);
			igVec4uc data = (igVec4uc)_default;
			sh.WriteByte(data._r);
			sh.WriteByte(data._g);
			sh.WriteByte(data._b);
			sh.WriteByte(data._a);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec4uc data = new igVec4uc();
			data._r = sh.ReadByte();
			data._g = sh.ReadByte();
			data._b = sh.ReadByte();
			data._a = sh.ReadByte();
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
			if (bytes.Length != 4) return false;

			igVec4uc tempTarget;

			if (!byte.TryParse(bytes[0], out tempTarget._r)) return false;
			if (!byte.TryParse(bytes[1], out tempTarget._g)) return false;
			if (!byte.TryParse(bytes[2], out tempTarget._b)) return false;
			if (!byte.TryParse(bytes[3], out tempTarget._a)) return false;

			target = tempTarget;

			return true;
		}
	}
}