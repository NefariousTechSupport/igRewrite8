namespace igLibrary.Core
{
	public class igUnsignedShortMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadUInt16();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteUInt16((ushort)value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 2;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 2;
		public override Type GetOutputType() => typeof(ushort);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(2);
			sh.WriteUInt16((ushort)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadUInt16();
		}
	}
}