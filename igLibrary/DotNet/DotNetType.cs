/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igLibrary.DotNet
{
	public struct DotNetType
	{
		public igBaseMeta? _baseMeta;
		public uint _flags;		//Consider making this private

		public ElementType _elementType {
			get => (ElementType)(_flags & (uint)Flags.kTypeMask);
			set => _flags = (_flags & ~(uint)Flags.kTypeMask) | (uint)value;
		}
		public bool _isSimple {
			get => (_flags & (uint)Flags.kIsSimple) != 0;
			set => _flags = (_flags & ~(uint)Flags.kIsSimple) | (value ? 0u : 1u);
		}
		public bool _isArray {
			get => (_flags & (uint)Flags.kIsArray) != 0;
			set => _flags = (_flags & ~(uint)Flags.kIsArray) | (value ? 0u : 1u);
		}
		public enum Flags : uint
		{
			kIsSimple = 0x40000000,
			kIsArray  = 0x80000000,
			kTypeMask = 0x000000FF,
		}

		public DotNetType()
		{
			_baseMeta = null;
			_flags = (uint)ElementType.kElementTypeVoid;
		}
	}
	public class DotNetTypeList : igTDataList<DotNetType>{}
}