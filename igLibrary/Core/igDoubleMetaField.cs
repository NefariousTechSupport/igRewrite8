namespace igLibrary.Core
{
	public class igDoubleMetaField : igMetaField
	{
		public static igDoubleMetaField _MetaField { get; private set; } = new igDoubleMetaField();
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadDouble();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteDouble((double)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN || platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX) return 4;
			return 8;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => 8;
		public override Type GetOutputType() => typeof(double);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			sh.WriteDouble((double)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadDouble();
		}
	}
}