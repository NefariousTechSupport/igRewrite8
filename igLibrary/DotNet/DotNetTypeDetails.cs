/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetTypeDetails : igObject
	{
		public igDotNetTypeReference _baseType;
		public int _interfaceOffset;
		public int _interfaceCount;
		public int _templateParameterOffset;
		public int _templateParameterCount;
		public igBaseMeta _targetMeta;
		public bool _ownsMeta = true; 
	}
	public class DotNetTypeDetailsList : igTObjectList<DotNetTypeDetails>{}
}