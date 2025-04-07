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
	public class igVec2fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec2f data = new igVec2f();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec2f data = (igVec2f)value;
			section._sh.WriteSingle(data._x);
			section._sh.WriteSingle(data._y);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x08;
		public override Type GetOutputType() => typeof(igVec2f);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			igVec2f data = (igVec2f)_default;
			sh.WriteSingle(data._x);
			sh.WriteSingle(data._y);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec2f data = new igVec2f();
			data._x = sh.ReadSingle();
			data._y = sh.ReadSingle();
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
			string[] floats = input.Split(',');
			if (floats.Length != 2) return false;

			igVec2f tempTarget;

			if (!float.TryParse(floats[0], out tempTarget._x)) return false;
			if (!float.TryParse(floats[1], out tempTarget._y)) return false;

			target = tempTarget;

			return true;
		}
	}
}