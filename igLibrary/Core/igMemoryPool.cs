/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igMemoryPool : igObject
	{
		public bool _lockOnOperation;
		public bool _active;
		public int _poolIndex;
		public ulong _address;
		public ulong _size;
		public uint _alignment;
		public igObject _lock;	//igMutex
		public bool _isThreadSafe;
		public igObject _owner;	//igThread
		public bool _reportThreadSafety;
		public bool _alchemyMemory;
		public bool _reportOnFailure;
		public bool _useSentinels;
		public bool _fillMemory;
		public bool _checkIntegrity;
		public ulong _blocksAllocated;
		public ulong _peakBlocksAllocated;
		public ulong _userAllocated;
		public ulong _peakUserAllocated;
		public ulong _totalAllocated;
		public ulong _peakTotalAllocated;
		public int _ordinal;
		public string _name;
		public igMemoryPool _parentPool;
		public ulong _largestFreeBlockSizeMinimum;
		public bool _initializedForTag;
		public igMemoryPool(){}
		public igMemoryPool(string name)
		{
			_name = name;
		}
	}
}