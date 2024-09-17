using igLibrary.Core;

namespace igLibrary.DotNet
{
	public class DotNetDataMetaField : igMetaField
	{
		public static DotNetTypeMetaField _typeField;
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong baseOffset = loader._stream.Tell64();
			DotNetData data = new DotNetData();

			loader._stream.Seek(baseOffset + 0x08);
			data._type._baseMeta = (igBaseMeta?)igObjectRefMetaField.GetMetaField().ReadIGZField(loader);
			data._type._flags = loader._stream.ReadUInt32();
			if(igAlchemyCore.isPlatform64Bit(loader._platform))
			{
				loader._stream.Seek(4, SeekOrigin.Current);
			}
			data._maybeRepresentation = loader._stream.ReadUInt32();

			loader._stream.Seek(baseOffset);
			ElementType elementType = (ElementType)(data._type._flags & (uint)DotNetType.Flags.kTypeMask);
			switch(elementType)
			{
				case ElementType.kElementTypeEnd:
				case ElementType.kElementTypeVoid:
					break;
				case ElementType.kElementTypeR8:		//Oddly enough, doubles aren't allowed
				case ElementType.kElementTypePtr:
				case ElementType.kElementTypeByRef:
					throw new NotImplementedException($"Element type {elementType} not supported");
				case ElementType.kElementTypeBoolean:
					data._data = loader._stream.ReadBoolean();
					break;
				case ElementType.kElementTypeChar:
					data._data = loader._stream.ReadChar();
					break;
				case ElementType.kElementTypeI1:
					data._data = loader._stream.ReadSByte();
					break;
				case ElementType.kElementTypeU1:
					data._data = loader._stream.ReadByte();
					break;
				case ElementType.kElementTypeI2:
					data._data = loader._stream.ReadInt16();
					break;
				case ElementType.kElementTypeU2:
					data._data = loader._stream.ReadUInt16();
					break;
				case ElementType.kElementTypeI4:
					data._data = loader._stream.ReadInt32();
					break;
				case ElementType.kElementTypeU4:
					data._data = loader._stream.ReadUInt32();
					break;
				case ElementType.kElementTypeI8:
					data._data = loader._stream.ReadInt64();
					break;
				case ElementType.kElementTypeU8:
					data._data = loader._stream.ReadUInt64();
					break;
				case ElementType.kElementTypeR4:
					data._data = loader._stream.ReadSingle();
					break;
				case ElementType.kElementTypeString:
					data._data = igStringMetaField.GetMetaField().ReadIGZField(loader);
					break;
				case ElementType.kElementTypeValueType:
				case ElementType.kElementTypeClass:
				case ElementType.kElementTypeObject:
					if(data._type._baseMeta is igMetaEnum metaEnum)
					{
						igEnumMetaField enumField = igEnumMetaField.GetMetaField();
						enumField._metaEnum = metaEnum;
						data._data = enumField.ReadIGZField(loader);
#pragma warning disable CS8625
						enumField._metaEnum = null;
#pragma warning restore CS8625
					}
					else
					{
						data._data = igObjectRefMetaField.GetMetaField().ReadIGZField(loader);
					}
					break;
			}

			loader._stream.Seek(baseOffset + GetSize(loader._platform));

			return data;
		}
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
#pragma warning disable CS8605, CS8625
			DotNetData data = (DotNetData)value;

			ulong baseOffset = section._sh.Tell64();
			section._sh.Seek(baseOffset + 0x08);
			igObjectRefMetaField metaWriter = igObjectRefMetaField.GetMetaField();
			metaWriter._refCounted = false;
			metaWriter.WriteIGZField(saver, section, data._type._baseMeta);
			metaWriter._refCounted = true;
			section._sh.WriteUInt32(data._type._flags);
			if(igAlchemyCore.isPlatform64Bit(saver._platform))
			{
				section._sh.Seek(4, SeekOrigin.Current);
			}
			section._sh.WriteUInt32(data._maybeRepresentation);

			section._sh.Seek(baseOffset);
			ElementType elementType = (ElementType)(data._type._flags & (uint)DotNetType.Flags.kTypeMask);

			switch(elementType)
			{
				case ElementType.kElementTypeEnd:		//Signifies end of array
				case ElementType.kElementTypeVoid:		//Signifies void type
					break;
				case ElementType.kElementTypeR8:		//Oddly enough, doubles aren't allowed
				case ElementType.kElementTypePtr:
				case ElementType.kElementTypeByRef:
					throw new NotImplementedException($"Element type {elementType} not supported");
				case ElementType.kElementTypeBoolean:
					section._sh.WriteByte((byte)((bool)data._data ? 1 : 0));
					break;
				case ElementType.kElementTypeChar:
					section._sh.WriteByte((byte)data._data);
					break;
				case ElementType.kElementTypeI1:
					section._sh.WriteSByte((sbyte)data._data);
					break;
				case ElementType.kElementTypeU1:
					section._sh.WriteByte((byte)data._data);
					break;
				case ElementType.kElementTypeI2:
					section._sh.WriteInt16((short)data._data);
					break;
				case ElementType.kElementTypeU2:
					section._sh.WriteUInt16((ushort)data._data);
					break;
				case ElementType.kElementTypeI4:
					section._sh.WriteInt32((int)data._data);
					break;
				case ElementType.kElementTypeU4:
					section._sh.WriteUInt32((uint)data._data);
					break;
				case ElementType.kElementTypeI8:
					section._sh.WriteInt64((long)data._data);
					break;
				case ElementType.kElementTypeU8:
					section._sh.WriteUInt64((ulong)data._data);
					break;
				case ElementType.kElementTypeR4:
					section._sh.WriteSingle((float)data._data);
					break;
				case ElementType.kElementTypeString:
					igStringMetaField.GetMetaField().WriteIGZField(saver, section, data._data);
					break;
				case ElementType.kElementTypeValueType:
				case ElementType.kElementTypeClass:
				case ElementType.kElementTypeObject:
					if(data._type._baseMeta is igMetaEnum metaEnum)
					{
						igEnumMetaField enumField = igEnumMetaField.GetMetaField();
						enumField._metaEnum = metaEnum;
						enumField.WriteIGZField(saver, section, data._data);
						enumField._metaEnum = null;
					}
					else
					{
						igObjectRefMetaField.GetMetaField().WriteIGZField(saver, section, data._data);
					}
					break;
			}
#pragma warning restore CS8605, CS8625
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => igAlchemyCore.GetPointerSize(platform);
		public override uint GetSize(IG_CORE_PLATFORM platform) => igAlchemyCore.isPlatform64Bit(platform) ? 0x20u : 0x18u;
		public override Type GetOutputType() => typeof(DotNetData);
	}
}