namespace igLibrary.Core
{
	public class igShortMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadInt16();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteInt16((short)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 2;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 2;
		public override Type GetOutputType() => typeof(short);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(2);
			sh.WriteInt16((short)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadInt16();
		}
	}
}