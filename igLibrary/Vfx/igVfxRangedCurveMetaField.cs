using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igVfxRangedCurveMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRangedCurve data = new igVfxRangedCurve();
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
			data._field_0x3C = loader._stream.ReadSingle();
			data._field_0x40 = loader._stream.ReadSingle();
			data._field_0x44 = loader._stream.ReadSingle();
			data._field_0x48 = loader._stream.ReadSingle();
			data._field_0x4C = loader._stream.ReadUInt16();
			data._field_0x4E = loader._stream.ReadBoolean();
			data._flags = loader._stream.ReadByte();
			data._field_0x50 = loader._stream.ReadUInt16();
			data._field_0x52 = loader._stream.ReadUInt16();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVfxRangedCurve data = (igVfxRangedCurve)value;
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
			section._sh.WriteSingle(data._field_0x3C);
			section._sh.WriteSingle(data._field_0x40);
			section._sh.WriteSingle(data._field_0x44);
			section._sh.WriteSingle(data._field_0x48);
			section._sh.WriteUInt16(data._field_0x4C);
			section._sh.WriteBoolean(data._field_0x4E);
			section._sh.WriteByte(data._flags);
			section._sh.WriteUInt16(data._field_0x50);
			section._sh.WriteUInt16(data._field_0x52);

		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x54;
		public override Type GetOutputType() => typeof(igVfxRangedCurve);
	}
	public class igVfxRangedCurveArrayMetaField : igVfxRangedCurveMetaField
	{
		public short _num;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			Array data = Array.CreateInstance(base.GetOutputType(), _num);
			for(int i = 0; i < _num; i++)
			{
				data.SetValue(base.ReadIGZField(loader), i);
			}
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			Array data = (Array)value;
			for(int i = 0; i < _num; i++)
			{
				base.WriteIGZField(saver, section, data.GetValue(i));
			}
		}
		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			return base.GetSize(platform) * (uint)_num;
		}
		public override Type GetOutputType()
		{
			return base.GetOutputType().MakeArrayType();
		}
	}
}