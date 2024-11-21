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


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			string[] floats = input.Split(',');
			if (floats.Length != 3) return false;

			igVec3f tempTarget;

			if (!float.TryParse(floats[0], out tempTarget._x)) return false;
			if (!float.TryParse(floats[1], out tempTarget._y)) return false;
			if (!float.TryParse(floats[2], out tempTarget._z)) return false;

			target = tempTarget;

			return true;
		}
	}
}