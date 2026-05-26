/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Diagnostics;
using System.Reflection;
using igLibrary.Gfx;

namespace igLibrary.Graphics
{
	public class igCommandStream : igObject
	{
		public ulong _writeHead;
		public ulong _writeChunkBegin;
		public ulong _writeChunkEnd;
		public ulong _readHead;
		public ulong _readChunkBegin;
		public ulong _readChunkEnd;


		public struct igCommand
		{
			public igCommandId _commandId;
			public object? _parameters;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="commandId">The id of the new command</param>
			/// <param name="parameters">The parameters for the new command</param>
			public igCommand(igCommandId commandId, object? parameters)
			{
				_commandId = commandId;
				_parameters = parameters;
			}
		}

		public List<igCommand> _commands = new List<igCommand>();


		private static Dictionary<igCommandId, igCompoundMetaFieldInfo> _commandFields = new Dictionary<igCommandId, igCompoundMetaFieldInfo>();


		private static igCompoundMetaFieldInfo? QueryForCommandField(igCommandId commandId)
		{
			if (!_commandFields.TryGetValue(commandId, out igCompoundMetaFieldInfo? decoderField))
			{
				// remove the k from the start of the enum name
				string typeName = $"igCommand{commandId.ToString().Substring(1)}ParametersMetaField";
				decoderField = igArkCore.GetCompoundFieldInfo(typeName);
				if (decoderField == null)
				{
					Logging.Error("Lacking the metafield type {0} (id {1}), skipping this command", typeName, commandId);
					return null;
				}

				decoderField.GatherDependancies();
				igArkCore.FlushPendingTypes();

				_commandFields.Add(commandId, decoderField);
			}

			return decoderField;
		}

