/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public enum igCommandId : uint
	{
		kNoop,
		kSetPrimitiveType,
		kSetVertexBuffer,
		kSetIndexBuffer,
		kSetVertexShader,
		kSetVertexShaderVariant,
		kSetVertexShaderTexture,
		kSetVertexShaderSampler,
		kSetViewport,
		kSetScissor,
		kSetScissorEnabled,
		kSetRasterizeStateBundle,
		kSetPixelShader,
		kSetPixelShaderVariant,
		kSetPixelShaderTexture,
		kSetPixelShaderSampler,
		kSetAlphaTestStateBundle,
		kSetBlendStateBundle,
		kSetDepthStateBundle,
		kSetStencilStateBundle,
		kSetStencilRef,
		kSetRenderTargets,
		kSetRenderTargetMask,
		kXenonSetHiStencil,
		kXenonFlushHiZStencil,
		kXenonSetGprCounts,
		kPS3DrawEdgeGeometry,
		kPS3SetSCull,
		kSetConstantBool,
		kSetConstantInt,
		kSetConstantFloat,
		kSetConstantVec4f,
		kSetConstantMatrix44f,
		kSetConstantArrayInt,
		kSetConstantArrayFloat,
		kSetConstantArrayVec4f,
		kSetConstantArrayMatrix44f,
		kApplyConstantBundle,
		kApplyConstantValueList,
		kSetPixelShaderTextureEnabledConstant,
		kSetVertexShaderTextureEnabledConstant,
		kSetPixelShaderTextureSizeConstant,
		kSetVertexShaderTextureSizeConstant,
		kClearRenderTarget,
		kDraw,
		kDrawPrimitives,
		kFlush,
		kResetState,
		kDecodeMemoryCommandStream,
		kCopyTexture,
		kUpdateTexture,
		kExecuteCallback,
		kSetCameraMatrices,
		kComputeAndSetInstanceMatrices,
		kComputeAndSetInstanceConstants,
		kSetCommonRenderState,
		kSetDitherState,
		kBeginNamedEvent,
		kEndNamedEvent,
		kIssueBufferedGpuTimestamp,
		kNumAlchemyCommands,
		kLastAlchemyCommand,
		kLastCommand,
	}
}
