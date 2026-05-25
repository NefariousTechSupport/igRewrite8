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
	public class igGraphicsMaterial : igMaterial
	{
		public ulong _globalTechniqueMask;
		public uint _materialBitField;
		public float _sortDepthOffset;
		public igHandle _effectHandle;
		public igMemoryCommandStream _commonState;
		public igVector<igMemoryCommandStream?> _techniques;
		public igGraphicsMaterialAnimationList _animations;
		public igGraphicsObjectSet _graphicsObjects;
		public byte _sortKey;
		public igDrawType _drawType;
		public igGraphicsMaterialAnimationTimeSource _timeSource;

		private DecompiledMaterial? _decompiledCommonState;
		private igVector<DecompiledMaterial?> _decompiledTechniques;

		public enum ConstantType
		{
			Bool,
			Int,
			Float,
			Vec4f,
			Matrix44f
		}

		public class DecompiledConstant
		{
			public string _name;
			public object _value;
			public ConstantType _type;


			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="name">The name of the constant</param>
			/// <param name="value">The value of the constant</param>
			/// <exception cref="NotImplementedException">The constant type is not implemented</exception>
			public DecompiledConstant(string name, object value)
			{
				_name = name;
				_value = value;
				     if (value is bool)        _type = ConstantType.Bool;
				else if (value is int)         _type = ConstantType.Int;
				else if (value is float)       _type = ConstantType.Float;
				else if (value is igVec4f)     _type = ConstantType.Vec4f;
				else if (value is igMatrix44f) _type = ConstantType.Matrix44f;
				else throw new NotImplementedException("Invalid constant datatype provided");
			}


			/// <summary>
			/// Gets the igShaderConstantValue of a decompiled constant
			/// </summary>
			/// <param name="graphicsObjects">The vector of graphics objects</param>
			/// <returns>the new igShaderConstantValue</returns>
			/// <exception cref="NotImplementedException">if the constant type isn't implemented</exception>
			public igShaderConstantValue GetShaderValue(igGraphicsObjectSet graphicsObjects)
			{
				igShaderConstantValue value;
				switch (_type)
				{
					case ConstantType.Bool:
						value = new igShaderConstantValueBool()   { _value =        (bool)this._value };
						break;
					case ConstantType.Int:
						value = new igShaderConstantValueInt()    { _value =         (int)this._value };
						break;
					case ConstantType.Float:
						value = new igShaderConstantValueFloat()  { _value =       (float)this._value };
						break;
					case ConstantType.Vec4f:
						value = new igShaderConstantValueVector() { _value =     (igVec4f)this._value };
						break;
					case ConstantType.Matrix44f:
						value = new igShaderConstantValueMatrix() { _value = (igMatrix44f)this._value };
						break;
					default:
						throw new NotImplementedException("Unimplemented shader constant type");
				}

				igGraphicsShaderConstant graphicsConstant = new igGraphicsShaderConstant();
				graphicsConstant._name = _name;
				value._constant = graphicsObjects.GetOrAddGraphicsObject(graphicsConstant);

				return value;
			}
		}
		public class DecompiledTexture
		{
			public byte _register;
			public igImage2? _image;
			public igHandle? _imageHandle;
			public igResourceUsage _usage;
			public igSamplerStateBundleDesc _samplerStateBundle;
		}
		public class DecompiledMaterial
		{
			public List<DecompiledConstant> _constants = new List<DecompiledConstant>();
			public igShaderConstantBundleList _bundles = new igShaderConstantBundleList();
			public List<DecompiledTexture> _textures = new List<DecompiledTexture>();
			public igStencilStateBundleDesc? _stencilStateBundle = null; 
			public uint? _stencilRef = null; 
			public ushort? _commonRenderState = null; 
			public igDepthStateBundleDesc? _depthStateBundle = null; 
			public igBlendStateBundleDesc? _blendStateBundle = null; 
			public igRasterizerStateBundleDesc? _rasterizerStateBundle = null; 
			public igRenderTargetMaskDesc? _renderTargetMask = null; 

			public DecompiledTexture GetAddTexture(byte register)
			{
				int index = _textures.FindIndex(x => x._register == register);
				if (index < 0)
				{
					index = _textures.Count;
					_textures.Add(new DecompiledTexture() { _register = register });
				}

				return _textures[index];
			}
		}

		/// <summary>
		/// Handle igMemoryCommandStream
		/// </summary>
		public override void PostFileRead()
		{
			if (_graphicsObjects != null)
			{
				if (_commonState != null)
				{
					_commonState.Decode(_graphicsObjects);

					_decompiledCommonState = DecompileMaterial(_commonState);
				}

				_decompiledTechniques = new igVector<DecompiledMaterial?>();
				for (int t = 0; t < _techniques._count; t++)
				{
					igMemoryCommandStream? technique = _techniques[t];
					if (technique != null)
					{
						technique.Decode(_graphicsObjects);

						_decompiledTechniques.Add(DecompileMaterial(technique));
					}
					else
					{
						_decompiledTechniques.Add(null);
					}
				}
			}
		}

		/// <summary>
		/// Write the decompiled material back to the igMemoryCommandStream
		/// </summary>
		public override void PreFileWrite()
		{
			if (_graphicsObjects != null)
			{
				int oldCount = _graphicsObjects._objects.Count;
				_graphicsObjects._objects.Clear();

				igMemoryPool pool = igMemoryContext.Singleton._pools["Default"];
				IG_CORE_PLATFORM platform = igRegistry.GetRegistry()._platform;
				igMetaObject? commandStreamMeta = igArkCore.GetObjectMeta(nameof(igMemoryCommandStream));
				Debug.Assert(commandStreamMeta != null);

				if (_decompiledCommonState != null)
				{
					_commonState = _commonState ?? (igMemoryCommandStream)commandStreamMeta.ConstructInstance(pool);

					CompileMaterial(_decompiledCommonState, _commonState);

					_commonState.Encode(pool, platform, _graphicsObjects);
				}

				List<int> oldTechniqueCommandCounts = new List<int>();
				for (int t = 0; t < _techniques.Count; t++)
				{
					oldTechniqueCommandCounts.Add(_techniques[t] != null ? _techniques[t]!._commands.Count : 0);
				}
				_techniques.Clear();
				for (int t = 0; t < _decompiledTechniques._count; t++)
				{
					if (_decompiledTechniques[t] != null)
					{
						igMemoryCommandStream technique = (igMemoryCommandStream)commandStreamMeta.ConstructInstance(pool);

						CompileMaterial(_decompiledTechniques[t]!, technique);
						technique.Encode(pool, platform, _graphicsObjects);

						_techniques.Add(technique);
						Debug.Assert(oldTechniqueCommandCounts[t] == technique._commands.Count);
					}
					else
					{
						_techniques.Add(null);
					}
				}

				Debug.Assert(oldCount == _graphicsObjects._objects.Count);
			}
		}

		public DecompiledMaterial GetDecompiledCommonState()
		{
			Debug.Assert(_decompiledCommonState != null);

			return _decompiledCommonState;
		}

		private DecompiledMaterial DecompileMaterial(igMemoryCommandStream? stream)
		{
			DecompiledMaterial decompiled = new DecompiledMaterial();

			if (stream == null)
			{
				return decompiled;
			}

			for (int i = 0; i < stream._commands.Count; i++)
			{
				igCommandStream.igCommand command = stream._commands[i];

				switch (command._commandId)
				{
					case igCommandId.kSetRasterizeStateBundle:
					{
						igCommandSetRasterizeStateBundleParameters parameters = (igCommandSetRasterizeStateBundleParameters)command._parameters!;
						igGraphicsRasterizerStateBundle? bundle = parameters._resource as igGraphicsRasterizerStateBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the bundle for a {0} command", command._commandId);
							continue;
						}
						decompiled._rasterizerStateBundle = bundle._rasterizerStateBundle;
						break;
					}
					case igCommandId.kSetDepthStateBundle:
					{
						igCommandSetDepthStateBundleParameters parameters = (igCommandSetDepthStateBundleParameters)command._parameters!;
						igGraphicsDepthStateBundle? bundle = parameters._resource as igGraphicsDepthStateBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the depth state bundle for a {0} command", command._commandId);
							continue;
						}
						decompiled._depthStateBundle = bundle._depthStateBundle;
						break;
					}
					case igCommandId.kSetStencilStateBundle:
					{
						igCommandSetStencilStateBundleParameters parameters = (igCommandSetStencilStateBundleParameters)command._parameters!;
						igGraphicsStencilStateBundle? bundle = parameters._resource as igGraphicsStencilStateBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the bundle for a {0} command", command._commandId);
							continue;
						}
						decompiled._stencilStateBundle = bundle._stencilStateBundle;
						break;
					}
					case igCommandId.kSetStencilRef:
					{
						igCommandSetStencilRefParameters parameters = (igCommandSetStencilRefParameters)command._parameters!;
						decompiled._stencilRef = parameters._stencilRef;
						break;
					}
					case igCommandId.kSetPixelShaderTexture:
					{
						igCommandSetPixelShaderTextureParameters parameters = (igCommandSetPixelShaderTextureParameters)command._parameters!;
						igGraphicsTexture? gTexture = parameters._resource as igGraphicsTexture;
						if (gTexture == null)
						{
							Logging.Warn("Failed to find the texture for a {0} command", command._commandId);
							continue;
						}
						DecompiledTexture dTexture = decompiled.GetAddTexture(parameters._register);
						dTexture._usage       = gTexture._usage;
						dTexture._image       = gTexture._image;
						dTexture._imageHandle = gTexture._imageHandle;
						break;
					}
					case igCommandId.kSetPixelShaderSampler:
					{
						igCommandSetPixelShaderSamplerParameters parameters = (igCommandSetPixelShaderSamplerParameters)command._parameters!;
						igGraphicsSamplerStateBundle? gSampler = parameters._resource as igGraphicsSamplerStateBundle;
						if (gSampler == null)
						{
							Logging.Warn("Failed to find the sampler for a {0} command", command._commandId);
							continue;
						}
						DecompiledTexture dTexture = decompiled.GetAddTexture(parameters._register);
						dTexture._samplerStateBundle = gSampler._samplerStateBundle;
						break;
					}
					case igCommandId.kSetBlendStateBundle:
					{
						igCommandSetBlendStateBundleParameters parameters = (igCommandSetBlendStateBundleParameters)command._parameters!;
						igGraphicsBlendStateBundle? bundle = parameters._resource as igGraphicsBlendStateBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the bandle for a {0} command", command._commandId);
							continue;
						}
						decompiled._blendStateBundle = bundle._blendStateBundle;
						break;
					}
					case igCommandId.kSetConstantBool:
					{
						igCommandSetConstantBoolParameters parameters = (igCommandSetConstantBoolParameters)command._parameters!;
						igGraphicsShaderConstant? gConstant = parameters._resource as igGraphicsShaderConstant;
						if (gConstant == null)
						{
							Logging.Warn("Failed to find the constant for a {0} command", command._commandId);
							continue;
						}
						decompiled._constants.Add(new DecompiledConstant(gConstant._name, parameters._value));
						break;
					}
					case igCommandId.kSetConstantInt:
					{
						igCommandSetConstantIntParameters parameters = (igCommandSetConstantIntParameters)command._parameters!;
						igGraphicsShaderConstant? gConstant = parameters._resource as igGraphicsShaderConstant;
						if (gConstant == null)
						{
							Logging.Warn("Failed to find the constant for a {0} command", command._commandId);
							continue;
						}
						decompiled._constants.Add(new DecompiledConstant(gConstant._name, parameters._value));
						break;
					}
					case igCommandId.kSetConstantFloat:
					{
						igCommandSetConstantFloatParameters parameters = (igCommandSetConstantFloatParameters)command._parameters!;
						igGraphicsShaderConstant? gConstant = parameters._resource as igGraphicsShaderConstant;
						if (gConstant == null)
						{
							Logging.Warn("Failed to find the constant for a {0} command", command._commandId);
							continue;
						}
						decompiled._constants.Add(new DecompiledConstant(gConstant._name, parameters._value));
						break;
					}
					case igCommandId.kSetConstantVec4f:
					{
						igCommandSetConstantVec4fParameters parameters = (igCommandSetConstantVec4fParameters)command._parameters!;
						igGraphicsShaderConstant? gConstant = parameters._resource as igGraphicsShaderConstant;
						if (gConstant == null)
						{
							Logging.Warn("Failed to find the constant for a {0} command", command._commandId);
							continue;
						}
						decompiled._constants.Add(new DecompiledConstant(gConstant._name, parameters._value));
						break;
					}
					case igCommandId.kSetConstantMatrix44f:
					{
						igCommandSetConstantMatrix44fParameters parameters = (igCommandSetConstantMatrix44fParameters)command._parameters!;
						igGraphicsShaderConstant? gConstant = parameters._resource as igGraphicsShaderConstant;
						if (gConstant == null)
						{
							Logging.Warn("Failed to find the constant for a {0} command", command._commandId);
							continue;
						}
						decompiled._constants.Add(new DecompiledConstant(gConstant._name, parameters._value));
						break;
					}
					case igCommandId.kApplyConstantBundle:
					{
						igCommandApplyConstantBundleParameters parameters = (igCommandApplyConstantBundleParameters)command._parameters!;
						igShaderConstantBundle? bundle = parameters._bundle as igShaderConstantBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the bundle for a {0} command", command._commandId);
							continue;
						}
						decompiled._bundles.Add(bundle);
						break;
					}
					case igCommandId.kApplyConstantValueList:
					{
						igCommandApplyConstantValueListParameters parameters = (igCommandApplyConstantValueListParameters)command._parameters!;
						igShaderConstantValueList? list = parameters._list as igShaderConstantValueList;
						if (list == null)
						{
							Logging.Warn("Failed to find the list for a {0} command", command._commandId);
							continue;
						}
						for (int v = 0; v < list._values._count; v++)
						{
							FieldInfo? valueField = list._values[v].GetType().GetField("_value");
							if (valueField == null)
							{
								throw new InvalidDataException($"Failed to find value field on an object of type {list._values[v].GetType().Name}");
							}
							decompiled._constants.Add(new DecompiledConstant(list._values[v]._constant._name, valueField.GetValue(list._values[v])!));
						}
						break;
					}
					case igCommandId.kSetCommonRenderState:
					{
						igCommandSetCommonRenderStateParameters parameters = (igCommandSetCommonRenderStateParameters)command._parameters!;
						decompiled._commonRenderState = parameters._commonRenderState;
						break;
					}
					case igCommandId.kSetRenderTargetMask:
					{
						igCommandSetRenderTargetMaskParameters parameters = (igCommandSetRenderTargetMaskParameters)command._parameters!;
						igGraphicsRenderTargetMask? mask = parameters._resource as igGraphicsRenderTargetMask;
						if (mask == null)
						{
							Logging.Warn("Failed to find the render target mask for a {0} command", command._commandId);
							continue;
						}
						decompiled._renderTargetMask = mask._renderTargetMask;
						break;
					}
					default:
						throw new Exception("Not handling this one missie");
				}
			}

			return decompiled;
		}


		/// <summary>
		/// Compiles a material to a series of commants
		/// </summary>
		/// <param name="material">The material</param>
		/// <param name="stream">The output stream</param>
		/// <exception cref="NotImplementedException">If a constant value type is unimplemeted</exception>
		private void CompileMaterial(DecompiledMaterial material, igMemoryCommandStream stream)
		{
			stream._commands.Clear();

			if (material._stencilRef.HasValue)
			{
				var stencilRefParams = new igCommandSetStencilRefParameters();
				stencilRefParams._stencilRef = material._stencilRef.Value;
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetStencilRef, stencilRefParams));
			}

			if (material._stencilStateBundle.HasValue)
			{
				var stencilBundleParams = new igCommandSetStencilStateBundleParameters();
				igGraphicsStencilStateBundle stencilStateBundle = new igGraphicsStencilStateBundle();
				stencilStateBundle._stencilStateBundle = material._stencilStateBundle.Value;
				stencilBundleParams._resource = _graphicsObjects.GetOrAddGraphicsObject(stencilStateBundle);
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetStencilStateBundle, stencilBundleParams));
			}

			if (material._blendStateBundle.HasValue)
			{
				var blendStateBundleParams = new igCommandSetBlendStateBundleParameters();
				igGraphicsBlendStateBundle blendStateBundle = new igGraphicsBlendStateBundle();
				blendStateBundle._blendStateBundle = material._blendStateBundle.Value;
				blendStateBundleParams._resource = _graphicsObjects.GetOrAddGraphicsObject(blendStateBundle);
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetBlendStateBundle, blendStateBundleParams));
			}

			for (int i = 0; i < material._textures.Count; i++)
			{
				DecompiledTexture dTexture = material._textures[i];

				igGraphicsTexture gTexture = new igGraphicsTexture();
				gTexture._image       = dTexture._image;
				gTexture._imageHandle = dTexture._imageHandle;
				gTexture._usage       = dTexture._usage;

				gTexture = _graphicsObjects.GetOrAddGraphicsObject(gTexture);

				var setTextureParams = new igCommandSetPixelShaderTextureParameters();
				setTextureParams._register = dTexture._register;
				setTextureParams._resource = gTexture;

				igGraphicsSamplerStateBundle gSampler = new igGraphicsSamplerStateBundle();
				gSampler._samplerStateBundle = dTexture._samplerStateBundle;

				gSampler = _graphicsObjects.GetOrAddGraphicsObject(gSampler);

				var setSamplerParams = new igCommandSetPixelShaderSamplerParameters();
				setSamplerParams._register = dTexture._register;
				setSamplerParams._resource = gSampler;

				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetPixelShaderTexture, setTextureParams));
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetPixelShaderSampler, setSamplerParams));
			}

			igMetaObject? valueListMeta = igArkCore.GetObjectMeta(nameof(igShaderConstantValueList));
			if (valueListMeta != null)
			{
				igShaderConstantValueList valueList = (igShaderConstantValueList)valueListMeta.ConstructInstance(igMemoryContext.Singleton._pools["Default"]);
				for (int c = 0; c < material._constants.Count; c++)
				{
					if (material._constants[c]._name.StartsWith(_effectHandle._namespace._string))
					{
						valueList._values.Append(material._constants[c].GetShaderValue(_graphicsObjects));
					}
				}

				if (valueList._values._count != 0)
				{
					var valueListParams = new igCommandApplyConstantValueListParameters();
					valueListParams._list = _graphicsObjects.GetOrAddGraphicsObject(valueList);

					stream._commands.Add(new igCommandStream.igCommand(igCommandId.kApplyConstantValueList, valueListParams));
				}
			}


			for (int b = 0; b < material._bundles.Count; b++)
			{
				var applyBundleParameters = new igCommandApplyConstantBundleParameters();
				applyBundleParameters._bundle = _graphicsObjects.GetOrAddGraphicsObject(material._bundles[b]);
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kApplyConstantBundle, applyBundleParameters));
			}

			for (int c = 0; c < material._constants.Count; c++)
			{
				DecompiledConstant constant = material._constants[c];

				if (material._rasterizerStateBundle.HasValue && constant._name == "ig_cullface_enable")
				{
					igGraphicsRasterizerStateBundle rasterizerStateBundle = new igGraphicsRasterizerStateBundle();
					rasterizerStateBundle._rasterizerStateBundle = material._rasterizerStateBundle.Value;

					var rasterizerStateBundleParams = new igCommandSetRasterizeStateBundleParameters();
					rasterizerStateBundleParams._resource = _graphicsObjects.GetOrAddGraphicsObject(rasterizerStateBundle);

					stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetRasterizeStateBundle, rasterizerStateBundleParams));
				}

				if (!constant._name.StartsWith(_effectHandle._namespace._string))
				{
					igGraphicsShaderConstant shaderConstant = new igGraphicsShaderConstant();
					shaderConstant._name = constant._name;
					shaderConstant = _graphicsObjects.GetOrAddGraphicsObject(shaderConstant);

					object setValueParameters;
					igCommandId setValueType;
					switch (constant._type)
					{
						case ConstantType.Bool:
							setValueParameters = new igCommandSetConstantBoolParameters()
							{
								_resource = shaderConstant,
								_value = (bool)constant._value
							};
							setValueType = igCommandId.kSetConstantBool;
							break;
						case ConstantType.Int:
							setValueParameters = new igCommandSetConstantIntParameters()
							{
								_resource = shaderConstant,
								_value = (int)constant._value
							};
							setValueType = igCommandId.kSetConstantInt;
							break;
						case ConstantType.Float:
							setValueParameters = new igCommandSetConstantFloatParameters()
							{
								_resource = shaderConstant,
								_value = (float)constant._value
							};
							setValueType = igCommandId.kSetConstantFloat;
							break;
						case ConstantType.Vec4f:
							setValueParameters = new igCommandSetConstantVec4fParameters()
							{
								_resource = shaderConstant,
								_value = (igVec4f)constant._value
							};
							setValueType = igCommandId.kSetConstantVec4f;
							break;
						case ConstantType.Matrix44f:
							setValueParameters = new igCommandSetConstantMatrix44fParameters()
							{
								_resource = shaderConstant,
								_value = (igMatrix44f)constant._value
							};
							setValueType = igCommandId.kSetConstantMatrix44f;
							break;
						default:
							throw new NotImplementedException("Unimplemented shader constant type");
					}

					stream._commands.Add(new igCommandStream.igCommand(setValueType, setValueParameters));
				}
			}

			if (material._depthStateBundle.HasValue)
			{
				igGraphicsDepthStateBundle depthStateBundle = new igGraphicsDepthStateBundle();
				depthStateBundle._depthStateBundle = material._depthStateBundle.Value;

				var depthStateBundleParams = new igCommandSetDepthStateBundleParameters();
				depthStateBundleParams._resource = _graphicsObjects.GetOrAddGraphicsObject(depthStateBundle);
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetDepthStateBundle, depthStateBundleParams));
			}

			if (material._commonRenderState.HasValue)
			{
				var renderStateParams = new igCommandSetCommonRenderStateParameters();
				renderStateParams._commonRenderState = material._commonRenderState.Value;
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetCommonRenderState, renderStateParams));
			}

			if (material._renderTargetMask.HasValue)
			{
				igGraphicsRenderTargetMask renderTargetMask = new igGraphicsRenderTargetMask();
				renderTargetMask._renderTargetMask = material._renderTargetMask.Value;

				var renderTargetMaskParams = new igCommandSetRenderTargetMaskParameters();
				renderTargetMaskParams._resource = _graphicsObjects.GetOrAddGraphicsObject(renderTargetMask);
				stream._commands.Add(new igCommandStream.igCommand(igCommandId.kSetRenderTargetMask, renderTargetMaskParams));
			}
		}
	}
}