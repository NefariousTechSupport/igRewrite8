/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igFileCache : igFileWorkItemProcessor
	{
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _lruMap;        //igIntIntHashTable
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _lockCountMap;  //igIntIntHashTable
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _hashMap;       //igIntIntHashTable
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _mapLock;              //igSpinLock
		[Obsolete("This exists for the reflection system, do not use.")] public int _lruCounter;
		[Obsolete("This exists for the reflection system, do not use.")] public int _fileCountMax = 128;
		[Obsolete("This exists for the reflection system, do not use.")] public igStorageDevice _device;
		[Obsolete("This exists for the reflection system, do not use.")] public igMemoryPool _ramCachePool;
		[Obsolete("This exists for the reflection system, do not use.")] public ulong _cacheTotalSize;
		[Obsolete("This exists for the reflection system, do not use.")] public bool _corruptionCheck;
		[Obsolete("This exists for the reflection system, do not use.")] public igMemory<byte> _fetchBuffer;
		[Obsolete("This exists for the reflection system, do not use.")] public igObject _workingFolder;          //igFolder
		[Obsolete("This exists for the reflection system, do not use.")] public igFile _sourceFile;
		[Obsolete("This exists for the reflection system, do not use.")] public igFile _cacheFile;
		[Obsolete("This exists for the reflection system, do not use.")] public igFile _hashFile;
		[Obsolete("This exists for the reflection system, do not use.")] public int _fetchPauseFlag;
		public override void Process(igFileWorkItem workItem)
		{
			SendToNextProcessor(workItem);
		}
	}
}