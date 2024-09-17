using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec3dMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec3d data = new igVec3d();
			data._x = loader._stream.ReadDouble();
			data._y = loader._stream.ReadDouble();
			data._z = loader._stream.ReadDouble();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec3d data = (igVec3d)value;
			section._sh.WriteDouble(data._x);
			section._sh.WriteDouble(data._y);
			section._sh.WriteDouble(data._z);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x08;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x18;
		public override Type GetOutputType() => typeof(igVec3d);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x18);
			igVec3d data = (igVec3d)_default;
			sh.WriteDouble(data._x);
			sh.WriteDouble(data._y);
			sh.WriteDouble(data._z);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec3d data = new igVec3d();
			data._x = sh.ReadDouble();
			data._y = sh.ReadDouble();
			data._z = sh.ReadDouble();
			_default = data;
		}
	}
}