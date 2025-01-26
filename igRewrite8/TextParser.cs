/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary;
using igLibrary.Core;
using System.Reflection;

namespace igRewrite8.Devel
{
	//A lot of this is very poorly designed since it's meant to handle a temporary file format, perhaps i should make it a better format first lol

	public class TextParser
	{
		public Dictionary<string, igMetaObject> metaObjectLookup = new Dictionary<string, igMetaObject>();
		public Dictionary<string, igCompoundMetaFieldInfo> compoundInfoLookup = new Dictionary<string, igCompoundMetaFieldInfo>();

		public void ReadMetaEnumFile(string filePath)
		{
			StreamHelper sh = new StreamHelper(File.Open(filePath, FileMode.Open));

			while(true)
			{
				igMetaEnum metaEnum = new igMetaEnum();
				string metaEnumLine = sh.ReadLine();
				if(metaEnumLine.Length == 0) break;
				metaEnum._name = metaEnumLine.Split(' ')[1];
				sh.ReadLine();
				while(true)
				{
					string memberLine = sh.ReadLine();
					if(memberLine[0] == '}') break;
					string[] memberInfo = memberLine.Substring(1).Split(' ');
					metaEnum._names.Add(memberInfo[0]);
					metaEnum._values.Add(int.Parse(memberInfo[2].TrimEnd(',')));
				}
				igArkCore.AddEnumMeta(metaEnum);
			}
		}
		public void ReadMetaFieldFile(string filePath)
		{
			StreamHelper fieldSh = new StreamHelper(new FileStream(filePath, FileMode.Open, FileAccess.Read));

			List<IG_CORE_PLATFORM> platformLookup = new List<IG_CORE_PLATFORM>();
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX);
			platformLookup.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX);

			while(true)
			{
				string metaFieldLine = fieldSh.ReadLine();
				if(metaFieldLine.Length == 0) break;

				string[] members = metaFieldLine.Split(' ');


				int index = 0;
				igMetaFieldPlatformInfo platformInfo = new igMetaFieldPlatformInfo();
				platformInfo._name = members[index++];
				while(true)
				{
					index++;	//Skip "psa"
					if(index > members.Length) break;
					IG_CORE_PLATFORM platform = platformLookup[int.Parse(members[index++], System.Globalization.NumberStyles.HexNumber)];
					ushort size = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);
					ushort alignment = ushort.Parse(members[index++], System.Globalization.NumberStyles.HexNumber);

					platformInfo._sizes.Add(platform, size);
					platformInfo._alignments.Add(platform, alignment);
				}

