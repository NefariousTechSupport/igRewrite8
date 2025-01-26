/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetMethodMeta : igObject
	{
		public string _importTag;
		public string _methodName;
		public string _entryPoint;
		public DotNetParameterMeta _return;
		public DotNetParameterMetaList _parameters = new DotNetParameterMetaList();
	}
	public class DotNetMethodMetaList : igTObjectList<DotNetMethodMeta>{}
}