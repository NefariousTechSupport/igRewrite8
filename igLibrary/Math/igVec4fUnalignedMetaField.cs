using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec4fUnalignedMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec4fUnaligned data = new igVec4fUnaligned();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			data._z = loader._stream.ReadSingle();
			data._w = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec4fUnaligned data = (igVec4fUnaligned)value;
			section._sh.WriteSingle(data._x);
			section._sh.WriteSingle(data._y);
			section._sh.WriteSingle(data._z);
			section._sh.WriteSingle(data._w);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x10;
		public override Type GetOutputType() => typeof(igVec4fUnaligned);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteInt32(0x10);
			igVec4fUnaligned data = (igVec4fUnaligned)_default;
			sh.WriteSingle(data._x);
			sh.WriteSingle(data._y);
			sh.WriteSingle(data._z);
			sh.WriteSingle(data._w);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec4fUnaligned data = new igVec4fUnaligned();
			data._x = sh.ReadSingle();
			data._y = sh.ReadSingle();
			data._z = sh.ReadSingle();
			data._w = sh.ReadSingle();
			_default = data;
		}
	}
}