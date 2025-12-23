/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

namespace igLibrary.Sg
{
	public class igTechniqueSampler : igNamedObject
	{
		public int _unitID;
		public IG_GFX_TEXTURE_FILTER _magFilter;
		public IG_GFX_TEXTURE_FILTER _minFilter;
		public IG_GFX_TEXTURE_WRAP _wrapS;
		public IG_GFX_TEXTURE_WRAP _wrapT;
		public uint _formatHint;
		public bool _vertexSampler;
		public bool _useMaterialSamplerState;
	}

	public class igTechniqueSamplerList : igTObjectList<igTechniqueSampler>
	{
	}
}