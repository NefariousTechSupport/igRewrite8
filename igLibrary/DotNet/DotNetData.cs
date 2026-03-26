/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public struct DotNetData
	{
		public object? _data;
		public DotNetType _type;
		public DataRepresentation _representation;
		public uint _maybeRepresentation;

		public enum DataRepresentation
		{
			Normal = 0,
			Complex = 1,
			Indirect = 2,
			RawIndirect = 4,
			FieldReference = 8,
		}

		public DotNetData()
		{
			_data = null;
			_type = new DotNetType();
			_representation = DataRepresentation.Normal;
			_maybeRepresentation = 0;
		}

		public void Reset()
		{
			Type? vTablePointer;
			if (_type._baseMeta is igMetaEnum metaEnum)
			{
				vTablePointer = metaEnum._internalType;
			}
			else if (_type._baseMeta is igMetaObject metaObject)
			{
				if (metaObject._vTablePointer == null)
				{
					metaObject.GatherDependancies();
					igArkCore.FlushPendingTypes();
				}
				vTablePointer = metaObject._vTablePointer!;
			}
			else
			{
				_type._baseMeta = igArkCore.GetObjectMeta(nameof(igObject));
				vTablePointer = typeof(igObject);
			}

			if (_type._isArray)
			{
				switch (_type._elementType)
				{
					case ElementType.kElementTypeEnd:
					case ElementType.kElementTypeVoid:
						_data = null;
						break;
					case ElementType.kElementTypeBoolean:
						_data = new bool[0];
						break;
					case ElementType.kElementTypeChar:
						_data = new char[0];
						break;
					case ElementType.kElementTypeI1:
						_data = new sbyte[0];
						break;
					case ElementType.kElementTypeU1:
						_data = new byte[0];
						break;
					case ElementType.kElementTypeI2:
						_data = new short[0];
						break;
					case ElementType.kElementTypeU2:
						_data = new ushort[0];
						break;
					case ElementType.kElementTypeI4:
						_data = new int[0];
						break;
					case ElementType.kElementTypeU4:
						_data = new uint[0];
						break;
					case ElementType.kElementTypeI8:
						_data = new long[0];
						break;
					case ElementType.kElementTypeU8:
						_data = new ulong[0];
						break;
					case ElementType.kElementTypeR4:
						_data = new float[0];
						break;
					case ElementType.kElementTypeString:
						_data = new string[0];
						break;
					case ElementType.kElementTypeValueType:
					case ElementType.kElementTypeClass:
					case ElementType.kElementTypeObject:
						_data = Array.CreateInstance(vTablePointer, 0);
						break;
				}
			}

			switch (_type._elementType)
			{
				case ElementType.kElementTypeEnd:
				case ElementType.kElementTypeVoid:
					_data = null;
					break;
				case ElementType.kElementTypeBoolean:
					_data = default(bool);
					break;
				case ElementType.kElementTypeChar:
					_data = default(char);
					break;
				case ElementType.kElementTypeI1:
					_data = default(sbyte);
					break;
				case ElementType.kElementTypeU1:
					_data = default(byte);
					break;
				case ElementType.kElementTypeI2:
					_data = default(short);
					break;
				case ElementType.kElementTypeU2:
					_data = default(ushort);
					break;
				case ElementType.kElementTypeI4:
					_data = default(int);
					break;
				case ElementType.kElementTypeU4:
					_data = default(uint);
					break;
				case ElementType.kElementTypeI8:
					_data = default(long);
					break;
				case ElementType.kElementTypeU8:
					_data = default(ulong);
					break;
				case ElementType.kElementTypeR4:
					_data = default(float);
					break;
				case ElementType.kElementTypeString:
					_data = string.Empty;
					break;
				case ElementType.kElementTypeValueType:
					_data = Enum.ToObject(vTablePointer, 0);
					break;
				case ElementType.kElementTypeClass:
				case ElementType.kElementTypeObject:
					_data = null;
					break;
			}
		}
	}
}