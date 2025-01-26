/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igVertexFormat : igLibrary.Core.igObject
	{
		public uint _vertexSize;
		public igMemory<igVertexElement> _elements;
		public igMemory<byte> _platformData;
		public IG_GFX_PLATFORM _platform;
		public igVertexFormat _softwareBlendedFormat;
		public igVertexBlender _blender;
		public bool _dynamic;
		public igVertexFormatPlatform _platformFormat;
		public igMemory<igVertexStream> _streams;
		public uint _hashCode;
		public igVertexFormat _softwareBlendedMultistreamFormat;
		public bool _enableSoftwareBlending;
		public uint _cachedUsage;
	}
}