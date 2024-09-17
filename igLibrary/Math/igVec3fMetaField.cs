using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec3fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec3f data = new igVec3f();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			data._z = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec3f data = (igVec3f)value;
			section._sh.WriteSingle(data._x);
			section._sh.WriteSingle(data._y);
			section._sh.WriteSingle(data._z);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x0C;
		public override Type GetOutputType() => typeof(igVec3f);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x0C);
			igVec3f data = (igVec3f)_default;
			sh.WriteSingle(data._x);
			sh.WriteSingle(data._y);
			sh.WriteSingle(data._z);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec3f data = new igVec3f();
			data._x = sh.ReadSingle();
			data._y = sh.ReadSingle();
			data._z = sh.ReadSingle();
			_default = data;
		}
	}
}