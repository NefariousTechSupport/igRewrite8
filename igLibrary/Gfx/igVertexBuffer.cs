/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igVertexBuffer : igObject
	{
		public uint _vertexCount;
		public igMemory<uint> _vertexCountArray;
		public igMemory<byte> _data;
		public igVertexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igMemory<byte> _packData;
		public igVertexArray _vertexArray;
		public int _vertexArrayRefCount;
	}
}