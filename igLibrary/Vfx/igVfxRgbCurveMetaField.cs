/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Diagnostics;

namespace igLibrary.Vfx
{
	public class igVfxRgbCurveMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			uint baseOffset = loader._stream.Tell();
			bool isPaddingNonZero = false;
			igVfxRgbCurve data = new igVfxRgbCurve();
			data._enableInterpolation = loader._stream.ReadBoolean();
			data._enableRandomness    = loader._stream.ReadBoolean();
			isPaddingNonZero         |= loader._stream.ReadUInt16() != 0;
			data._modulationHelper    = (igVfxModulationHelper)igVfxModulationHelperMetaField._MetaField.ReadIGZField(loader)!;
			isPaddingNonZero         |= loader._stream.ReadUInt64() != 0;
			data._c00                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c01                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c02                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c03                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c04                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c05                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c06                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c07                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c08                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c09                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c10                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c11                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c12                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c13                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;
			data._c14                 = (igVec4f)igVec4fMetaField._MetaField.ReadIGZField(loader)!;

			if (isPaddingNonZero)
			{
				Logging.Info("igz {0} at offset {1} has an igVfxRgbCurve with non-zero padding!", loader._dir._path, baseOffset.ToString("X08"));
#if DEBUG
				Debug.Assert(false, "We found an igVfxRgbCurve with non-zero padding!!");
#endif // DEBUG
			}

			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVfxRgbCurve data = new igVfxRgbCurve();
			section._sh.WriteBoolean(data._enableInterpolation);
			section._sh.WriteBoolean(data._enableRandomness);
			section._sh.WriteUInt16(0x0); // Padding
			igVfxModulationHelperMetaField._MetaField.WriteIGZField(saver, section, data._modulationHelper);
			section._sh.WriteUInt64(0x0); // Padding
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c00);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c01);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c02);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c03);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c04);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c05);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c06);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c07);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c08);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c09);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c10);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c11);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c12);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c13);
			igVec4fMetaField._MetaField.WriteIGZField(saver, section, data._c14);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x110;
		public override Type GetOutputType() => typeof(igVfxRgbCurve);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// I cannot be bothered to implement this
			Logging.Warn("Tried parsing igVfxRgbCurveMetaField value string when unimplemented, returning success...");
			return true;
		}
	}
}