namespace igLibrary.Core
{
	public class igTimeOfDayMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igTimeOfDay data = new igTimeOfDay();
			data._year = loader._stream.ReadUInt32();
			data._month = loader._stream.ReadUInt32();
			data._day = loader._stream.ReadUInt32();
			data._hour = loader._stream.ReadUInt32();
			data._minute = loader._stream.ReadUInt32();
			data._second = loader._stream.ReadUInt32();
			data._unk = loader._stream.ReadUInt32();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igTimeOfDay data = (igTimeOfDay)value;
			section._sh.ReadUInt32(data._year);
			section._sh.ReadUInt32(data._month);
			section._sh.ReadUInt32(data._day);
			section._sh.ReadUInt32(data._hour);
			section._sh.ReadUInt32(data._minute);
			section._sh.ReadUInt32(data._second);
			section._sh.ReadUInt32(data._unk);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x1C;
		public override Type GetOutputType() => typeof(igTimeOfDay);
	}
	public class igTimeOfDayArrayMetaField : igTimeOfDayMetaField
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