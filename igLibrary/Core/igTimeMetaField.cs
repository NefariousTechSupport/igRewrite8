namespace igLibrary.Core
{
	public class igTimeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => new igTime(loader._stream.ReadSingle());
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.WriteSingle(((igTime)value!)._elapsedDays);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => typeof(igTime);
	}
}