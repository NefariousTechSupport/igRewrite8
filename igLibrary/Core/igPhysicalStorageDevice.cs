/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public abstract class igPhysicalStorageDevice : igStorageDevice
	{
		public int _maxOperationsInFlight;
		public int _threadStackSize;
		public bool _scheduleReads;
		public ulong _mediaPosition;
		public float _emulatedMediaBandwidth;
		public float _emulatedMediaSeekTime;
		public igFileWorkItemList _stagingWorkItems;
		public int _stagingBufferSize;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _writeCountStat;      //igStatHandle
		[Obsolete("This exists for the reflection system, do not use.")] public object? _readCountStat;       //igStatHandle
		[Obsolete("This exists for the reflection system, do not use.")] public object? _readSizeStat;        //igStatHandle
		[Obsolete("This exists for the reflection system, do not use.")] public object? _readBandwidthStat;   //igStatHandle
		[Obsolete("This exists for the reflection system, do not use.")] public object? _seekCountStat;       //igStatHandle
	}
}