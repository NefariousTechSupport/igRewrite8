/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Gfx;

//You'll see a lot of commented out igObjectRefMetaFields, the commented out ones are correct, but when the game decodes the stream it takes the raw value and looks it up in some sort of list

namespace igLibrary.Graphics
{

	[igStruct]
	public struct igCommandSetPrimitiveTypeParameters
	{
		public IG_GFX_DRAW _type;
	}
	[igStruct]
	public struct igCommandSetVertexBufferParameters
	{
		public igGraphicsObject? _resource;
		public igGraphicsObject? _format;
		public int _offset;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetIndexBufferParameters
	{
		public igGraphicsObject? _resource;
		public IG_INDEX_TYPE _format;
		public int _offset;
	}
	[igStruct]
	public struct igCommandSetVertexShaderParameters
	{
		public igGraphicsObject? _resource;
	}

	[igStruct]
	public struct igCommandSetVertexShaderVariantParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetVertexShaderTextureParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetVertexShaderSamplerParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetViewportParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetScissorParameters
	{
		public int _x;
		public int _y;
		public int _w;
		public int _h;
	}
	[igStruct]
	public struct igCommandSetScissorEnabledParameters
	{
		public bool _enabled;
	}
	[igStruct]
	public struct igCommandSetRasterizeStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetPixelShaderParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetPixelShaderVariantParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetPixelShaderTextureParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetPixelShaderSamplerParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetAlphaTestStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetBlendStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetDepthStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetStencilStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandSetStencilRefParameters
	{
		public uint _stencilRef;
	}
	[igStruct]
	public struct igCommandSetRenderTargetsParameters
	{
		public ulong[] _colorTargets;
		public uint _colorCount;
		public ulong _depthTarget;
	}
	[igStruct]
	public struct igCommandSetRenderTargetMaskParameters
	{
		public igGraphicsObject? _resource;
	}
	[igStruct]
	public struct igCommandXenonSetHiStencilParameters
	{
		public bool _state;
		public bool _writeState;
		public IG_GFX_HISTENCIL_FUNCTION _func;
		public uint _refValue;
	}
	[igStruct]
	public struct igCommandXenonSetFlushHiZStencilParameters
	{
		public bool _async;
	}
	[igStruct]
	public struct igCommandXenonSetGprCountsParameters
	{
		public uint _vertex;
		public uint _pixel;
	}
	[igStruct]
	public struct igCommandDrawEdgeGeometryParameters
	{
		public igGraphicsObject? _edgeGeometry;
		public ulong _modelMatrix;
		public ulong _morphTargetWeights;
		public byte _morphTargetCount;
		public ulong _blendVectors;
		public int _blendVectorCount;
		public bool _ignoreNearPlaneForCulling;
		public uint _cacheId;
		public bool _cacheResults;
	}
	[igStruct]
	public struct igCommandPS3SetSCullParameters
	{
		public IG_GFX_STENCIL_FUNCTION _function;
		public uint _refValue;
		public uint _mask;
	}
	[igStruct]
	public struct igCommandSetConstantBoolParameters
	{
		public igGraphicsObject? _resource;
		public bool _value;
	}
	[igStruct]
	public struct igCommandSetConstantIntParameters
	{
		public igGraphicsObject? _resource;
		public int _value;
	}
	[igStruct]
	public struct igCommandSetConstantFloatParameters
	{
		public igGraphicsObject? _resource;
		public float _value;
	}
	[igStruct]
	public struct igCommandSetConstantVec4fParameters
	{
		public igGraphicsObject? _resource;
		public igVec4f _value;
	}
	[igStruct]
	public struct igCommandSetConstantMatrix44fParameters
	{
		public igGraphicsObject? _resource;
		public igMatrix44f _value;
	}
	[igStruct]
	public struct igCommandSetConstantArrayIntParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	[igStruct]
	public struct igCommandSetConstantArrayFloatParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	[igStruct]
	public struct igCommandSetConstantArrayVec4fParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	[igStruct]
	public struct igCommandSetConstantArrayMatrix44fParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	[igStruct]
	public struct igCommandApplyConstantBundleParameters
	{
		public igGraphicsObject? _bundle;
	}
	[igStruct]
	public struct igCommandApplyConstantValueListParameters
	{
		public igGraphicsObject? _list;
	}
	[igStruct]
	public struct igCommandSetPixelShaderTextureEnabledConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetVertexShaderTextureEnabledConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetPixelShaderTextureSizeConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandSetVertexShaderTextureSizeConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandClearRenderTargetParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	[igStruct]
	public struct igCommandDrawPrimitivesParameters
	{
		public IG_GFX_DRAW _primitive;
		public int _numPrimitives;
		public int _offset;
	}
	[igStruct]
	public struct igCommandDecodeMemoryCommandStreamParameters
	{
		public igGraphicsObject? _stream;
	}
	[igStruct]
	public struct igCommandCopyTextureParameters
	{
		public igGraphicsObject? _source;
		public igGraphicsObject? _destination;
		public igCopyTextureParameters _params;
	}
	[igStruct]
	public struct igCommandUpdateTextureParameters
	{
		public ulong _texture;
		public ulong _data;
		public uint _size;
		public uint _imageIndex;
		public uint _mipLevel;
		public uint _flags;
	}
	[igStruct]
	public struct igCommandExecuteCallbackParameters
	{
		//It's a struct called _callback
	}
	[igStruct]
	public struct igCommandSetCameraMatricesParameters
	{
		public byte _cameraIndex;
		public byte _viewMatrix;
		public byte _previousViewMatrix;
		public byte _projMatrix;
	}
	[igStruct]
	public struct igCommandComputeAndSetInstanceMatricesParameters
	{
		public ulong _modelMatrix;
		public ulong _prevModelMatrix;
		public ushort _matrixConstants;
		public byte _cameraIndex;
	}
	[igStruct]
	public struct igCommandComputeAndSetInstanceConstantsParameters
	{
		public byte _effectFlags;
		public byte _geometryFlags;
	}
	[igStruct]
	public struct igCommandSetCommonRenderStateParameters
	{
		public ushort _commonRenderState;
	}
	[igStruct]
	public struct igCommandSetDitherStateParameters
	{
		public bool _enabled;
		public float _ditherOpacity;
	}
	[igStruct]
	public struct igCommandBeginNamedEventParameters
	{
		public string _name;
	}
	[igStruct]
	public struct igCommandEndNamedEventParameters
	{
		public int _count;
	}
	[igStruct]
	public struct igCommandIssueBufferedGpuTimestampParameters
	{
		public igGraphicsObject? _timestamp;
	}
}
