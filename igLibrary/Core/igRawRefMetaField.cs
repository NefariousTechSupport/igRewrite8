namespace igLibrary.Core
{
	public class igRawRefMetaField : igRefMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader.ReadRawOffset();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value){}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(ulong);
	}
}