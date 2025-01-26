/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igIndexBuffer : igObject
	{
		public uint _indexCount;
		public igMemory<uint> _indexCountArray;
		public igMemory<byte> _data;
		public igIndexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igVertexFormat _vertexFormat;
		public igIndexArray2 _indexArray;
		public int _indexArrayRefCount;
	}
}