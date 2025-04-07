/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetRuntime : igObject
	{
		//Commented out ones aren't useful for us

		[Obsolete("This exists for the reflection system, do not use.")] public object? _debugger;             //igDotNetDebugger
		public string _prefix;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _allocatedThreads;     //DotNetThreadList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _traceActive;          //bool
		[Obsolete("This exists for the reflection system, do not use.")] public object? _trace;                //igVector<uint>
		[Obsolete("This exists for the reflection system, do not use.")] public object? _traceHead;            //int
		[Obsolete("This exists for the reflection system, do not use.")] public object? _ignoreExceptions;     //igStringIntHashTable
		[Obsolete("This exists for the reflection system, do not use.")] public object? _exceptionScopes;      //igStringIntHashTable
		public igDotNetMetaInterface _methodLookup;
	}
}