/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public enum igCommandId : int
	{
		kNoop = 0,
		kSetPrimitiveType = 1,
		kSetVertexBuffer = 2,
		kSetIndexBuffer = 3,
		kSetVertexShader = 4,
		kSetVertexShaderVariant = 5,
		kSetVertexShaderTexture = 6,
		kSetVertexShaderSampler = 7,
		kSetViewport = 8,
		kSetScissor = 9,
		kSetScissorEnabled = 10,
		kSetRasterizeStateBundle = 11,
		kSetPixelShader = 12,
		kSetPixelShaderVariant = 13,
		kSetPixelShaderTexture = 14,
		kSetPixelShaderSampler = 15,
		kSetAlphaTestStateBundle = 16,
		kSetBlendStateBundle = 17,
		kSetDepthStateBundle = 18,
		kSetStencilStateBundle = 19,
		kSetStencilRef = 20,
		kSetRenderTargets = 21,
		kSetRenderTargetMask = 22,
		kXenonSetHiStencil = 23,
		kXenonFlushHiZStencil = 24,
		kXenonSetGprCounts = 25,
		kPS3DrawEdgeGeometry = 26,
		kPS3SetSCull = 27,
		kSetConstantBool = 28,
		kSetConstantInt = 29,
		kSetConstantFloat = 30,
		kSetConstantVec4f = 31,
		kSetConstantMatrix44f = 32,
		kSetConstantArrayInt = 33,
		kSetConstantArrayFloat = 34,
		kSetConstantArrayVec4f = 35,
		kSetConstantArrayMatrix44f = 36,
		kApplyConstantBundle = 37,
		kApplyConstantValueList = 38,
		kSetPixelShaderTextureEnabledConstant = 39,
		kSetVertexShaderTextureEnabledConstant = 40,
		kSetPixelShaderTextureSizeConstant = 41,
		kSetVertexShaderTextureSizeConstant = 42,
		kClearRenderTarget = 43,
		kDraw = 44,
		kDrawPrimitives = 45,
		kFlush = 46,
		kResetState = 47,
		kDecodeMemoryCommandStream = 48,
		kCopyTexture = 49,
		kUpdateTexture = 50,
		kExecuteCallback = 51,
		kSetCameraMatrices = 52,
		kComputeAndSetInstanceMatrices = 53,
		kComputeAndSetInstanceConstants = 54,
		kSetCommonRenderState = 55,
		kSetDitherState = 56,
		kBeginNamedEvent = 57,
		kEndNamedEvent = 58,
		kIssueBufferedGpuTimestamp = 59,
		kNumAlchemyCommands = 60,
		kLastAlchemyCommand = 8191,
		kLastCommand = 65535,
	}
}