				igArkCore.AddPlatformMeta(platformInfo);
			}
		}
		public void ReadMetaObjectFile(string filePath)
		{
			StreamHelper sh = new StreamHelper(File.Open(filePath, FileMode.Open));

			//First pass, instantiate metaobjects

			while(true)
			{
				string line = sh.ReadLine();

				if(line.Length == 0) break;

				string[] members = line.Split(' ');
				if(members[0] == "igMetaObject")
				{
					igMetaObject metaObject = (igMetaObject)Activator.CreateInstance(igArkCore.GetObjectDotNetType(members[1]));
					metaObject._name = members[2];

					metaObjectLookup.Add(metaObject._name, metaObject);
				}
				else if(members[0] == "igCompoundField")
				{
					igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
					compoundInfo._name = metaObjectLookup.Last().Key;
					compoundInfoLookup.Add(compoundInfo._name, compoundInfo);
				}
			}

			//Second pass, read metaobject information
			sh.Seek(0);
			string previousMeta = null;

			for(int i = 0; i < metaObjectLookup.Count + compoundInfoLookup.Count; i++)
			{
				string metaObjectLine = sh.ReadLine();

				if(metaObjectLine.Length == 0) break;

				string[] members = metaObjectLine.Split(' ');

				List<(int, igMetaField)> replacedFields = new List<(int, igMetaField)>();

				if(members[0] == "igMetaObject")
				{
					igMetaObject metaObject = metaObjectLookup[members[2]];

					if(members.Length > 4)
					{
						metaObject._parent = metaObjectLookup[members[3]];
						metaObject.InheritFields();
						int attrIndex = 4;
						if(metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList" || metaObject._parent._name == "igHashTable")
						{
							attrIndex++;
						}
						int attrCount = int.Parse(members[attrIndex], System.Globalization.NumberStyles.HexNumber);
						attrIndex++;
						metaObject._attributes = new igObjectList();
						metaObject._attributes.SetCapacity(attrCount);
						for(int a = 0; a < attrCount; a++)
						{
							metaObject._attributes.Append(ReadFieldAttribute(ref attrIndex, members));
						}
					}

					while(true)
					{
						string memberLine = sh.ReadLine();
						string[] memberInfo = memberLine.Split(' ');

						if(memberInfo[0] == "igMetaEndObject") break;
						
						int dataIndex = 0;
						int parentFieldIndex = -1;
						if(memberInfo[dataIndex] == "p")
						{
							parentFieldIndex = int.Parse(memberInfo[++dataIndex].Substring(2), System.Globalization.NumberStyles.HexNumber);
							dataIndex++;
						}
						igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex, metaObject);
						metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
						metaField._fieldName = memberInfo[dataIndex++];
						if(metaField._fieldName == "0") metaField._fieldName = null;
						ReadFieldAttributes(metaField, ref dataIndex, memberInfo);
						ReadFieldDefault(metaField, ref dataIndex, memberInfo);
						if(parentFieldIndex < 0)
						{
							metaObject._metaFields.Add(metaField);
						}
						else
						{
							replacedFields.Add((parentFieldIndex, metaField));
						}
					}

					if(members.Length > 4)
					{
						for(int j = 0; j < replacedFields.Count; j++)
						{
							metaObject.ValidateAndSetField(replacedFields[j].Item1, replacedFields[j].Item2);
						}
						int parentMetaIndex = 4;
						if(metaObject._parent._name == "igObjectList" || metaObject._parent._name == "igNonRefCountedObjectList")
						{
							igMemoryRefMetaField _data = (igMemoryRefMetaField)metaObject._metaFields[2].CreateFieldCopy();
							((igObjectRefMetaField)_data._memType)._metaObject = metaObjectLookup[members[parentMetaIndex]];
							metaObject.ValidateAndSetField(2, _data);
						}
						else if(metaObject._parent._name == "igHashTable")
						{
							igMemoryRefMetaField _keys = (igMemoryRefMetaField)metaObject._metaFields[1];
							if(_keys._memType is igEnumMetaField emf)
							{
								emf._default = int.Parse(members[parentMetaIndex]);
							}
						}
					}
					previousMeta = metaObject._name;
				}
				else if(members[0] == "igCompoundField")
				{
					igCompoundMetaFieldInfo compoundInfo = compoundInfoLookup[previousMeta];
					while(true)
					{
						string memberLine = sh.ReadLine();
						string[] memberInfo = memberLine.Split(' ');

						if(memberInfo[0] == "igEndCompoundField") break;
						
						int dataIndex = 0;
						igMetaField metaField = ReadFieldType(memberInfo, ref dataIndex, compoundInfo);
						metaField._offset = ushort.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
						metaField._fieldName = memberInfo[dataIndex++];
						if(metaField._fieldName == "0") metaField._fieldName = null;
						ReadFieldAttributes(metaField, ref dataIndex, memberInfo);
						ReadFieldDefault(metaField, ref dataIndex, memberInfo);
						compoundInfo._fieldList.Add(metaField);
					}
				}
				else break;
			}

			sh.Close();
			sh.Dispose();

			foreach(KeyValuePair<string, igMetaObject> kvp in metaObjectLookup)
			{
				kvp.Value.AppendToArkCore();
			}
			foreach(KeyValuePair<string, igCompoundMetaFieldInfo> kvp in compoundInfoLookup)
			{
				igArkCore.AddCompoundMeta(kvp.Value);
			}
		}

		private igMetaField ReadFieldType(string[] data, ref int index, igBaseMeta? meta = null)
		{
			List<igMetaField> templateArgs = new List<igMetaField>();
			igMetaField? metaField = null;

			string typeName = data[index++];
			string properties = data[index++];
			int templateCount = int.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);

			for(int i = 0; i < templateCount; i++)
			{
				if(data[index] == "0")
				{
					index++;
					templateArgs.Add(null);
				}
				else
				{
					templateArgs.Add(ReadFieldType(data, ref index));
				}
			}

			Type? t = igArkCore.GetObjectDotNetType(typeName);
			if(t == null)
			{
				t = typeof(igPlaceHolderMetaField);
			}
			metaField = (igMetaField)Activator.CreateInstance(t);
			metaField._parentMeta = meta;

			if(metaField is igRefMetaField refMetaField)
			{
				string refData = data[index++];
				refMetaField._construct = refData[0] == '1';
				refMetaField._destruct = refData[1] == '1';
				refMetaField._reconstruct = refData[2] == '1';
				refMetaField._refCounted = refData[3] == '1';
			}

			if(typeName.StartsWith("igMemoryRefHandle"))
			{
				(metaField as igMemoryRefHandleMetaField)._memType = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igMemoryRef"))
			{
				(metaField as igMemoryRefMetaField)._memType = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igObjectRef"))
			{
				(metaField as igObjectRefMetaField)._metaObject = metaObjectLookup[data[index++]];
			}
			else if(typeName.StartsWith("igHandle") && !typeName.StartsWith("igHandleName"))
			{
				(metaField as igHandleMetaField)._metaObject = metaObjectLookup[data[index++]];
			}
			else if(typeName.StartsWith("igStruct"))
			{
				(metaField as igStructMetaField)._sizes.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, ushort.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber));
				(metaField as igStructMetaField)._alignments.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, ushort.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber));
			}
			else if(typeName.StartsWith("igEnum"))
			{
				string enumName = data[index++];
				//metaField = new igEnumMetaField() { _metaEnumName = (enumName == "0" ? null : enumName) };
				(metaField as igEnumMetaField)._metaEnum = (enumName == "0" ? null : igArkCore.GetMetaEnum(enumName));
			}
			else if(typeName.StartsWith("igStatic"))
			{
				(metaField as igStaticMetaField)._storageMetaField = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igPropertyField"))
			{
				(metaField as igPropertyFieldMetaField)._innerMetaField = ReadFieldType(data, ref index);
			}
			else if(typeName.StartsWith("igBitField"))
			{
				igBitFieldMetaField bfMetaField = metaField as igBitFieldMetaField;
				bfMetaField._storageMetaField = bfMetaField._parentMeta.GetFieldByName(data[index++]);
				igMetaField assignmentMetaField = ReadFieldType(data, ref index);
				uint shift = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				uint bits = uint.Parse(data[index++].Substring(2), System.Globalization.NumberStyles.HexNumber);
				bfMetaField._assignmentMetaField = assignmentMetaField;
				bfMetaField._shift = shift;
				bfMetaField._bits = bits;
			}

			if(metaField is igPlaceHolderMetaField placeholder)
			{
				placeholder._platformInfo = igArkCore.GetMetaFieldPlatformInfo(typeName);
			}

			metaField._properties._copyMethod = (igMetaField.CopyType)int.Parse(properties.Substring(0, 1));
			metaField._properties._resetMethod = (igMetaField.ResetType)int.Parse(properties.Substring(1, 1));
			metaField._properties._isAlikeCompareMethod = (igMetaField.IsAlikeCompareType)int.Parse(properties.Substring(2, 1));
			metaField._properties._itemsCopyMethod = (igMetaField.CopyType)int.Parse(properties.Substring(3, 1));
			metaField._properties._keysCopyMethod = (igMetaField.CopyType)int.Parse(properties.Substring(4, 1));
			metaField._properties._persistent = properties[5] != '0';
			metaField._properties._hasInvariance = properties[6] != '0';
			metaField._properties._hasPoolName = properties[7] != '0';
			metaField._properties._mutable = properties[8] != '0';
			metaField._properties._implicitAlignment = properties[9] != '0';

			metaField.SetTemplateParameterCount((uint)templateCount);
			for(int i= 0; i < templateCount; i++)
			{
				metaField.SetTemplateParameter((uint)i, templateArgs[i]);
			}

			if(typeName.EndsWith("ArrayMetaField"))
			{
				string possibleArray = data[index];
				if(possibleArray.StartsWith("["))
				{
					FieldInfo? num = metaField.GetType().GetField("_num");
					num.SetValue(metaField, short.Parse(data[index++].Substring(3, 8), System.Globalization.NumberStyles.HexNumber));
				}
			}

			return metaField;
		}
		private void ReadFieldAttributes(igMetaField field, ref int dataIndex, string[] memberInfo)
		{
			if(memberInfo.Length <= dataIndex) return;

			int attrCount = int.Parse(memberInfo[dataIndex++].Substring(2), System.Globalization.NumberStyles.HexNumber);
			field._attributes.SetCapacity(attrCount);
			if(attrCount == 0) return;
			for(uint i = 0; i < attrCount; i++)
			{
				igObject? attr = ReadFieldAttribute(ref dataIndex, memberInfo);
				if(attr != null)
					field._attributes.Append(attr);
			}
		}
		private igObject? ReadFieldAttribute(ref int dataIndex, string[] memberInfo)
		{
			if(memberInfo.Length <= dataIndex) return null;

			string typeName = memberInfo[dataIndex++];
			string rawdata = memberInfo[dataIndex++];

			if(rawdata == "MISSING_ATTR") return null;

			Type? t = igArkCore.GetObjectDotNetType(typeName);
			if(t == null) return null;

			FieldInfo? valueField = t.GetField("_value");
			object attrValue = null;
			     if(valueField.FieldType == typeof(bool))             attrValue = rawdata == "1";
			else if(valueField.FieldType == typeof(int))              attrValue = int.Parse(rawdata, System.Globalization.NumberStyles.HexNumber);
			else if(valueField.FieldType == typeof(igMetaObject))     attrValue = metaObjectLookup[rawdata];
			else if(valueField.FieldType == typeof(string))
			{
				attrValue = System.Text.Encoding.ASCII.GetString(Convert.FromHexString(rawdata));
			}
			else return null;

			igObject? attr = (igObject?)Activator.CreateInstance(t);
			if(attr == null) return null;

			valueField.SetValue(attr, attrValue);

			return attr;
		}
		private void ReadFieldDefault(igMetaField field, ref int dataIndex, string[] memberInfo)
		{
			if(memberInfo.Length <= dataIndex) return;

			string typeName = field.GetType().Name;
			string[] defaultInfo = memberInfo[dataIndex].Split(',');

			     if(typeName.StartsWith("igIntPtr"))			field._default = long.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igInt"))				field._default = int.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igEnum"))				field._default = int.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedIntPtr"))	field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedInt"))		field._default = uint.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igLong"))				field._default = long.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedLong"))		field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igShort"))				field._default = short.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedShort"))		field._default = ushort.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igSizeType"))			field._default = ulong.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igBool"))				field._default = byte.Parse(memberInfo[dataIndex]) == 1;
			else if(typeName.StartsWith("igChar"))				field._default = sbyte.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igUnsignedChar"))		field._default = byte.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igFloat"))				field._default = float.Parse(memberInfo[dataIndex]);
			else if(typeName.StartsWith("igVec2f"))				field._default = new igLibrary.Math.igVec2f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]));
			else if(typeName.StartsWith("igVec2uc"))			field._default = new igLibrary.Math.igVec2uc(byte.Parse(defaultInfo[0]), byte.Parse(defaultInfo[1]));
			else if(typeName.StartsWith("igVec3fAligned"))		field._default = new igLibrary.Math.igVec3fAligned(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec3f"))				field._default = new igLibrary.Math.igVec3f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec3d"))				field._default = new igLibrary.Math.igVec3d(double.Parse(defaultInfo[0]), double.Parse(defaultInfo[1]), double.Parse(defaultInfo[2]));
			else if(typeName.StartsWith("igVec4fUnaligned"))	field._default = new igLibrary.Math.igVec4fUnaligned(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4f"))				field._default = new igLibrary.Math.igVec4f(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igQuaternionf"))		field._default = new igLibrary.Math.igQuaternionf(float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4uc"))			field._default = new igLibrary.Math.igVec4uc(byte.Parse(defaultInfo[0]), byte.Parse(defaultInfo[1]), byte.Parse(defaultInfo[2]), byte.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igVec4i"))				field._default = new igLibrary.Math.igVec4i(int.Parse(defaultInfo[0]), int.Parse(defaultInfo[1]), int.Parse(defaultInfo[2]), int.Parse(defaultInfo[3]));
			else if(typeName.StartsWith("igMatrix44f"))			field._default = new igLibrary.Math.igMatrix44f(new float[16] {float.Parse(defaultInfo[0]), float.Parse(defaultInfo[1]), float.Parse(defaultInfo[2]), float.Parse(defaultInfo[3]), float.Parse(defaultInfo[4]), float.Parse(defaultInfo[5]), float.Parse(defaultInfo[6]), float.Parse(defaultInfo[7]), float.Parse(defaultInfo[8]), float.Parse(defaultInfo[9]), float.Parse(defaultInfo[10]), float.Parse(defaultInfo[11]), float.Parse(defaultInfo[12]), float.Parse(defaultInfo[13]), float.Parse(defaultInfo[14]), float.Parse(defaultInfo[15])});
			else if(typeName.StartsWith("igObjectRef"))
			{
				igObjectRefMetaField objField = (igObjectRefMetaField)field;
				if(objField._metaObject._name == "igMetaObject")
				{
					string hexString = memberInfo[dataIndex];
					field._default = metaObjectLookup[System.Text.Encoding.ASCII.GetString(Convert.FromHexString(hexString))];
				}
			}
			else if(typeName.StartsWith("igString"))
			{
				string hexString = memberInfo[dataIndex];
				field._default = System.Text.Encoding.ASCII.GetString(Convert.FromHexString(hexString));
			}
			else if(typeName.StartsWith("igBitField"))
			{
				igBitFieldMetaField bfmf = (igBitFieldMetaField)field;
				ReadFieldDefault(bfmf._assignmentMetaField, ref dataIndex, memberInfo);
				bfmf._default = bfmf._assignmentMetaField._default;
				bfmf._assignmentMetaField._default = null;
			}
		}
	}
}