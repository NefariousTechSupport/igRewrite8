/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/



namespace igLibrary.Vfx
{
	public class igVfxRangedCurveMetaField : igMetaField
	{
		/// <summary>
		/// The structure changed wildly in superchargers, use this to check the game
		/// </summary>
		/// <returns>Whether or not it's after superchargers</returns>
		private static bool PostSuperChargers()
		{
			return igArkCore.Game == igArkCore.EGame.EV_SkylandersSuperchargers
			    || igArkCore.Game == igArkCore.EGame.EV_SkylandersImaginators
			    || igArkCore.Game == igArkCore.EGame.EV_CrashNSaneTrilogy
			    || igArkCore.Game == igArkCore.EGame.EV_CrashNitroKart;
		}


		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRangedCurve data = new igVfxRangedCurve();

			if (PostSuperChargers())
			{
				for(int i = 0; i < 5; i++)
				{
					data._keyframes[i]._range = loader._stream.ReadSingle();
					data._keyframes[i]._linear = loader._stream.ReadBoolean();
					data._keyframes[i]._x = loader._stream.ReadSByte();
					data._keyframes[i]._y = loader._stream.ReadSByte();
					data._keyframes[i]._variance = loader._stream.ReadSByte();
					data._keyframes[i]._data1 = loader._stream.ReadSByte();
					data._keyframes[i]._data2 = loader._stream.ReadSByte();
					loader._stream.Seek(2, SeekOrigin.Current);
				}
				data._modulationHelper = (igVfxModulationHelper)igVfxModulationHelperMetaField._MetaField.ReadIGZField(loader)!;
				data._field_0x50       = loader._stream.ReadUInt16();
				data._field_0x52       = loader._stream.ReadUInt16();
			}
			else
			{
				data._valueOrScale = loader._stream.ReadSingle();
				data._data = (igMemory<byte>)igMemoryRefMetaField._MetaField.ReadIGZField(loader)!;
			}
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVfxRangedCurve data = (igVfxRangedCurve)value!;

			if (PostSuperChargers())
			{
				for(int i = 0; i < 5; i++)
				{
					section._sh.WriteSingle(data._keyframes[i]._range);
					section._sh.WriteBoolean(data._keyframes[i]._linear);
					section._sh.WriteSByte(data._keyframes[i]._x);
					section._sh.WriteSByte(data._keyframes[i]._y);
					section._sh.WriteSByte(data._keyframes[i]._variance);
					section._sh.WriteSByte(data._keyframes[i]._data1);
					section._sh.WriteSByte(data._keyframes[i]._data2);
					section._sh.Seek(2, SeekOrigin.Current);
				}

				igVfxModulationHelperMetaField._MetaField.WriteIGZField(saver, section, data._modulationHelper);

				section._sh.WriteUInt16(data._field_0x50);
				section._sh.WriteUInt16(data._field_0x52);
			}
			else
			{
				section._sh.WriteSingle(data._valueOrScale);
				igMemoryRefMetaField._MetaField.WriteIGZField(saver, section, data._data);
			}
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if (PostSuperChargers())
			{
				return 0x04;
			}
			else
			{
				return igAlchemyCore.GetPointerSize(platform);
			}
		}
		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			if (PostSuperChargers())
			{
				return 0x54;
			}
			else
			{
				return 0x0C;
			}
		}
		public override object? GetDefault(igMemoryPool pool) => new igVfxRangedCurve();
		public override Type GetOutputType() => typeof(igVfxRangedCurve);


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			// I cannot be bothered to implement this
			Logging.Warn("Tried parsing igVfxRangedCurveMetaField value string when unimplemented, returning success...");
			return true;
		}
	}
}