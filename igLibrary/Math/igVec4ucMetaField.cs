using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec4ucMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4uc data = new igVec4uc();
			data._r = loader._stream.ReadByte();
			data._g = loader._stream.ReadByte();
			data._b = loader._stream.ReadByte();
			data._a = loader._stream.ReadByte();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec4uc data = (igVec4uc)value;
			section._sh.WriteByte(data._r);
			section._sh.WriteByte(data._g);
			section._sh.WriteByte(data._b);
			section._sh.WriteByte(data._a);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x01;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x04;
		public override Type GetOutputType() => typeof(igVec4uc);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x10);
			igVec4uc data = (igVec4uc)_default;
			sh.WriteByte(data._r);
			sh.WriteByte(data._g);
			sh.WriteByte(data._b);
			sh.WriteByte(data._a);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec4uc data = new igVec4uc();
			data._r = sh.ReadByte();
			data._g = sh.ReadByte();
			data._b = sh.ReadByte();
			data._a = sh.ReadByte();
			_default = data;
		}
	}
}