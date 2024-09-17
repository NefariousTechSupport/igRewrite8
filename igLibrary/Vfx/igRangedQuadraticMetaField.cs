using igLibrary.Core;

namespace igLibrary.Vfx
{
	public class igRangedQuadraticMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igRangedQuadratic data = new igRangedQuadratic();
			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x20;
		public override Type GetOutputType() => typeof(igRangedQuadratic);
	}
}