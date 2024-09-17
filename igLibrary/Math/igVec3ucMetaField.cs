using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec3ucMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec3uc data = new igVec3uc();
			data._x = loader._stream.ReadByte();
			data._y = loader._stream.ReadByte();
			data._z = loader._stream.ReadByte();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec3uc data = (igVec3uc)value;
			section._sh.WriteByte(data._x);
			section._sh.WriteByte(data._y);
			section._sh.WriteByte(data._z);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x01;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x03;
		public override Type GetOutputType() => typeof(igVec3uc);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x03);
			igVec3uc data = (igVec3uc)_default;
			sh.WriteByte(data._x);
			sh.WriteByte(data._y);
			sh.WriteByte(data._z);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec3uc data = new igVec3uc();
			data._x = sh.ReadByte();
			data._y = sh.ReadByte();
			data._z = sh.ReadByte();
			_default = data;
		}
	}
}