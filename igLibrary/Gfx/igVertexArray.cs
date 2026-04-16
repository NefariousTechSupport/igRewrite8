/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igVertexArray : igBaseVertexArray
	{
		public igRawRefMetaField _attr;
		public igRawRefMetaField _descDirect;
		public igRawRefMetaField _descIndex8;
		public igRawRefMetaField _descIndex16;
        public ulong _ps3Elements;
		public uint _streamOffset;
		public uint _location;
	}
}