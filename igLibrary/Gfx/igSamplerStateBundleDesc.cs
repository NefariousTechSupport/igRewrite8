/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	[igStruct]
	public struct igSamplerStateBundleDesc
	{
		public IG_GFX_TEXTURE_FILTER _minFilter;
		public IG_GFX_TEXTURE_FILTER _magFilter;
		public IG_GFX_TEXTURE_WRAP _addressU;
		public IG_GFX_TEXTURE_WRAP _addressV;
		public IG_GFX_TEXTURE_WRAP _addressW;
		public uint _maxAnisotropy;
		public igComparisonFunction _comparisonFunc;
		public bool _comparisonSampler;
		public uint _hash;
		public bool _hashDirty;
	}
}
