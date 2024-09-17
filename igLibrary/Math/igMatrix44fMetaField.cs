using igLibrary.Core;

namespace igLibrary.Math
{
	public class igMatrix44fMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader)
		{
			igMatrix44f data = new igMatrix44f();
			data._m11 = loader._stream.ReadSingle();
			data._m12 = loader._stream.ReadSingle();
			data._m13 = loader._stream.ReadSingle();
			data._m14 = loader._stream.ReadSingle();
			data._m21 = loader._stream.ReadSingle();
			data._m22 = loader._stream.ReadSingle();
			data._m23 = loader._stream.ReadSingle();
			data._m24 = loader._stream.ReadSingle();
			data._m31 = loader._stream.ReadSingle();
			data._m32 = loader._stream.ReadSingle();
			data._m33 = loader._stream.ReadSingle();
			data._m34 = loader._stream.ReadSingle();
			data._m41 = loader._stream.ReadSingle();
			data._m42 = loader._stream.ReadSingle();
			data._m43 = loader._stream.ReadSingle();
			data._m44 = loader._stream.ReadSingle();
			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			igMatrix44f data = (igMatrix44f)value;
			section._sh.WriteSingle(data._m11);
			section._sh.WriteSingle(data._m12);
			section._sh.WriteSingle(data._m13);
			section._sh.WriteSingle(data._m14);
			section._sh.WriteSingle(data._m21);
			section._sh.WriteSingle(data._m22);
			section._sh.WriteSingle(data._m23);
			section._sh.WriteSingle(data._m24);
			section._sh.WriteSingle(data._m31);
			section._sh.WriteSingle(data._m32);
			section._sh.WriteSingle(data._m33);
			section._sh.WriteSingle(data._m34);
			section._sh.WriteSingle(data._m41);
			section._sh.WriteSingle(data._m42);
			section._sh.WriteSingle(data._m43);
			section._sh.WriteSingle(data._m44);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0x10;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0x40;
		public override Type GetOutputType() => typeof(igMatrix44f);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(0x40);
			igMatrix44f data = (igMatrix44f)_default;
			sh.WriteSingle(data._m11);
			sh.WriteSingle(data._m12);
			sh.WriteSingle(data._m13);
			sh.WriteSingle(data._m14);
			sh.WriteSingle(data._m21);
			sh.WriteSingle(data._m22);
			sh.WriteSingle(data._m23);
			sh.WriteSingle(data._m24);
			sh.WriteSingle(data._m31);
			sh.WriteSingle(data._m32);
			sh.WriteSingle(data._m33);
			sh.WriteSingle(data._m34);
			sh.WriteSingle(data._m41);
			sh.WriteSingle(data._m42);
			sh.WriteSingle(data._m43);
			sh.WriteSingle(data._m44);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			igMatrix44f data = new igMatrix44f();
			data._m11 = sh.ReadSingle();
			data._m12 = sh.ReadSingle();
			data._m13 = sh.ReadSingle();
			data._m14 = sh.ReadSingle();
			data._m21 = sh.ReadSingle();
			data._m22 = sh.ReadSingle();
			data._m23 = sh.ReadSingle();
			data._m24 = sh.ReadSingle();
			data._m31 = sh.ReadSingle();
			data._m32 = sh.ReadSingle();
			data._m33 = sh.ReadSingle();
			data._m34 = sh.ReadSingle();
			data._m41 = sh.ReadSingle();
			data._m42 = sh.ReadSingle();
			data._m43 = sh.ReadSingle();
			data._m44 = sh.ReadSingle();
			_default = data;
		}
	}
}