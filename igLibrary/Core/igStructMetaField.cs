namespace igLibrary.Core
{
	public class igStructMetaField : igMetaField
	{
		public Dictionary<IG_CORE_PLATFORM, ushort> _sizes = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Dictionary<IG_CORE_PLATFORM, ushort> _alignments = new Dictionary<IG_CORE_PLATFORM, ushort>();
		[Obsolete("This exists for the reflection system, use _sizes instead.")] public int _typeSize;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _cppConstructor;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _cppDestructor;

		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);
			_sizes.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, sh.ReadUInt16());
			_alignments.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, sh.ReadUInt16());
		}
		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);
			sh.WriteUInt16((ushort)GetSize(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT));
			sh.WriteUInt16((ushort)GetAlignment(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT));
		}

		public override object? ReadIGZField(igIGZLoader loader) => loader._stream.ReadBytes(GetSize(loader._platform));
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value) => section._sh.BaseStream.Write((byte[])value);
		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			if(!_alignments.TryGetValue(platform, out ushort align))
			{
				align = _alignments[IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT];
			}
			return align;
		}
		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			if(!_sizes.TryGetValue(platform, out ushort size))
			{
				size = _sizes[IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT];
			}
			return size;
		}
		public override Type GetOutputType() => typeof(byte[]);
	}
}