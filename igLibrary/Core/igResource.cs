/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igResource : igObject
	{
		public igDirectoryList _directoryList;
		public string _relativeFilePath;
		public string _absoluteFilePath;
		public bool _autoCompatibility;
		public bool _IGBSharingState;
		public int _IGBChunkSize;
		public bool _useMemoryPoolAssignmentsState;
		public igReferenceResolverSet _referenceResolverSet;
	}
}