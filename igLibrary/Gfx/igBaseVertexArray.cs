/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igBaseVertexArray : igObject
	{
		public uint _vertexCount;
		public ulong _vertexCountArray;
		public uint _vertexCounts;
		public igVertexFormat _format;
		public IG_GFX_DRAW _primitiveType;
		public igMemory<byte> _packData;
		public uint _size;
		public igVertexBuffer _buffer;
		public ulong _platformBuffer;
		public igVertexArray _softwareBlendedArray;
		public uint _softwareBlendedSequenceId;
		public bool _skinned;
		public bool _transient;
		public uint _cacheFlushSequenceId;
		public bool _discardGeometry;
	}
}