using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedVectorMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedVector data = new igRangedVector();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18;
		public override Type GetOutputType() => typeof(igRangedVector);
	}
}