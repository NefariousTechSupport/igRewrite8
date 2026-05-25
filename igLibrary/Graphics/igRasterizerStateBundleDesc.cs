/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

namespace igLibrary.Graphics
{
	[igStruct]
	public struct igRasterizerStateBundleDesc
	{
		public bool _cullingEnabled;
		public IG_GFX_CULL_FACE_MODE _cullMode;
		public IG_GFX_RENDER_MODE _fillMode;
		public float _depthOffset;
		public float _depthSlope;
		public uint _hash;
		public bool _hashDirty;
	}
}
