using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec4iMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4i data = new igVec4i();
			data._x = loader._stream.ReadInt32();
			data._y = loader._stream.ReadInt32();
			data._z = loader._stream.ReadInt32();
			data._w = loader._stream.ReadInt32();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec4i data = (igVec4i)value;
			section._sh.WriteInt32(data._x);
			section._sh.WriteInt32(data._y);
			section._sh.WriteInt32(data._z);
			section._sh.WriteInt32(data._w);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x10;
		public override Type GetOutputType() => typeof(igVec4i);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x10);
			igVec4i data = (igVec4i)_default;
			sh.WriteInt32(data._x);
			sh.WriteInt32(data._y);
			sh.WriteInt32(data._z);
			sh.WriteInt32(data._w);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec4i data = new igVec4i();
			data._x = sh.ReadInt32();
			data._y = sh.ReadInt32();
			data._z = sh.ReadInt32();
			data._w = sh.ReadInt32();
			_default = data;
		}
	}
}