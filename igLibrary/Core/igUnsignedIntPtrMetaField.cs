namespace igLibrary.Core
{
	public class igUnsignedIntPtrMetaField : igMetaField
	{
		public override object? ReadIGZField(igIGZLoader loader) => loader.ReadRawOffset();
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => saver.WriteRawOffset((ulong)value, section);
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override Type GetOutputType() => typeof(ulong);
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(8);
			sh.WriteUInt64((ulong)_default);
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadUInt64();
		}


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			if (!ulong.TryParse(input, out ulong buffer))
			{
				return false;
			}
			target = buffer;
			return true;
		}
	}
}