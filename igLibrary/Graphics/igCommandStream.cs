/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;

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
		protected void DecodeIGZ(IG_CORE_PLATFORM platform, StreamHelper stream)
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

			while (stream.Tell() < stream.BaseStream.Length)
			{
				igCommand command = new igCommand();
				stream.Align(sizeof(int));
				command._commandId = (igCommandId)commandIdEnum.GetEnumFromValue(stream.ReadInt32());

				if (command._commandId != igCommandId.kNoop)
				{
					igCompoundMetaFieldInfo? decoderField = QueryForCommandField(command._commandId);
					if (decoderField == null)
					{
						continue;
					}

					stream.Align(decoderField._platformInfo._alignments[platform]);

					// had to reimplement reading, sorry
					command._parameters = decoderField.ConstructInstance(decoderField._vTablePointer);
					for (int f = 0; f < decoderField._fieldList.Count; f++)
					{
						igMetaField field = decoderField._fieldList[f];

						Array? arrayValue = field.IsArray ? Array.CreateInstance(field.GetOutputType(), field.ArrayNum) : null;
						object itemValue = null;

						for (int a = 0; a < (field.IsArray ? field.ArrayNum : 1); a++)
						{
							stream.Align(field.GetAlignment(platform));

							if (field is igSizeTypeMetaField)
							{
								if (is64Bit)
								{
									itemValue = stream.ReadUInt64();
								}
								else
								{
									itemValue = stream.ReadUInt32();
								}
							}
							else if (field is igIntMetaField)
							{
								itemValue = stream.ReadInt32();
							}
							else if (field is igUnsignedIntMetaField)
							{
								itemValue = stream.ReadUInt32();
							}
							else if (field is igUnsignedShortMetaField)
							{
								itemValue = stream.ReadUInt16();
							}
							else if (field is igFloatMetaField)
							{
								itemValue = stream.ReadSingle();
							}
							else if (field is igVec4fMetaField)
							{
								itemValue = new igVec4f(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
							}
							else if (field is igMatrix44fMetaField)
							{
								itemValue = new igMatrix44f(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
							}
							else if (field is igCompoundMetaField compoundField && compoundField._compoundFieldInfo._name == nameof(igCopyTextureParameters))
							{
								igCopyTextureParameters ctParams = new igCopyTextureParameters();
								ctParams._sourceX             = stream.ReadInt32();
								ctParams._sourceY             = stream.ReadInt32();
								ctParams._destinationX        = stream.ReadInt32();
								ctParams._destinationY        = stream.ReadInt32();
								ctParams._width               = stream.ReadInt32();
								ctParams._height              = stream.ReadInt32();
								ctParams._sourceMipLevel      = stream.ReadInt32();
								ctParams._destinationMipLevel = stream.ReadInt32();
								itemValue = ctParams;
							}
							else if (field is igUnsignedCharMetaField)
							{
								itemValue = stream.ReadByte();
							}
							else if (field is igBoolMetaField)
							{
								itemValue = stream.ReadBoolean();
							}
							else if (field is igEnumMetaField enumMetaField)
							{
								itemValue = enumMetaField._metaEnum.GetEnumFromValue(stream.ReadInt32());
							}
							else
							{
								Logging.Warn("Unimplemented metafield type {0}", field.GetType().Name);
							}

							if (arrayValue != null)
							{
								arrayValue.SetValue(itemValue, a);
							}
						}

						field._fieldHandle!.SetValue(command._parameters, field.IsArray ? arrayValue : itemValue);
					}

					if (command._parameters == null)
					{
						Logging.Warn("Command parameters for command id {0} returned null, pretending this didn't happen...", command._commandId);
					}
				}

				_commands.Add(command);
			}
		}


		/// <summary>
		/// Encodes a command stream
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <param name="stream">The stream</param>
		protected void EncodeIGZ(IG_CORE_PLATFORM platform, StreamHelper stream)
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

							if (field is igSizeTypeMetaField)
							{
								if (is64Bit)
								{
									stream.WriteUInt64((ulong)itemValue!);
								}
								else
								{
									stream.WriteUInt32((uint)(ulong)itemValue!);
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
		}
	}
}