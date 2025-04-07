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
	public class igVec4iMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4i data = new igVec4i();
			data._x = loader._stream.ReadInt32();
			data._y = loader._stream.ReadInt32();
			data._z = loader._stream.ReadInt32();
			data._w = loader._stream.ReadInt32();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec4i data = (igVec4i)value;
			section._sh.WriteInt32(data._x);
			section._sh.WriteInt32(data._y);
			section._sh.WriteInt32(data._z);
			section._sh.WriteInt32(data._w);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x10;
		public override Type GetOutputType() => typeof(igVec4i);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x10);
			igVec4i data = (igVec4i)_default;
			sh.WriteInt32(data._x);
			sh.WriteInt32(data._y);
			sh.WriteInt32(data._z);
			sh.WriteInt32(data._w);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec4i data = new igVec4i();
			data._x = sh.ReadInt32();
			data._y = sh.ReadInt32();
			data._z = sh.ReadInt32();
			data._w = sh.ReadInt32();
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
			string[] ints = input.Split(',');
			if (ints.Length != 4) return false;

			igVec4i tempTarget;

			if (!int.TryParse(ints[0], out tempTarget._x)) return false;
			if (!int.TryParse(ints[1], out tempTarget._y)) return false;
			if (!int.TryParse(ints[2], out tempTarget._z)) return false;
			if (!int.TryParse(ints[3], out tempTarget._w)) return false;

			target = tempTarget;

			return true;
		}
	}
}