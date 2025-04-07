/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

namespace igLibrary.Core
{
	public class igRegistry : igObject
	{
		public IG_CORE_PLATFORM _platform;
		public IG_GFX_PLATFORM _gfxPlatform;
		private static igRegistry _instance;
		[Obsolete("This exists for the reflection system, do not use.")] public string _rootElementName;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _document;          //igXmlDocument
		public static igRegistry GetRegistry()
		{
			if(_instance == null) _instance = new igRegistry();
			return _instance;
		}
	}
}