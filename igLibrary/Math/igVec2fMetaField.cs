using igLibrary.Core;

namespace igLibrary.Math
{
	public class igVec2fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igVec2f data = new igVec2f();
			data._x = loader._stream.ReadSingle();
			data._y = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igVec2f data = (igVec2f)value;
			section._sh.WriteSingle(data._x);
			section._sh.WriteSingle(data._y);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x04;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x08;
		public override Type GetOutputType() => typeof(igVec2f);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			igVec2f data = (igVec2f)_default;
			sh.WriteSingle(data._x);
			sh.WriteSingle(data._y);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igVec2f data = new igVec2f();
			data._x = sh.ReadSingle();
			data._y = sh.ReadSingle();
			_default = data;
		}
	}
}