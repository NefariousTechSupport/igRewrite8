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

	public struct igCommandSetPrimitiveTypeParameters
	{
		public IG_GFX_DRAW _type;
	}
	public struct igCommandSetVertexBufferParameters
	{
		public igGraphicsObject? _resource;
		public igGraphicsObject? _format;
		public int _offset;
		public byte _register;
	}
	public struct igCommandSetIndexBufferParameters
	{
		public igGraphicsObject? _resource;
		public IG_INDEX_TYPE _format;
		public int _offset;
	}
	public struct igCommandSetVertexShaderParameters
	{
		public igGraphicsObject? _resource;
	}

	public struct igCommandSetVertexShaderVariantParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetVertexShaderTextureParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetVertexShaderSamplerParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetViewportParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetScissorParameters
	{
		public int _x;
		public int _y;
		public int _w;
		public int _h;
	}
	public struct igCommandSetScissorEnabledParameters
	{
		public bool _enabled;
	}
	public struct igCommandSetRasterizeStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetPixelShaderParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetPixelShaderVariantParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetPixelShaderTextureParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetPixelShaderSamplerParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetAlphaTestStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetBlendStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetDepthStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetStencilStateBundleParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandSetStencilRefParameters
	{
		public uint _stencilRef;
	}
	public struct igCommandSetRenderTargetsParameters
	{
		public ulong[] _colorTargets;
		public uint _colorCount;
		public ulong _depthTarget;
	}
	public struct igCommandSetRenderTargetMaskParameters
	{
		public igGraphicsObject? _resource;
	}
	public struct igCommandXenonSetHiStencilParameters
	{
		public bool _state;
		public bool _writeState;
		public IG_GFX_HISTENCIL_FUNCTION _func;
		public uint _refValue;
	}
	public struct igCommandXenonSetFlushHiZStencilParameters
	{
		public bool _async;
	}
	public struct igCommandXenonSetGprCountsParameters
	{
		public uint _vertex;
		public uint _pixel;
	}
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
	public struct igCommandPS3SetSCullParameters
	{
		public IG_GFX_STENCIL_FUNCTION _function;
		public uint _refValue;
		public uint _mask;
	}
	public struct igCommandSetConstantBoolParameters
	{
		public igGraphicsObject? _resource;
		public bool _value;
	}
	public struct igCommandSetConstantIntParameters
	{
		public igGraphicsObject? _resource;
		public int _value;
	}
	public struct igCommandSetConstantFloatParameters
	{
		public igGraphicsObject? _resource;
		public float _value;
	}
	public struct igCommandSetConstantVec4fParameters
	{
		public igGraphicsObject? _resource;
		public igVec4f _value;
	}
	public struct igCommandSetConstantMatrix44fParameters
	{
		public igGraphicsObject? _resource;
		public igMatrix44f _value;
	}
	public struct igCommandSetConstantArrayIntParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	public struct igCommandSetConstantArrayFloatParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	public struct igCommandSetConstantArrayVec4fParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	public struct igCommandSetConstantArrayMatrix44fParameters
	{
		public igGraphicsObject? _resource;
		public ulong _value;
		public int _count;
	}
	public struct igCommandApplyConstantBundleParameters
	{
		public igGraphicsObject? _bundle;
	}
	public struct igCommandApplyConstantValueListParameters
	{
		public igGraphicsObject? _bundle;
	}
	public struct igCommandSetPixelShaderTextureEnabledConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetVertexShaderTextureEnabledConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetPixelShaderTextureSizeConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandSetVertexShaderTextureSizeConstantParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandClearRenderTargetParameters
	{
		public igGraphicsObject? _resource;
		public byte _register;
	}
	public struct igCommandDrawPrimitivesParameters
	{
		public IG_GFX_DRAW _primitive;
		public int _numPrimitives;
		public int _offset;
	}
	public struct igCommandDecodeMemoryCommandStreamParameters
	{
		public igGraphicsObject? _stream;
	}
	public struct igCommandCopyTextureParameters
	{
		public igGraphicsObject? _source;
		public igGraphicsObject? _destination;
		public igCopyTextureParameters _params;
	}
	public struct igCommandUpdateTextureParameters
	{
		public ulong _texture;
		public ulong _data;
		public uint _size;
		public uint _imageIndex;
		public uint _mipLevel;
		public uint _flags;
	}
	public struct igCommandExecuteCallbackParameters
	{
		//It's a struct called _callback
	}
	public struct igCommandSetCameraMatricesParameters
	{
		public byte _cameraIndex;
		public byte _viewMatrix;
		public byte _previousViewMatrix;
		public byte _projMatrix;
	}
	public struct igCommandComputeAndSetInstanceMatricesParameters
	{
		public ulong _modelMatrix;
		public ulong _prevModelMatrix;
		public ushort _matrixConstants;
		public byte _cameraIndex;
	}
	public struct igCommandComputeAndSetInstanceConstantsParameters
	{
		public byte _effectFlags;
		public byte _geometryFlags;
	}
	public struct igCommandSetCommonRenderStateParameters
	{
		public ushort _commonRenderState;
	}
	public struct igCommandSetDitherStateParameters
	{
		public bool _enabled;
		public float _ditherOpacity;
	}
	public struct igCommandBeginNamedEventParameters
	{
		public string _name;
	}
	public struct igCommandEndNamedEventParameters
	{
		public int _count;
	}
	public struct igCommandIssueBufferedGpuTimestampParameters
	{
		public igGraphicsObject? _timestamp;
	}
}
