namespace igLibrary.Core
{
	public class igBoolMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadBoolean();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteByte((byte)((bool)value ? 1u : 0u));
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 1;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 1;
		public override Type GetOutputType() => typeof(bool);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(1);
			sh.WriteByte((byte)((bool)_default ? 1 : 0));
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadBoolean();
		}
	}
}