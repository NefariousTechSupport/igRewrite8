namespace igLibrary.Core
{
	public class igIntPtrMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader.ReadRawOffset();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => saver.WriteRawOffset(unchecked((ulong)value), section);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(long);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			sh.WriteInt64((long)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadInt64();
		}
	}
}