using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igVfxRangedRampMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRangedRamp data = new igVfxRangedRamp();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18;
		public override Type GetOutputType() => typeof(igVfxRangedRamp);
	}
}