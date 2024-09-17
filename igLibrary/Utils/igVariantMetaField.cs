using igLibrary.Core;

namespace igLibrary.Utils
{
	public class igVariantMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVariant data = new igVariant();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igVariant);
	}
}