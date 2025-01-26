/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CLanguageFilePrecacher : CResourcePrecacher
	{
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _loadedCallback;         //igObjectDirectoryLoadCallback
		[Obsolete("This exists for the reflection system, do not use.")] public static object? _localizedNamespaceData; //igStringObjectDirectoryHashTable
		public override void Precache(string filePath)
		{
			//Unimplemented
			igObjectStreamManager.Singleton.Load(filePath);
		}
	}
}