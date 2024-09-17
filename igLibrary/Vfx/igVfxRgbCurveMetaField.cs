using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igVfxRgbCurveMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVfxRgbCurve data = new igVfxRgbCurve();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x110;
		public override Type GetOutputType() => typeof(igVfxRgbCurve);
	}
}