/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igDirectory : igTObjectList<igDirEntry>
    {
		public string _name;
		public igDirectoryList _externalDirectoryList;
		public igInfoList _infoList;
		public igResource _managingResource;
		public ulong _shouldInternalizeHandleCallback;
		public bool _loadPendingState;
		public bool _validState;
		public bool _compatibleState;
		public int _loadedRefCount;
		public igShortListList _metaFieldPerObjectIndices;
		public bool _concreteState;
		public bool _sharingState;
		public bool _trackUniqueEntries;
		public igDirectory _uniqueEntryList;
		public igRawRefList _refList;
		public igIntList _alignmentList;
		public bool _autoWriteFilePreProcessState;
		public bool _useMemoryPoolAssignmentsState;
		public bool _forceWriteMemoryPoolInfoFromMetaState;
		public igStringRefList _stringTable;
		public bool _stringRefCompatibilityMode;
		public igReferenceResolverSet _referenceResolverSet;
		public igReferenceResolverSet _globalReferenceResolverSet;
		public igReferenceResolverContext _resolverContext;
		public igObjectDirectory _objectDirectory;
		public igVector<igObjectDirectory> _dependencies;
		public igUnsignedIntStringHashTable _handleNames;
		public igVector<uint> _memoryPoolIndices;
	}
	public class igDirectoryList : igTObjectList<igDirectory> {}
}