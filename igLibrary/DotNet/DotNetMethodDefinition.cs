/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetMethodDefinition : DotNetMethodSignature
	{
		public object _method;
		public DotNetTypeList _locals = new DotNetTypeList();
		public DotNetMethodDefinition _next;
		public DotNetType _declaringType;
		public igVector<byte> _IL;
		public int _stackHeight;
		public int _methodIndex;
		public DotNetLibrary _owner;
		public string _name;
	}
	public class DotNetMethodDefinitionList : igTObjectList<DotNetMethodDefinition>{}
}