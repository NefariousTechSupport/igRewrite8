namespace igLibrary.Core
{
	public class igUnsignedIntPtrMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader.ReadRawOffset();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => saver.WriteRawOffset((ulong)value, section);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
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