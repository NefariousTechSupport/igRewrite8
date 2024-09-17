using System.Reflection;

namespace igLibrary.Core
{
	//Should define DotNet::igDotNetEnumMetaField but it's not too important as it doesn't appear in igzs
	public class igEnumMetaField : igMetaField
	{
		private static igEnumMetaField _MetaField = new igEnumMetaField();
		public static igEnumMetaField GetMetaField() => _MetaField;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _getMetaEnumFunction;

		public igMetaEnum _metaEnum;

		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			saver.SaveString(sh, _metaEnum == null ? null : _metaEnum._name);
		}
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_metaEnum = igArkCore.GetMetaEnum(loader.ReadString(sh));
		}

		public override object? ReadIGZField(igIGZLoader loader)
		{
			int raw = loader._stream.ReadInt32();
			if(_metaEnum != null) return _metaEnum.GetEnumFromValue(raw);
			else                  return raw;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			if(_metaEnum == null) section._sh.WriteInt32((int)value);

			int raw = _metaEnum.GetValueFromEnum(value);
			section._sh.WriteInt32(raw);
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 4;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 4;
		public override Type GetOutputType() => _metaEnum == null ? typeof(int) : _metaEnum._internalType;
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			sh.WriteUInt32(4);			
			sh.WriteInt32((int)_default);			
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			_default = sh.ReadInt32();
		}
		public override object? GetDefault(igMemoryPool pool)
		{
			if(_metaEnum == null) return _default;
			if(_default != null) return _metaEnum.GetEnumFromValue((int)_default);
			return Activator.CreateInstance(GetOutputType());
		}
	}
}