/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

namespace igLibrary.Core
{
	/// <summary>
	/// Metafield for bitfields
	/// </summary>
	public class igBitFieldMetaField : igMetaField
	{
		public igMetaField _storageMetaField;
		public igMetaField _assignmentMetaField;
		public uint _bits;
		public uint _shift;


		/// <summary>
		/// Dump binary ark data
		/// </summary>
		/// <param name="saver">The saver</param>
		/// <param name="sh">The streamhelper</param>
		public override void DumpArkData(igArkCoreFile saver, StreamHelper sh)
		{
			base.DumpArkData(saver, sh);

			sh.WriteUInt32(_shift);
			sh.WriteUInt32(_bits);
			saver.SaveString(sh, _storageMetaField._fieldName);
			saver.SaveMetaField(sh, _assignmentMetaField);
		}


		/// <summary>
		/// Undump binary ark data
		/// </summary>
		/// <param name="loader">The loader</param>
		/// <param name="sh">The streamhelper</param>
		public override void UndumpArkData(igArkCoreFile loader, StreamHelper sh)
		{
			base.UndumpArkData(loader, sh);

			_shift = sh.ReadUInt32();
			_bits = sh.ReadUInt32();
			_storageMetaField = _parentMeta.GetFieldByName(loader.ReadString(sh));
			_assignmentMetaField = loader.ReadMetaField(sh);
		}


		/// <summary>
		/// Read data from the igz
		/// </summary>
		/// <param name="loader">The loader</param>
		/// <returns>The value</returns>
		/// <exception cref="NotImplementedException">Thrown when there's an unimplemented bitfield type</exception>
		public override object? ReadIGZField(igIGZLoader loader)
		{
			ulong storage = (ulong)Convert.ChangeType(_storageMetaField.ReadIGZField(loader), typeof(ulong));
			storage = (storage >> (int)_shift) & (0xFFFFFFFFFFFFFFFF >> (int)(64 - _bits));

			if(_assignmentMetaField is igUnsignedIntMetaField)		return unchecked((uint)storage);
			if(_assignmentMetaField is igIntMetaField)				return unchecked((int)storage);
			if(_assignmentMetaField is igUnsignedCharMetaField)		return unchecked((byte)storage);
			if(_assignmentMetaField is igCharMetaField)				return unchecked((sbyte)storage);
			if(_assignmentMetaField is igBoolMetaField)				return storage != 0;
			if(_assignmentMetaField is igUnsignedShortMetaField)	return unchecked((ushort)storage);
			if(_assignmentMetaField is igShortMetaField)			return unchecked((short)storage);
			if(_assignmentMetaField is igUnsignedLongMetaField)		return storage;
			if(_assignmentMetaField is igLongMetaField)				return unchecked((long)storage);
			if(_assignmentMetaField is igEnumMetaField enumMf)		return enumMf._metaEnum.GetEnumFromValue((int)storage);

			throw new NotImplementedException($"_assignmentMetaField for {_assignmentMetaField.GetType().Name} is not implemented, contact a developer.");
		}


		/// <summary>
		/// Write igz field
		/// </summary>
		/// <param name="saver">the saver</param>
		/// <param name="section">the section of the file</param>
		/// <param name="value">the value</param>
		/// <exception cref="NotImplementedException">Thrown when there's an unimplemented bitfield type</exception>
		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			uint size = _storageMetaField.GetSize(saver._platform);
			ulong storage;
			switch(size)
			{
				case 1:
					storage = section._sh.ReadByte();
					break;
				case 2:
					storage = section._sh.ReadUInt16();
					break;
				case 4:
					storage = section._sh.ReadUInt32();
					break;
				case 8:
					storage = section._sh.ReadUInt64();
					break;
				default:
					throw new NotSupportedException("unsupported assignment type");
			}
			section._sh.Seek(-size, SeekOrigin.Current);
			ulong assignment;

				 if(_assignmentMetaField is igBoolMetaField)          assignment = (ulong)(((bool)value) ? 1 : 0);
			else if(_assignmentMetaField is igCharMetaField)          assignment = unchecked((ulong)(sbyte)value);
			else if(_assignmentMetaField is igUnsignedCharMetaField)  assignment = (ulong)(byte)value;
			else if(_assignmentMetaField is igShortMetaField)         assignment = unchecked((ulong)(short)value);
			else if(_assignmentMetaField is igUnsignedShortMetaField) assignment = (ulong)(ushort)value;
			else if(_assignmentMetaField is igIntMetaField)           assignment = unchecked((ulong)(int)value);
			else if(_assignmentMetaField is igUnsignedIntMetaField)   assignment = (ulong)(uint)value;
			else if(_assignmentMetaField is igEnumMetaField e)        assignment = unchecked((ulong)e._metaEnum.GetValueFromEnum(value));
			else if(_assignmentMetaField is igLongMetaField)          assignment = unchecked((ulong)(long)value);
			else if(_assignmentMetaField is igUnsignedLongMetaField)  assignment = (ulong)value;
			else throw new NotSupportedException("unsupported assignment type");

