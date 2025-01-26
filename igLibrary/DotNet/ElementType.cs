/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public enum ElementType
	{
		kElementTypeEnd = 0,
		kElementTypeVoid = 1,
		kElementTypeBoolean = 2,
		kElementTypeChar = 3,
		kElementTypeI1 = 4,
		kElementTypeU1 = 5,
		kElementTypeI2 = 6,
		kElementTypeU2 = 7,
		kElementTypeI4 = 8,
		kElementTypeU4 = 9,
		kElementTypeI8 = 10,
		kElementTypeU8 = 11,
		kElementTypeR4 = 12,
		kElementTypeR8 = 13,
		kElementTypeString = 14,
		kElementTypePtr = 15,
		kElementTypeByRef = 16,
		kElementTypeValueType = 17,
		kElementTypeClass = 18,
		kElementTypeVar = 19,
		kElementTypeArray = 20,
		kElementTypeGenericInst = 21,
		kElementTypeTypedByRef = 22,
		kElementTypeReserved_17 = 23,
		kElementTypeI = 24,
		kElementTypeU = 25,
		kElementTypeReserved_1A = 26,
		kElementTypeFnPtr = 27,
		kElementTypeObject = 28,
		kElementTypeSzArray = 29,
		kElementTypeMVar = 30,
		kElementTypeCModReqd = 31,
		kElementTypeCModOpt = 32,
		kElementTypeInternal = 33,
		kElementTypeModifier = 64,
		kElementTypeSentinel = 65,
		kElementTypePinned = 69,
		kElementTypeSystemType = 80,
		kElementTypeBoxed = 81,
		kElementTypeEnum = 85,
		kElementTypeReserved = 86,
		kElementTypeCustomAttribute_Field = 87,
		kElementTypeCustomAttribute_Property = 88,
		kElementTypeCustomAttribute_Enum = 89,
		kElementTypeYield = 90,
		kElementTypeCall = 91,
		kElementTypeCallConstructor = 92,
	}
}