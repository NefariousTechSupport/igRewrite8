/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//I didn't know this class existed when I named this lib
	public class igLibrary : igNamedObject
	{
		public object? _registerFunction;
		public object? _registerEnumsFunction;
		public object? _registerFunctionParametersFunction;
		public object? _libraryFunction;
		public int _version;
		public bool _codeCanBeUnloaded;
	}
}