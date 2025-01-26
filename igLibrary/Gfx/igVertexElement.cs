/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	[igStruct]
	public struct igVertexElement
	{
		public byte _type;
		public byte _stream;
		public byte _mapToElement;
		public byte _count;
		public byte _usage;
		public byte _usageIndex;
		public byte _packDataOffset;
		public byte _packTypeAndFracHint;
		public ushort _offset;
		public ushort _freq;
	}
}