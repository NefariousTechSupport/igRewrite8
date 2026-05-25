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
	public struct igBlendStateBundleDesc
	{
		public bool _blendEnabled;
		public bool _alphaToCoverageEnable;
		public bool _independentBlendEnable;
		public IG_GFX_BLENDING_FUNCTION _srcBlend;
		public IG_GFX_BLENDING_FUNCTION _dstBlend;
		public IG_GFX_BLENDING_EQUATION _blendOp;
		public IG_GFX_BLENDING_FUNCTION _srcBlendAlpha;
		public IG_GFX_BLENDING_FUNCTION _dstBlendAlpha;
		public IG_GFX_BLENDING_EQUATION _blendOpAlpha;
		public uint _hash;
		public bool _hashDirty;
	}
}
