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
	public struct igStencilStateBundleDesc
	{
		public bool _enabled;
		public IG_GFX_STENCIL_FUNCTION _frontFunc;
		public IG_GFX_STENCIL_OPERATION _frontFailOp;
		public IG_GFX_STENCIL_OPERATION _frontZPassOp;
		public IG_GFX_STENCIL_OPERATION _frontZFailOp;
		public IG_GFX_STENCIL_FUNCTION _backFunc;
		public IG_GFX_STENCIL_OPERATION _backFailOp;
		public IG_GFX_STENCIL_OPERATION _backZPassOp;
		public IG_GFX_STENCIL_OPERATION _backZFailOp;
		public uint _writeMask;
		public uint _readMask;
		public uint _hash;
		public bool _hashDirty;


		public igStencilStateBundleDesc()
		{
			_enabled = default;
			_frontFunc = IG_GFX_STENCIL_FUNCTION.IG_GFX_STENCIL_FUNCTION_ALWAYS;
			_frontFailOp = default;
			_frontZPassOp = default;
			_frontZFailOp = default;
			_backFunc = IG_GFX_STENCIL_FUNCTION.IG_GFX_STENCIL_FUNCTION_ALWAYS;
			_backFailOp = default;
			_backZPassOp = default;
			_backZFailOp = default;
			_writeMask = 0xFFFFFFFF;
			_readMask = 0xFFFFFFFF;
			_hash = default;
			_hashDirty = default;
		}
	}
}
