namespace igLibrary.Core
{
	public class igCharMetaField : igMetaField
	{
		public static igCharMetaField _MetaField { get; private set; } = new igCharMetaField();
		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadSByte();
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 1;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 1;
		public override Type GetOutputType() => typeof(sbyte);	//Not sure if this should be char or sbyte
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(1);
			sh.WriteByte(unchecked((byte)(sbyte)_default));
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadSByte();
		}
	}
}