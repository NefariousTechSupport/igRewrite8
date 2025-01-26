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
	public class igVec3dMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec3d data = new igVec3d();
			data._x = loader._stream.ReadDouble();
			data._y = loader._stream.ReadDouble();
			data._z = loader._stream.ReadDouble();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec3d data = (igVec3d)value;
			section._sh.WriteDouble(data._x);
			section._sh.WriteDouble(data._y);
			section._sh.WriteDouble(data._z);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x08;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18;
		public override Type GetOutputType() => typeof(igVec3d);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x18);
			igVec3d data = (igVec3d)_default;
			sh.WriteDouble(data._x);
			sh.WriteDouble(data._y);
			sh.WriteDouble(data._z);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec3d data = new igVec3d();
			data._x = sh.ReadDouble();
			data._y = sh.ReadDouble();
			data._z = sh.ReadDouble();
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
			string[] doubles = input.Split(',');
			if (doubles.Length != 3) return false;

			igVec3d tempTarget;

			if (!double.TryParse(doubles[0], out tempTarget._x)) return false;
			if (!double.TryParse(doubles[1], out tempTarget._y)) return false;
			if (!double.TryParse(doubles[2], out tempTarget._z)) return false;

			target = tempTarget;

			return true;
		}
	}
}