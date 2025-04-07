/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igRuntimeFields
	{
		public List<ulong> _stringRefs = new List<ulong>();
		public List<ulong> _stringTables = new List<ulong>();
		public List<ulong> _offsets = new List<ulong>();
		public List<ulong> _vtables = new List<ulong>();
		public List<ulong> _poolIds = new List<ulong>();
		public List<ulong> _handles = new List<ulong>();
		public List<ulong> _namedExternals = new List<ulong>();
		public List<ulong> _externals = new List<ulong>();
		public List<ulong> _memoryHandles = new List<ulong>();
		public List<ulong> _objectLists = new List<ulong>();
	}
}