namespace igLibrary.Core
{
	public class igUnsignedLongMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUInt64();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteUInt64((ulong)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN || platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX) return 4;
			return 8;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform) => 8;
		public override Type GetOutputType() => typeof(ulong);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			sh.WriteUInt64((ulong)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadUInt64();
		}
	}
}