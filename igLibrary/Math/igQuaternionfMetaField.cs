using igLibrary.Core;

namespace igLibrary.Math
{
	public class igQuaternionfMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igQuaternionf data = new igQuaternionf();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			data._z = loader._stream.ReadSingle();
			data._w = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igQuaternionf data = (igQuaternionf)value;
			section._sh.WriteSingle(data._x);
			section._sh.WriteSingle(data._y);
			section._sh.WriteSingle(data._z);
			section._sh.WriteSingle(data._w);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x10;
		public override Type GetOutputType() => typeof(igQuaternionf);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(0x10);
			igQuaternionf data = (igQuaternionf)_default;
			sh.WriteSingle(data._x);
			sh.WriteSingle(data._y);
			sh.WriteSingle(data._z);
			sh.WriteSingle(data._w);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igQuaternionf data = new igQuaternionf();
			data._x = sh.ReadSingle();
			data._y = sh.ReadSingle();
			data._z = sh.ReadSingle();
			data._w = sh.ReadSingle();
			_default = data;
		}
	}
}