		/// <summary>
		/// Decodes a command stream
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <param name="stream">The stream</param>
		/// <param name="graphicsObjects">The graphics object list</param>
		protected void DecodeIGZ(IG_CORE_PLATFORM platform, StreamHelper stream, igGraphicsObjectSet graphicsObjects)
		{
			_commands.Clear();

			igMetaEnum? commandIdEnum = igArkCore.GetMetaEnum(nameof(igCommandId));
			if (commandIdEnum == null)
			{
				Logging.Error("Lacking the {0} metaenum while trying to decode an {1}", nameof(igCommandId), nameof(igCommandStream));
				return;
			}

			uint pointerSize = igAlchemyCore.GetPointerSize(platform);
			bool is64Bit     = igAlchemyCore.isPlatform64Bit(platform);

			igMetaEnum gfxDrawEnum       = igArkCore.GetMetaEnum(nameof(IG_GFX_DRAW))!;
			igMetaEnum indexTypeEnum     = igArkCore.GetMetaEnum(nameof(IG_INDEX_TYPE))!;
			igMetaEnum histencilFuncEnum = igArkCore.GetMetaEnum(nameof(IG_GFX_HISTENCIL_FUNCTION))!;
			igMetaEnum stencilFuncEnum   = igArkCore.GetMetaEnum(nameof(IG_GFX_STENCIL_FUNCTION))!;

			while (stream.Tell() < stream.BaseStream.Length)
			{
				igCommand command = new igCommand();
				command._commandId = (igCommandId)commandIdEnum.GetEnumFromValue(stream.ReadInt32());

				switch (command._commandId)
				{
					case igCommandId.kSetPrimitiveType:
					{
						igCommandSetPrimitiveTypeParameters parameters = new igCommandSetPrimitiveTypeParameters();
						parameters._type = DecodeEnum<IG_GFX_DRAW>(stream, gfxDrawEnum);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexBuffer:
					{
						igCommandSetVertexBufferParameters parameters = new igCommandSetVertexBufferParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._format   = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetIndexBuffer:
					{
						igCommandSetIndexBufferParameters parameters = new igCommandSetIndexBufferParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._format   = DecodeEnum<IG_INDEX_TYPE>(stream, indexTypeEnum);
						parameters._offset   = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShader:
					{
						igCommandSetVertexShaderParameters parameters = new igCommandSetVertexShaderParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShaderVariant:
					{
						igCommandSetVertexShaderVariantParameters parameters = new igCommandSetVertexShaderVariantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShaderTexture:
					{
						igCommandSetVertexShaderTextureParameters parameters = new igCommandSetVertexShaderTextureParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShaderSampler:
					{
						igCommandSetVertexShaderSamplerParameters parameters = new igCommandSetVertexShaderSamplerParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetViewport:
					{
						igCommandSetViewportParameters parameters = new igCommandSetViewportParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetScissor:
					{
						igCommandSetScissorParameters parameters = new igCommandSetScissorParameters();
						parameters._x = DecodeInt32(stream);
						parameters._y = DecodeInt32(stream);
						parameters._w = DecodeInt32(stream);
						parameters._h = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetScissorEnabled:
					{
						igCommandSetScissorEnabledParameters parameters = new igCommandSetScissorEnabledParameters();
						parameters._enabled = stream.ReadBoolean();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetRasterizeStateBundle:
					{
						igCommandSetRasterizeStateBundleParameters parameters = new igCommandSetRasterizeStateBundleParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShader:
					{
						igCommandSetPixelShaderParameters parameters = new igCommandSetPixelShaderParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShaderVariant:
					{
						igCommandSetPixelShaderVariantParameters parameters = new igCommandSetPixelShaderVariantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShaderTexture:
					{
						igCommandSetPixelShaderTextureParameters parameters = new igCommandSetPixelShaderTextureParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShaderSampler:
					{
						igCommandSetPixelShaderSamplerParameters parameters = new igCommandSetPixelShaderSamplerParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetAlphaTestStateBundle:
					{
						igCommandSetAlphaTestStateBundleParameters parameters = new igCommandSetAlphaTestStateBundleParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetBlendStateBundle:
					{
						igCommandSetBlendStateBundleParameters parameters = new igCommandSetBlendStateBundleParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetDepthStateBundle:
					{
						igCommandSetDepthStateBundleParameters parameters = new igCommandSetDepthStateBundleParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetStencilStateBundle:
					{
						igCommandSetStencilStateBundleParameters parameters = new igCommandSetStencilStateBundleParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetStencilRef:
					{
						igCommandSetStencilRefParameters parameters = new igCommandSetStencilRefParameters();
						parameters._stencilRef = DecodeUInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetRenderTargets:
					{
						igCommandSetRenderTargetsParameters parameters = new igCommandSetRenderTargetsParameters();
						parameters._colorTargets = new ulong[8];
						parameters._colorTargets[0] = DecodeSizeT(stream, platform);
						parameters._colorTargets[1] = DecodeSizeT(stream, platform);
						parameters._colorTargets[2] = DecodeSizeT(stream, platform);
						parameters._colorTargets[3] = DecodeSizeT(stream, platform);
						parameters._colorTargets[4] = DecodeSizeT(stream, platform);
						parameters._colorTargets[5] = DecodeSizeT(stream, platform);
						parameters._colorTargets[6] = DecodeSizeT(stream, platform);
						parameters._colorTargets[7] = DecodeSizeT(stream, platform);
						parameters._colorCount = DecodeUInt32(stream);
						parameters._depthTarget = DecodeSizeT(stream, platform);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetRenderTargetMask:
					{
						igCommandSetRenderTargetMaskParameters parameters = new igCommandSetRenderTargetMaskParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kXenonSetHiStencil:
					{
						igCommandXenonSetHiStencilParameters parameters = new igCommandXenonSetHiStencilParameters();
						parameters._state      = stream.ReadBoolean();
						parameters._writeState = stream.ReadBoolean();
						parameters._func       = DecodeEnum<IG_GFX_HISTENCIL_FUNCTION>(stream, histencilFuncEnum);
						parameters._refValue   = DecodeUInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kXenonFlushHiZStencil:
					{
						igCommandXenonSetFlushHiZStencilParameters parameters = new igCommandXenonSetFlushHiZStencilParameters();
						parameters._async      = stream.ReadBoolean();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kXenonSetGprCounts:
					{
						igCommandXenonSetGprCountsParameters parameters = new igCommandXenonSetGprCountsParameters();
						parameters._vertex = DecodeUInt32(stream);
						parameters._pixel  = DecodeUInt32(stream);
						command._parameters = parameters;
						break;
					}
					/*case igCommandId.kPS3DrawEdgeGeometry:
					{
						igCommandDrawEdgeGeometryParameters parameters = new igCommandDrawEdgeGeometryParameters();
						parameters._edgeGeometry              = DecodeResource(stream, platform, graphicsObjects);
						parameters._modelMatrix               = DecodeSizeT(stream, platform);
						parameters._morphTargetWeights        = DecodeSizeT(stream, platform);
						parameters._morphTargetCount          = stream.ReadByte();
						parameters._blendVectors              = DecodeSizeT(stream, platform);
						parameters._blendVectorCount          = DecodeInt32(stream);
						parameters._ignoreNearPlaneForCulling = stream.ReadBoolean();
						parameters._cacheId                   = DecodeUInt32(stream);
						parameters._cacheResults              = stream.ReadBoolean();
						command._parameters = parameters;
						break;
					}*/
					case igCommandId.kPS3SetSCull:
					{
						igCommandPS3SetSCullParameters parameters = new igCommandPS3SetSCullParameters();
						parameters._function = DecodeEnum<IG_GFX_STENCIL_FUNCTION>(stream, stencilFuncEnum);
						parameters._refValue = DecodeUInt32(stream);
						parameters._mask     = DecodeUInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantBool:
					{
						igCommandSetConstantBoolParameters parameters = new igCommandSetConstantBoolParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = stream.ReadBoolean();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantInt:
					{
						igCommandSetConstantIntParameters parameters = new igCommandSetConstantIntParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantFloat:
					{
						igCommandSetConstantFloatParameters parameters = new igCommandSetConstantFloatParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeFloat(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantVec4f:
					{
						stream.Align(0x10); // Align of igVec4f
						igCommandSetConstantVec4fParameters parameters = new igCommandSetConstantVec4fParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						stream.Align(0x10); // Align of igVec4f
						parameters._value    = new igVec4f(DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream));
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantMatrix44f:
					{
						stream.Align(0x10); // Align of igMatrix44f
						igCommandSetConstantMatrix44fParameters parameters = new igCommandSetConstantMatrix44fParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						stream.Align(0x10); // Align of igVec4f
						parameters._value    = new igMatrix44f(DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream), DecodeFloat(stream));
						command._parameters = parameters;
						break;
					}
					/*case igCommandId.kSetConstantArrayInt:
					{
						igCommandSetConstantArrayIntParameters parameters = new igCommandSetConstantArrayIntParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeSizeT(stream, platform);
						parameters._count    = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantArrayFloat:
					{
						igCommandSetConstantArrayFloatParameters parameters = new igCommandSetConstantArrayFloatParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeSizeT(stream, platform);
						parameters._count    = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantArrayVec4f:
					{
						igCommandSetConstantArrayVec4fParameters parameters = new igCommandSetConstantArrayVec4fParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeSizeT(stream, platform);
						parameters._count    = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetConstantArrayMatrix44f:
					{
						igCommandSetConstantArrayMatrix44fParameters parameters = new igCommandSetConstantArrayMatrix44fParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._value    = DecodeSizeT(stream, platform);
						parameters._count    = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}*/
					case igCommandId.kApplyConstantBundle:
					{
						igCommandApplyConstantBundleParameters parameters = new igCommandApplyConstantBundleParameters();
						parameters._bundle = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kApplyConstantValueList:
					{
						igCommandApplyConstantValueListParameters parameters = new igCommandApplyConstantValueListParameters();
						parameters._list = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShaderTextureEnabledConstant:
					{
						igCommandSetPixelShaderTextureEnabledConstantParameters parameters = new igCommandSetPixelShaderTextureEnabledConstantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShaderTextureEnabledConstant:
					{
						igCommandSetVertexShaderTextureEnabledConstantParameters parameters = new igCommandSetVertexShaderTextureEnabledConstantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetPixelShaderTextureSizeConstant:
					{
						igCommandSetPixelShaderTextureSizeConstantParameters parameters = new igCommandSetPixelShaderTextureSizeConstantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetVertexShaderTextureSizeConstant:
					{
						igCommandSetVertexShaderTextureSizeConstantParameters parameters = new igCommandSetVertexShaderTextureSizeConstantParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kClearRenderTarget:
					{
						igCommandClearRenderTargetParameters parameters = new igCommandClearRenderTargetParameters();
						parameters._resource = DecodeResource(stream, platform, graphicsObjects);
						parameters._register = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					/*case igCommandId.kDraw:
					{
						// Nothing?
						break;
					}*/
					case igCommandId.kDrawPrimitives:
					{
						igCommandDrawPrimitivesParameters parameters = new igCommandDrawPrimitivesParameters();
						parameters._primitive     = DecodeEnum<IG_GFX_DRAW>(stream, gfxDrawEnum);
						parameters._numPrimitives = DecodeInt32(stream);
						parameters._numPrimitives = DecodeInt32(stream);
						break;
					}
					/*case igCommandId.kFlush:
					{
						// Nothing?
						break;
					}
					case igCommandId.kResetState:
					{
						// Nothing?
						break;
					}
					case igCommandId.kDecodeMemoryCommandStream:
					{
						igCommandDecodeMemoryCommandStreamParameters parameters = new igCommandDecodeMemoryCommandStreamParameters();
						parameters._stream = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kCopyTexture:
					{
						igCommandCopyTextureParameters parameters = new igCommandCopyTextureParameters();
						parameters._source      = DecodeResource(stream, platform, graphicsObjects);
						parameters._destination = DecodeResource(stream, platform, graphicsObjects);
						parameters._params._sourceX             = DecodeInt32(stream);
						parameters._params._sourceY             = DecodeInt32(stream);
						parameters._params._destinationX        = DecodeInt32(stream);
						parameters._params._destinationY        = DecodeInt32(stream);
						parameters._params._width               = DecodeInt32(stream);
						parameters._params._height              = DecodeInt32(stream);
						parameters._params._sourceMipLevel      = DecodeInt32(stream);
						parameters._params._destinationMipLevel = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kUpdateTexture:
					{
						igCommandUpdateTextureParameters parameters = new igCommandUpdateTextureParameters();
						parameters._texture
						command._parameters = parameters;
						break;
					}
					case igCommandId.kExecuteCallback:
					{
						igCommandExecuteCallbackParameters parameters = new igCommandExecuteCallbackParameters();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kSetCameraMatrices:
					{
						igCommandSetCameraMatricesParameters parameters = new igCommandSetCameraMatricesParameters();
						parameters._cameraIndex        = stream.ReadByte();
						parameters._viewMatrix         = stream.ReadByte();
						parameters._previousViewMatrix = stream.ReadByte();
						parameters._projMatrix         = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kComputeAndSetInstanceMatrices:
					{
						igCommandComputeAndSetInstanceMatricesParameters parameters = new igCommandComputeAndSetInstanceMatricesParameters();
						parameters._modelMatrix     = DecodeSizeT(stream, platform);
						parameters._prevModelMatrix = DecodeSizeT(stream, platform);
						parameters._matrixConstants = DecodeUInt16(stream);
						parameters._cameraIndex     = stream.ReadByte();
						command._parameters = parameters;
						break;
					}
					case igCommandId.kComputeAndSetInstanceConstants:
					{
						igCommandComputeAndSetInstanceConstantsParameters parameters = new igCommandComputeAndSetInstanceConstantsParameters();
						parameters._effectFlags   = stream.ReadByte();
						parameters._geometryFlags = stream.ReadByte();
						command._parameters = parameters;
						break;
					}*/
					case igCommandId.kSetCommonRenderState:
					{
						igCommandSetCommonRenderStateParameters parameters = new igCommandSetCommonRenderStateParameters();
						parameters._commonRenderState = DecodeUInt16(stream);
						command._parameters = parameters;
						break;
					}
					/*case igCommandId.kSetDitherState:
					{
						igCommandSetDitherStateParameters parameters = new igCommandSetDitherStateParameters();
						parameters._enabled       = stream.ReadBoolean();
						parameters._ditherOpacity = DecodeFloat(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kBeginNamedEvent:
					{
						igCommandBeginNamedEventParameters parameters = new igCommandBeginNamedEventParameters();
						parameters._name = "Idk how to read the name :(";
						DecodeSizeT(stream, platform);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kEndNamedEvent:
					{
						igCommandEndNamedEventParameters parameters = new igCommandEndNamedEventParameters();
						parameters._count = DecodeInt32(stream);
						command._parameters = parameters;
						break;
					}
					case igCommandId.kIssueBufferedGpuTimestamp:
					{
						igCommandIssueBufferedGpuTimestampParameters parameters = new igCommandIssueBufferedGpuTimestampParameters();
						parameters._timestamp = DecodeResource(stream, platform, graphicsObjects);
						command._parameters = parameters;
						break;
					}*/
					default:
						throw new NotImplementedException($"Command id {command._commandId} is not implemented");
				}

				_commands.Add(command);

				stream.Align(sizeof(int));
			}
		}


		private ushort DecodeUInt16(StreamHelper stream)
		{
			stream.Align(sizeof(ushort));
			return stream.ReadUInt16();
		}

		private int DecodeInt32(StreamHelper stream)
		{
			stream.Align(sizeof(int));
			return stream.ReadInt32();
		}

		private float DecodeFloat(StreamHelper stream)
		{
			stream.Align(sizeof(float));
			return stream.ReadSingle();
		}

		private uint DecodeUInt32(StreamHelper stream)
		{
			stream.Align(sizeof(uint));
			return stream.ReadUInt32();
		}

		private T DecodeEnum<T>(StreamHelper stream, igMetaEnum metaEnum) where T : Enum
		{
			return (T)metaEnum.GetEnumFromValue(DecodeInt32(stream));
		}

		private ulong DecodeSizeT(StreamHelper stream, IG_CORE_PLATFORM platform)
		{
			uint pointerSize = igAlchemyCore.GetPointerSize(platform);
			stream.Align(pointerSize);
			ulong value = 0;
			if (pointerSize == 4)
			{
				value = stream.ReadUInt32();
			}
			else
			{
				value = stream.ReadUInt64();
			}
			return value;
		}

		private igGraphicsObject? DecodeResource(StreamHelper stream, IG_CORE_PLATFORM platform, igGraphicsObjectSet graphicsObjects)
		{
			ulong index = DecodeSizeT(stream, platform);

			if (index >= 0 && index < (ulong)graphicsObjects._objects._count)
			{
				return graphicsObjects._objects[(int)index];
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Encodes a command stream
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <param name="graphicsObjects">The graphics object set for the commands</param>
		/// <param name="stream">The stream</param>
		protected void EncodeIGZ(IG_CORE_PLATFORM platform, igGraphicsObjectSet graphicsObjects, StreamHelper stream)
		{
			igMetaEnum? commandIdEnum = igArkCore.GetMetaEnum(nameof(igCommandId));
			if (commandIdEnum == null)
			{
				Logging.Error("Lacking the {0} metaenum while trying to decode an {1}", nameof(igCommandId), nameof(igCommandStream));
				return;
			}

			uint pointerSize = igAlchemyCore.GetPointerSize(platform);
			bool is64Bit     = igAlchemyCore.isPlatform64Bit(platform);

			for (int c = 0; c < _commands.Count; c++)
			{
				igCommand command = _commands[c];
				stream.Align(sizeof(int));
				stream.WriteInt32(commandIdEnum.GetValueFromEnum(command._commandId));

				if (command._commandId != igCommandId.kNoop)
				{
					igCompoundMetaFieldInfo? decoderField = QueryForCommandField(command._commandId);
					if (decoderField == null)
					{
						continue;
					}

					stream.Align(decoderField._platformInfo._alignments[platform]);

					// had to reimplement writing, sorry
					for (int f = 0; f < decoderField._fieldList.Count; f++)
					{
						igMetaField field = decoderField._fieldList[f];

						object? itemValue = field._fieldHandle!.GetValue(command._parameters);
						Array? arrayValue = field.IsArray ? (Array?)itemValue : null;

						for (int a = 0; a < (field.IsArray ? field.ArrayNum : 1); a++)
						{
							if (arrayValue != null)
							{
								itemValue = arrayValue.GetValue(a);
							}

							stream.Align(field.GetAlignment(platform));

							if (field is igSizeTypeMetaField || field is igObjectRefMetaField)
							{
								ulong ulongValue;
								if (field._fieldName == "_resource" || field is igObjectRefMetaField)
								{
									int index = graphicsObjects.FindResourceIndex((igGraphicsObject?)itemValue);
									if (index < 0)
									{
										throw new InvalidDataException("Trying to write a command that wasn't added to the object set");
									}
									ulongValue = (ulong)index;
								}
								else
								{
									ulongValue = (ulong)itemValue!;
								}

								if (is64Bit)
								{
									stream.WriteUInt64(ulongValue);
								}
								else
								{
									stream.WriteUInt32((uint)ulongValue);
								}
							}
							else if (field is igIntMetaField)
							{
								stream.WriteInt32((int)itemValue!);
							}
							else if (field is igUnsignedIntMetaField)
							{
								stream.WriteUInt32((uint)itemValue!);
							}
							else if (field is igUnsignedShortMetaField)
							{
								stream.WriteUInt16((ushort)itemValue!);
							}
							else if (field is igFloatMetaField)
							{
								stream.WriteSingle((float)itemValue!);
							}
							else if (field is igVec4fMetaField)
							{
								igVec4f vecValue = (igVec4f)itemValue!;
								stream.WriteSingle(vecValue._x);
								stream.WriteSingle(vecValue._y);
								stream.WriteSingle(vecValue._z);
								stream.WriteSingle(vecValue._w);
							}
							else if (field is igMatrix44fMetaField)
							{
								igMatrix44f matValue = (igMatrix44f)itemValue!;
								stream.WriteSingle(matValue._m11);
								stream.WriteSingle(matValue._m12);
								stream.WriteSingle(matValue._m13);
								stream.WriteSingle(matValue._m14);
								stream.WriteSingle(matValue._m21);
								stream.WriteSingle(matValue._m22);
								stream.WriteSingle(matValue._m23);
								stream.WriteSingle(matValue._m24);
								stream.WriteSingle(matValue._m31);
								stream.WriteSingle(matValue._m32);
								stream.WriteSingle(matValue._m33);
								stream.WriteSingle(matValue._m34);
								stream.WriteSingle(matValue._m41);
								stream.WriteSingle(matValue._m42);
								stream.WriteSingle(matValue._m43);
								stream.WriteSingle(matValue._m44);
							}
							else if (field is igCompoundMetaField compoundField && compoundField._compoundFieldInfo._name == nameof(igCopyTextureParameters))
							{
								igCopyTextureParameters ctParams = (igCopyTextureParameters)itemValue!;
								stream.WriteInt32(ctParams._sourceX);
								stream.WriteInt32(ctParams._sourceY);
								stream.WriteInt32(ctParams._destinationX);
								stream.WriteInt32(ctParams._destinationY);
								stream.WriteInt32(ctParams._width);
								stream.WriteInt32(ctParams._height);
								stream.WriteInt32(ctParams._sourceMipLevel);
								stream.WriteInt32(ctParams._destinationMipLevel);
							}
							else if (field is igUnsignedCharMetaField)
							{
								stream.WriteByte((byte)itemValue!);
							}
							else if (field is igBoolMetaField)
							{
								stream.WriteBoolean((bool)itemValue!);
							}
							else if (field is igEnumMetaField enumMetaField)
							{
								stream.WriteInt32(enumMetaField._metaEnum.GetValueFromEnum(itemValue!));
							}
							else
							{
								Logging.Warn("Unimplemented metafield type {0}", field.GetType().Name);
							}
						}
					}
				}
			}

			stream.BaseStream.Flush();
		}
	}
}