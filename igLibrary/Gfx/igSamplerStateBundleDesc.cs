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
		IG_GFX_TEXTURE_FILTER _minFilter;
		IG_GFX_TEXTURE_FILTER _magFilter;
		IG_GFX_TEXTURE_WRAP _addressU;
		IG_GFX_TEXTURE_WRAP _addressV;
		IG_GFX_TEXTURE_WRAP _addressW;
		uint _maxAnisotropy;
		igComparisonFunction _comparisonFunc;
		bool _comparisonSampler;
		uint _hash;
		bool _hashDirty;
	}
}
