/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Attrs;
using igLibrary.Gfx;

namespace igLibrary.Sg
{
	public class igCustomMaterial : igNamedObject
	{
		public igCustomMaterialAnimationList _transforms;
		public uint _customMaterialBitfield;
		public ushort _customMaterialBitfield2;
		public ushort _customMaterialPreparedBitfield;
		public float _alphaRefValue;
		public float _depthBias;
		public float _slopeScaleDepthBias;
		public int _sortKey;
		public igAttrList _renderState;
		public igVector<igCustomMaterial> _DirtyMaterials;
		public IG_GFX_ALPHA_FUNCTION _alphaFunction;
		public bool _alphaTestState;
		public IG_GFX_BLENDING_FUNCTION _blendingSource;
		public IG_GFX_BLENDING_FUNCTION _blendingDestination;
		public IG_GFX_BLENDING_EQUATION _blendingEquation;
		public bool _blendingState;
		public IG_GFX_CULL_FACE_MODE _cullFaceMode;
		public bool _cullFaceState;
		public bool _depthTestState;
		public bool _depthWriteState;
		public IG_GFX_TEXTURE_WRAP _wrapS;
		public IG_GFX_TEXTURE_WRAP _wrapT;
		public igCustomMaterialAnimationTimeSource _timeSource;
		public bool _prepareAffectsRenderState;
		public bool _dirty;
		public IG_GFX_TEXTURE_FILTER _magnificationFilter;
		public IG_GFX_TEXTURE_FILTER _minificationFilter;
		public bool _useDefaultAlphaFunctionAttr;
		public bool _useDefaultAlphaStateAttr;
		public bool _useDefaultBlendFunctionAttr;
		public bool _useDefaultBlendStateAttr;
		public bool _useDefaultCullFaceAttr;
		public bool _useDefaultDecalAttr;
		public bool _useDefaultDepthStateAttr;
		public bool _useDefaultDepthWriteStateAttr;
	}
}