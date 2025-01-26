/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetMethodSignature : igObject
	{
		public uint _flags;
		public DotNetType _retType;
		public DotNetTypeList _parameters = new DotNetTypeList();
		public DotNetMethodMeta _methodMeta = new DotNetMethodMeta();

		public bool isStatic => (_flags & (uint)FlagTypes.StaticMethod) != 0;
		public bool isConstructor => (_flags & (uint)FlagTypes.Constructor) != 0;
		public bool isAbstract => (_flags & (uint)FlagTypes.AbstractMethod) != 0;
		public bool isRuntimeImpl => (_flags & (uint)FlagTypes.RuntimeImplMethod) != 0;
		public bool IsNoSpecializationCopy => (_flags & (uint)FlagTypes.NoSpecializationCopyMethod) != 0;

		public enum FlagTypes
		{
			StaticMethod = 0x04,
			Constructor = 0x08,
			AbstractMethod = 0x20,
			RuntimeImplMethod = 0x40,
			NoSpecializationCopyMethod = 0x80
		}
	}
}