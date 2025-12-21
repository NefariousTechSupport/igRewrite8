/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedQuadraticMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedQuadratic data = new igRangedQuadratic();
			data._data0 = loader._stream.ReadSingle();
			data._data1 = loader._stream.ReadSingle();
			data._data2 = loader._stream.ReadSingle();
			data._data3 = loader._stream.ReadSingle();
			data._data4 = loader._stream.ReadSingle();
			data._data5 = loader._stream.ReadSingle();
			data._data6 = loader._stream.ReadSingle();
			data._data7 = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igRangedQuadratic data = (igRangedQuadratic)value!;
			section._sh.WriteSingle(data._data0);
			section._sh.WriteSingle(data._data1);
			section._sh.WriteSingle(data._data2);
			section._sh.WriteSingle(data._data3);
			section._sh.WriteSingle(data._data4);
			section._sh.WriteSingle(data._data5);
			section._sh.WriteSingle(data._data6);
			section._sh.WriteSingle(data._data7);
		}
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			igRangedQuadratic quadratic = new igRangedQuadratic();
			string[] items = input.Split(',');
			if (items.Length != 8)
			{
				return false;
			}

			if (!float.TryParse(items[0], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data0)
			 || !float.TryParse(items[1], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data1)
			 || !float.TryParse(items[2], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data2)
			 || !float.TryParse(items[3], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data3)
			 || !float.TryParse(items[4], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data4)
			 || !float.TryParse(items[5], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data5)
			 || !float.TryParse(items[6], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data6)
			 || !float.TryParse(items[7], Localisation.kENNumberStyles, Localisation.kENCultureInfo, out quadratic._data7))
			{
				return false;
			}

			target = quadratic;

			return true;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igRangedQuadratic);
	}
}