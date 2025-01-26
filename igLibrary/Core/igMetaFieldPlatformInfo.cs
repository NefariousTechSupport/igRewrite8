/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igMetaFieldPlatformInfo
	{
		public string _name;
		public Dictionary<IG_CORE_PLATFORM, ushort> _sizes = new Dictionary<IG_CORE_PLATFORM, ushort>();
		public Dictionary<IG_CORE_PLATFORM, ushort> _alignments = new Dictionary<IG_CORE_PLATFORM, ushort>();
	}
}