			ulong mask1 = 0xFFFFFFFFFFFFFFFF;
			mask1 >>= (int)(64 - _bits);
			ulong mask2 = mask1;
			mask2 <<= (int)_shift;
			ulong mask3 = ~mask2;
			storage = (storage & mask3) | (assignment << (int)_shift);
			switch(size)
			{
				case 1:
					section._sh.WriteByte((byte)storage);
					break;
				case 2:
					section._sh.WriteUInt16((ushort)storage);
					break;
				case 4:
					section._sh.WriteUInt32((uint)storage);
					break;
				case 8:
					section._sh.WriteUInt64((ulong)storage);
					break;
				default:
					throw new NotSupportedException("unsupported assignment type");
			}
		}
		public override void DumpDefault(igArkCoreFile saver, StreamHelper sh)
		{
			uint size = _assignmentMetaField.GetSize(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT);
			//sh.WriteInt32(-1);
			//return;
			sh.WriteUInt32(size);
				 if(_assignmentMetaField is igBoolMetaField)          sh.WriteByte((byte)(((bool)_default) ? 1 : 0));
			else if(_assignmentMetaField is igCharMetaField)          sh.WriteSByte((sbyte)_default);
			else if(_assignmentMetaField is igUnsignedCharMetaField)  sh.WriteByte((byte)_default);
			else if(_assignmentMetaField is igShortMetaField)         sh.WriteInt16((short)_default);
			else if(_assignmentMetaField is igUnsignedShortMetaField) sh.WriteUInt16((ushort)_default);
			else if(_assignmentMetaField is igIntMetaField)           sh.WriteInt32((int)_default);
			else if(_assignmentMetaField is igUnsignedIntMetaField)   sh.WriteUInt32((uint)_default);
			else if(_assignmentMetaField is igEnumMetaField)          sh.WriteInt32((int)_default);
			else if(_assignmentMetaField is igLongMetaField)          sh.WriteInt64((long)_default);
			else if(_assignmentMetaField is igUnsignedLongMetaField)  sh.WriteUInt64((ulong)_default);
			else throw new NotSupportedException("unsupported assignment type");
		}
		public override void UndumpDefault(igArkCoreFile loader, StreamHelper sh)
		{
			//sh.Seek(_assignmentMetaField.GetSize(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT), SeekOrigin.Current);
			
				 if(_assignmentMetaField is igBoolMetaField)          _default = sh.ReadByte() != 0;
			else if(_assignmentMetaField is igCharMetaField)          _default = sh.ReadSByte();
			else if(_assignmentMetaField is igUnsignedCharMetaField)  _default = sh.ReadByte();
			else if(_assignmentMetaField is igShortMetaField)         _default = sh.ReadInt16();
			else if(_assignmentMetaField is igUnsignedShortMetaField) _default = sh.ReadUInt16();
			else if(_assignmentMetaField is igIntMetaField)           _default = sh.ReadInt32();
			else if(_assignmentMetaField is igUnsignedIntMetaField)   _default = sh.ReadUInt32();
			else if(_assignmentMetaField is igEnumMetaField)          _default = sh.ReadInt32();
			else if(_assignmentMetaField is igLongMetaField)          _default = sh.ReadInt64();
			else if(_assignmentMetaField is igUnsignedLongMetaField)  _default = sh.ReadUInt64();
			else throw new NotSupportedException("unsupported assignment type");
		}
		public override uint GetAlignment(IG_CORE_PLATFORM platform) => 0;
		public override uint GetSize(IG_CORE_PLATFORM platform) => 0;
		public override Type GetOutputType() => _assignmentMetaField.GetOutputType();


		/// <summary>
		/// Sets the target variable based on the string representation of the input
		/// </summary>
		/// <param name="target">The output field</param>
		/// <param name="input">The input field</param>
		/// <returns>boolean indicating whether the input was read successfully</returns>
		public override bool SetMemoryFromString(ref object? target, string input)
		{
			return _assignmentMetaField.SetMemoryFromString(ref target, input);
		}
	}
}