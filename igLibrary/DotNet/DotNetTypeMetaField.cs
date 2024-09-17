using igLibrary.Core;

namespace igLibrary.DotNet
{
	public class DotNetTypeMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong baseOffset = loader._stream.Tell64();
			DotNetType data = new DotNetType();

			return data;
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 8;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18 + (igAlchemyCore.isPlatform64Bit(platform) ? 8u : 0u);
		public override Type GetOutputType() => typeof(DotNetType);
	}
}