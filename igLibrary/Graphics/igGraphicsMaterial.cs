/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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

		public class DecompiledConstant
		{
			public string _name;
			public object _value;

			public DecompiledConstant(string name, object value)
			{
				_name = name;
				_value = value;
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
			public igGraphicsStencilStateBundle? _stencilStateBundle = null; 
			public uint? _stencilRef = null; 

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
				}

				for (int t = 0; t < _techniques._count; t++)
				{
					igMemoryCommandStream? technique = _techniques[t];
					if (technique != null)
					{
						technique.Decode(_graphicsObjects);
					}
				}
			}

			GetDecompiledCommonState();
		}

		public DecompiledMaterial GetDecompiledCommonState()
		{
			if (_decompiledCommonState != null)
			{
				return _decompiledCommonState;
			}

			DecompiledMaterial decompiled = DecompileMaterial(_commonState);

			_decompiledCommonState = decompiled;
			return decompiled;
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
					case igCommandId.kSetStencilStateBundle:
					{
						igCommandSetStencilStateBundleParameters parameters = (igCommandSetStencilStateBundleParameters)command._parameters!;
						igGraphicsStencilStateBundle? bundle = parameters._resource as igGraphicsStencilStateBundle;
						if (bundle == null)
						{
							Logging.Warn("Failed to find the bundle for a {0} command", command._commandId);
							continue;
						}
						decompiled._stencilStateBundle = bundle;
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
						igShaderConstantValueList? list = parameters._bundle as igShaderConstantValueList;
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
					default:
						throw new Exception("Not handling this one missie");
				}
			}

			return decompiled;
		}
	}
}