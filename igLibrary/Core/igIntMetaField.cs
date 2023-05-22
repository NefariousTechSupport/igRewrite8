namespace igLibrary.Core
{
	public class igIntMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadInt32();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteInt32((int)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => typeof(int);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(4);
			sh.WriteInt32((int)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadInt32();
		}
	}
	public class igIntArrayMetaField : igIntMetaField
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