/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Runtime.InteropServices;
using System.Diagnostics;

namespace igLibrary.DotNet
{
	public static class VvlLoader
	{
		/* Steps:
		 * Read Header
		 * Read sections into buffers
		 * Read DotNetMethodMeta array
		 * Read DotNetTypeDetails array
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * DotNet more like DeezNuts amirite 
		*/
		public static DotNetLibrary Load(string libName, DotNetRuntime runtime, out bool success)
		{
			DotNetLibrary? library;
			if(CDotNetaManager._Instance._libraries.TryGetValue(libName, out library))
			{
				success = true;
				return library;
			}
			//Open file
			igFileContext.Singleton.Open(libName, igFileContext.GetOpenFlags(FileAccess.Read, FileMode.Open), out igFileDescriptor fd, igBlockingType.kMayBlock, igFileWorkItem.Priority.kPriorityNormal);
			StreamHelper sh = new StreamHelper(fd._handle, StreamHelper.Endianness.Little);

			//Endianness
			if(sh.ReadUInt32() == 0x08000000) sh._endianness = StreamHelper.Endianness.Big;

			//Read Header
			sh.Seek(0);
			VvlHeader header = sh.ReadStruct<VvlHeader>();			
			Debug.Assert(header._sizeofSize == 0x28);

			//Prepare library and resolver
			library = new DotNetLibrary();
			library._isLittleEndian = sh._endianness == StreamHelper.Endianness.Little;
			library._runtime = runtime;
			library._path = libName;
			igDotNetLoadResolver resolver = new igDotNetLoadResolver();
			resolver._runtime = runtime;

			//Read the file's sections
			resolver._stringTable = new StreamHelper(sh.ReadBytes(header._stringTableLength));
			StreamHelper strings = resolver._stringTable;
			VvlMethodDef[] methodRefs = sh.ReadStructArray<VvlMethodDef>(header._numMethodRefs);
			VvlFieldDefinition[] fieldDefs = sh.ReadStructArray<VvlFieldDefinition>(header._fieldCount);
			SDotNetMethodMeta[] sMethodMetas = sh.ReadStructArray<SDotNetMethodMeta>(header._methodCount);
			VvlParameterMeta[] paramMetas = sh.ReadStructArray<VvlParameterMeta>(header._parameterCount);
			VvlAttribute[] attributeMetas = sh.ReadStructArray<VvlAttribute>(header._attributeCount);
			sh.Seek(header.unk20 * 0x04, SeekOrigin.Current);
			StreamHelper bTypeDetails = new StreamHelper(sh.ReadBytes(header._typeDetailLength), sh._endianness);

			sh.Close();
			sh.Dispose();

			VvlTypeDetailHeader typeHeader = bTypeDetails.ReadStruct<VvlTypeDetailHeader>();
			Debug.Assert(typeHeader._sizeofSize == 0x50);

			//Method metadata pass
			Debug.Assert(typeHeader._methodCount == header._methodCount);

			DotNetMethodMetaList methodMetas = new DotNetMethodMetaList();
			methodMetas.SetCapacity((int)header._methodCount);

			bTypeDetails.Seek(typeHeader._methodDefOffset);
			VvlMethodDef[] methodDefs = bTypeDetails.ReadStructArray<VvlMethodDef>(typeHeader._methodCount);
			for(int i = 0; i < typeHeader._methodCount; i++)
			{
				DotNetMethodMeta methodMeta = new DotNetMethodMeta();
				
				methodMeta._importTag = ReadVvlString(strings, sMethodMetas[i]._importTag);
				methodMeta._methodName = ReadVvlString(strings, sMethodMetas[i]._methodName);
				methodMeta._entryPoint = ReadVvlString(strings, sMethodMetas[i]._entryPoint);
				methodMeta._return = ConvertParameterMeta(paramMetas[sMethodMetas[i]._retParamIndex], attributeMetas, strings);

				int paramCount = methodDefs[i]._paramCount;
				methodMeta._parameters.SetCapacity(paramCount);
				for(int j = 0; j < paramCount; j++)
				{
					methodMeta._parameters.Append(ConvertParameterMeta(paramMetas[sMethodMetas[i]._paramIndex + j], attributeMetas, strings));
				}

				methodMetas.Append(methodMeta);
			}

			//Dereference these to hopefully free them?
			sMethodMetas = null;
			paramMetas = null;

			//Read types
			bTypeDetails.Seek(0x50);
			library._stringTable = new StreamHelper(bTypeDetails.ReadBytes(typeHeader._stringTableLength));
			library._ownedTypes.SetCapacity(typeHeader._ownedTypeCount);
			DotNetTypeDetailsList typeDetails = new DotNetTypeDetailsList();
			bTypeDetails.Seek(typeHeader._ownedTypeOffset);
			VvlTypeDetails[] vvltds = bTypeDetails.ReadStructArray<VvlTypeDetails>((uint)typeHeader._ownedTypeCount);
			bTypeDetails.Seek(typeHeader._genericTypeOffset);
			VvlGenericTypeDetails[] vvlgtds = bTypeDetails.ReadStructArray<VvlGenericTypeDetails>((uint)typeHeader._genericTypeCount);

			uint[] memberStarts = new uint[vvltds.Length + vvlgtds.Length];
			uint[] memberCounts = new uint[vvltds.Length + vvlgtds.Length];

			for(int i = 0; i < typeHeader._ownedTypeCount; i++)
			{
				memberStarts[i] = vvltds[i]._fieldStartIndex;
				memberCounts[i] = vvltds[i]._memberCount;
				DotNetTypeDetails dntd = ConvertTypeDetails(resolver, vvltds[i], strings, fieldDefs, attributeMetas, bTypeDetails);
				if(dntd != null)
				{
					typeDetails.Append(dntd);
				}
			}

			for(int i = 0; i < typeHeader._genericTypeCount; i++)
			{
				DotNetTypeDetails dntd = new DotNetTypeDetails();
				dntd._interfaceOffset = 0;
				dntd._interfaceCount = 0;
				dntd._templateParameterOffset = vvlgtds[i]._templateParameterOffset;
				dntd._templateParameterCount = vvlgtds[i]._templateParameterCount;
				string typeSpecName = ReadVvlString(strings, vvlgtds[i]._typeSpecName);
				typeSpecName = resolver._runtime._prefix + typeSpecName.TrimStart('.');
				
				memberStarts[typeHeader._ownedTypeCount + i] = 0;
				memberCounts[typeHeader._ownedTypeCount + i] = 0;
				igDotNetTypeSpecMetaObject? typeSpec = (igDotNetTypeSpecMetaObject?)igArkCore.GetObjectMeta(typeSpecName);
				if(typeSpec == null)
				{
					typeSpec = new igDotNetTypeSpecMetaObject();
					typeSpec._name = typeSpecName;
				}
				else
				{
					dntd._ownsMeta = false;
				}
				dntd._targetMeta = typeSpec;
				typeSpec._owners.Append(library);
				resolver.AppendMeta(typeSpec);
				if(typeSpec._name != typeSpecName)
				{
					resolver._aliases.TryAdd(typeSpecName, typeSpec._name);
				}
				igDotNetTypeReference baseTypeRef = new igDotNetTypeReference(resolver, vvlgtds[i]._isArray != 0, vvlgtds[i]._elementType, ReadVvlString(strings, vvlgtds[i]._someTypeReference));
				dntd._baseType = baseTypeRef;
				typeDetails.Append(dntd);
			}

			igVector<int> incompleteObjects = new igVector<int>();

			for(int i = 0; i < typeDetails._count; i++)
			{
				if(!typeDetails[i]._ownsMeta) continue;
				if(typeDetails[i]._targetMeta is igMetaObject metaObject)
				{
					incompleteObjects.Append(i);
				}
				else
				{
					igBaseMeta targetMeta = typeDetails[i]._targetMeta;
					if(targetMeta is igDotNetDynamicMetaEnum dndme)
					{
						dndme._owner = library;
					}
					library._ownedTypes.Append(targetMeta);
				}
			}

			resolver.AddTypes(library);

			bTypeDetails.Seek(typeHeader._staticFieldOffset);
			VvlFieldDefinition[] staticFields = bTypeDetails.ReadStructArray<VvlFieldDefinition>((uint)typeHeader._staticFieldCount);
			library._staticFields.SetCapacity(typeHeader._staticFieldCount);
			for(int i = 0; i < typeHeader._staticFieldCount; i++)
			{
				DotNetFieldDefinition dnFieldDef = new DotNetFieldDefinition();
				dnFieldDef.Name = ReadVvlString(strings, staticFields[i]._name);
				dnFieldDef.Flags = staticFields[i]._flags;
				dnFieldDef.Data = new DotNetData();
				if(staticFields[i]._fieldType == ElementType.kElementTypeString)
				{
					dnFieldDef.Data._type._flags = (uint)ElementType.kElementTypeString | (uint)DotNetType.Flags.kIsSimple;
					if(staticFields[i]._default != 0)
					{
						dnFieldDef.Data._data = ReadVvlString(strings, (uint)(staticFields[i]._default - 1));
					}
				}
				else if(staticFields[i]._fieldType == ElementType.kElementTypeObject && staticFields[i]._isArray == 0)
				{
					igDotNetTypeReference dntr = new igDotNetTypeReference(resolver, false, staticFields[i]._fieldType, ReadVvlString(resolver._stringTable, staticFields[i]._refTypeName));
					if(!dntr.TryResolveObject(out dnFieldDef.Data._type))
					{
						dnFieldDef.Data._type._baseMeta = igArkCore.GetObjectMeta("Object");
						dnFieldDef.Data._type._flags = (uint)ElementType.kElementTypeObject;
					}
				}
				else
				{
					dnFieldDef.Data._type._flags = 0;
					if(staticFields[i]._fieldType != ElementType.kElementTypeObject)
					{
						dnFieldDef.Data._type._flags = (uint)DotNetType.Flags.kIsSimple;
					}
					dnFieldDef.Data._type._flags |= (uint)staticFields[i]._fieldType | (staticFields[i]._isArray != 0 ? 0 : (uint)DotNetType.Flags.kIsArray);
					dnFieldDef.Data._representation = staticFields[i]._dataRep;
				}
				library._staticFields.Append(dnFieldDef);
			}

			//Read fields of types
			for(int i = 0; i < incompleteObjects._count; i++)
			{
				int metaIndex = incompleteObjects[i];
				igMetaObject meta = (igMetaObject)typeDetails[metaIndex]._targetMeta;
				if(meta is igDotNetDynamicMetaObject dynamicMeta)
				{
					//You'd implement igPatchAttribute here

					bool importSuccess = false;
					igDotNetTypeReference typeRef = typeDetails[metaIndex]._baseType;
					DotNetType baseType = new DotNetType();
					while(!importSuccess)
					{
						importSuccess = typeRef.TryResolveObject(out baseType);
						if(!importSuccess)
						{
							int nsIndex = typeRef._name.IndexOf('.');
							if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
							//else throw new TypeLoadException($"Failed to find class {typeName}");
							else break;
						}
					}
					if(!importSuccess)
					{
						baseType._baseMeta = igArkCore.GetObjectMeta("Object");	//Should replace this with DotNet.Object._Meta when that's implemented
						baseType._flags = (uint)ElementType.kElementTypeObject;
					}
					if(baseType._baseMeta is not igMetaObject baseMetaObject)
					{
						throw new TypeLoadException($"Somehow didn't get a metaobject for the parent of {meta._name}, instead got {baseType._baseMeta._name}");
					}
					else
					{
						if(baseMetaObject._parent == null)
						{
							incompleteObjects.Append(metaIndex);
							continue;
						}
						meta._parent = baseMetaObject;
						meta.InheritFields();
						for(int j = 0; j < memberCounts[metaIndex]; j++)
						{
							meta.AppendDynamicField(AddField(library, resolver, fieldDefs[memberStarts[metaIndex] + j], strings));
						}

						// The sunburn exception... (Issue #41)
						// This exists because sunburn is bad and for some reason internally uses a CTriggerVolumeSphereComponentData,
						// this gets cast into a CTriggerVolumeCapsuleComponentData, which shouldn't be allowed, however the Vvl runner
						// has no type safety, so this is allowed.
						// I've added this special case to cope, it manually modifies the problematic field, I just wish there was a better way
						if(meta._name == "Scripts.Sunburn_FlamethrowerComponentData")
						{
							igObjectRefMetaField contactTriggerVolumeField = (igObjectRefMetaField)meta.GetFieldByName("ContactTriggerVolume")!;
							contactTriggerVolumeField._metaObject = igArkCore.GetObjectMeta("CTriggerVolumeCapsuleComponentData")!;
						}

						((igDotNetDynamicMetaObject)meta)._owner = library;
					}
				}
				else if(meta is igDotNetMetaObject dnmo)
				{
					dnmo._wrappedIn = library;
				}
				library._ownedTypes.Append(meta);
			}

			library._referencedTypes.SetCapacity(typeHeader._referencedTypeCount);
			for(int i = 0; i < typeHeader._referencedTypeCount; i++)
			{
				bTypeDetails.Seek(typeHeader._referencedTypeOffset + i * 0xC);
				igDotNetTypeReference dntr = new igDotNetTypeReference();
				dntr._resolver = resolver;
				dntr._elementType = (ElementType)bTypeDetails.ReadUInt32();
				dntr._isArray = bTypeDetails.ReadUInt32() == 1;
				resolver._stringTable.Seek(bTypeDetails.ReadUInt32());
				string typeName = dntr._name = resolver._stringTable.ReadString();

				DotNetType dnt = new DotNetType();

				if(dntr._elementType == ElementType.kElementTypeObject && !dntr._isArray)
				{
					bool escape = false;
					while(!escape)
					{
						if(!dntr.TryResolveObject(out dnt))
						{
							int nsIndex = dntr._name.IndexOf('.');
							if(nsIndex >= 0) dntr._name = dntr._name.Substring(nsIndex+1);
							else
							{
								library._referencedTypes.Append(new DotNetType());
								escape = true;
							}
						}
						else break;
					}
					if(escape) continue;
				}
				else
				{
					if(dntr._elementType != ElementType.kElementTypeObject) dnt._flags = (uint)DotNetType.Flags.kIsSimple;
					dnt._flags = ((uint)dntr._elementType) | (dntr._isArray ? (uint)DotNetType.Flags.kIsArray : 0);
				}
				library._referencedTypes.Append(dnt);
			}

			for(int i = 0; i < typeDetails._count; i++)
			{
				if(typeDetails[i]._targetMeta is not igDotNetTypeSpecMetaObject metaObject) continue;

				metaObject._templateParameters = new DotNetTypeList();
				metaObject._templateParameters.SetCapacity(typeDetails[i]._templateParameterCount);
				for(int j = 0; j < typeDetails[i]._templateParameterCount; j++)
				{
					metaObject._templateParameters.Append(library._referencedTypes[typeDetails[i]._templateParameterOffset + j]);
				}
			}

			//Read method defs
			bTypeDetails.Seek(typeHeader._ILOffset);
			StreamHelper IL = new StreamHelper(new MemoryStream(bTypeDetails.ReadBytes(typeHeader._ILCount)), bTypeDetails._endianness);
			Dictionary<igObject, int> methodLookup = new Dictionary<igObject, int>();
			library._methodDefs.SetCapacity((int)typeHeader._methodCount);
			for(int i = 0; i < typeHeader._methodCount; i++)
			{
				DotNetMethodDefinition? dnMethodDef = ConvertMethodDef(resolver, methodDefs[i], strings, out int methodIndex, library, IL);
				
				if(dnMethodDef != null)
				{
					dnMethodDef._methodMeta = methodMetas[i];	//Not real but it's good to have it
					//TODO: Finish this
				}

				library._methodDefs.Append(dnMethodDef);
			}

			//There's code here
			
			library._methodRefs.SetCapacity((int)typeHeader._methodRefCount);
			for(int i = 0; i < typeHeader._methodRefCount; i++)
			{
				//string typeName = ReadVvlString(strings, methodRefs[i]._declaringTypeName);
				//string methodName = ReadVvlString(strings, methodRefs[i]._methodName);
				//ResolveMethod(resolver, out DotNetMethodDefinition methodRef, typeName, methodName);

				DotNetMethodDefinition? dnMethodRef = ConvertMethodDef(resolver, methodRefs[i], strings, out int methodIndex, library, null);

				library._methodRefs.Append(dnMethodRef);
			}

			library._fields.SetCapacity((int)typeHeader._fieldCount);
			for(uint i = 0; i < typeHeader._fieldCount; i++)
			{
				string fieldHandle = ReadVvlString(strings, bTypeDetails.ReadUInt32(typeHeader._fieldOffset + i * 4));
				string typeName = fieldHandle.Substring(0, fieldHandle.LastIndexOf('.'));
				string fieldName = fieldHandle.Substring(typeName.Length + 1);
				bool successfulImport = false;
				DotNetType dnt = default(DotNetType);
				igDotNetTypeReference typeRef = new igDotNetTypeReference(resolver, false, ElementType.kElementTypeObject, typeName);
				while(!successfulImport)
				{
					successfulImport = typeRef.TryResolveObject(out dnt);
					if(!successfulImport)
					{
						int nsIndex = typeRef._name.IndexOf('.');
						if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
						else throw new TypeLoadException($"Failed to find class {typeName}");
					}
					else
					{
						typeName = typeRef._name;
						break;
					}
				}
				if(!successfulImport)
				{
					dnt._baseMeta = igArkCore.GetObjectMeta("Object");
					dnt._flags = (uint)ElementType.kElementTypeObject;
				}
				if(fieldName[0] == '<')
				{
					fieldName = fieldName.Substring(1, fieldName.LastIndexOf('>') - 1);
				}
				igMetaField? field = dnt._baseMeta.GetFieldByName(fieldName);
				if(field == null)
				{
					Logging.Error("{0}.{1} (from {2}) could not be loaded! file: {3}", dnt._baseMeta._name, fieldName, fieldHandle, libName);
				}
				library._fields.Append(field);
			}

			success = true;
			CDotNetaManager._Instance._libraries.Add(libName, library);

			resolver.FinalizeTypes(library);

			return library;
		}
		public static string ReadVvlString(StreamHelper strings, uint offset)
		{
			strings.Seek(offset);
			return strings.ReadString();
		}

		public static DotNetParameterMeta ConvertParameterMeta(VvlParameterMeta paramMeta, VvlAttribute[] attributes, StreamHelper strings)
		{
			DotNetParameterMeta dnpm = new DotNetParameterMeta();
			dnpm._name = ReadVvlString(strings, paramMeta._name);
			//TODO: Read Attributes
			return dnpm;
		}
		public static DotNetTypeDetails ConvertTypeDetails(igDotNetLoadResolver resolver, VvlTypeDetails vvltd, StreamHelper strings, VvlFieldDefinition[] fields, VvlAttribute[] attributeMetas, StreamHelper bTypeDetails)
		{
			//Should probably put in checks to verify that the metaobject/metaenum is what we expect it to be

			DotNetTypeDetails dntd = new DotNetTypeDetails();
			dntd._interfaceOffset = vvltd._interfaceOffset;
			dntd._interfaceCount = vvltd._interfaceCount;
			string typeName = ReadVvlString(strings, vvltd._name).TrimStart('.');
			if(resolver._runtime._prefix != null)
			{
				typeName = resolver._runtime._prefix + typeName;
			}

			igBaseMeta? meta = null;

			if(vvltd._elementType == ElementType.kElementTypeI4)	//If enum
			{
				if((vvltd._typeFlag & 1) == 0)	//If owned
				{
					igDotNetDynamicMetaEnum dndme = new igDotNetDynamicMetaEnum();
					meta = dndme;
					dndme._flags = false;
					dndme._name = typeName;
					dndme._values.Capacity = vvltd._memberCount;
					dndme._names.Capacity = vvltd._memberCount;
					for(int i = 0; i < vvltd._memberCount; i++)
					{
						VvlFieldDefinition field = fields[vvltd._fieldStartIndex + i];
						dndme._names.Add(ReadVvlString(strings, field._name));
						dndme._values.Add(field._default);
					}
					dndme.PostUndump();
					resolver.AppendMeta(dndme);
				}
				else
				{
					//dntd._ownsMeta = false;
					string trimmedTypeName = typeName;
					while(true)
					{
						meta = igArkCore.GetMetaEnum(trimmedTypeName);
						if(meta == null)
						{
							int nsIndex = trimmedTypeName.IndexOf('.');
							if(nsIndex >= 0) trimmedTypeName = trimmedTypeName.Substring(nsIndex+1);
							else break;
							continue;
						}
						else break;
					}
				}

				if(meta != null && typeName != meta._name)
				{
					resolver._aliases.TryAdd(typeName, meta._name);
				}
			}
			else	//If metaobject
			{
				if((vvltd._typeFlag & 1) == 0)	//If owned
				{
					igDotNetDynamicMetaObject dndmo = new igDotNetDynamicMetaObject();
					dndmo._name = typeName;
					meta = dndmo;
					resolver.AppendMeta(dndmo);
				}
				else
				{
					//dntd._ownsMeta = false;
					bool successfulImport = false;
					igDotNetTypeReference typeRef = new igDotNetTypeReference(resolver, false, ElementType.kElementTypeObject, typeName);
					while(!successfulImport)
					{
						successfulImport = typeRef.TryResolveObject(out DotNetType dnt);
						if(!successfulImport)
						{
							int nsIndex = typeRef._name.IndexOf('.');
							if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
							//else throw new TypeLoadException($"Failed to find class {typeName}");
							else break;
						}
						else
						{
							meta = dnt._baseMeta;
							typeName = typeRef._name;
							break;
						}
					}
				}
			}

			//if(meta == null) throw new TypeLoadException($"Somehow failed to load {typeName}, I don't really know how it got past the previous checks");
			if(meta == null)
			{
				Logging.Error("FAILED TO LOAD {0} SOMETIMES THIS IS MEANT TO HAPPEN IDK", typeName);
				return null;
			}

			dntd._targetMeta = meta;
			
			if(typeName != meta._name)
			{
				resolver._aliases.TryAdd(typeName, meta._name);
			}

			if(meta is igMetaObject metaObject)
			{
				igDotNetTypeReference baseTypeRef = new igDotNetTypeReference(resolver, vvltd._isArray != 0, vvltd._elementType, ReadVvlString(strings, vvltd._typeReferenceName));
				dntd._baseType = baseTypeRef;
				if(dntd._baseType.TryResolveObject(out DotNetType baseDnt))
				{
					metaObject._parent = (igMetaObject)baseDnt._baseMeta;
				}
			}

			//TODO: Read attributes

			return dntd;
		}

		private static unsafe igMetaField AddField(DotNetLibrary library, igDotNetLoadResolver resolver, VvlFieldDefinition field, StreamHelper strings)
		{
			igMetaField metaField = null;
			if(field._isArray != 0)
			{
				switch(field._fieldType)
				{
					case ElementType.kElementTypeBoolean:
						metaField = new igBoolArrayMetaField();
						((igBoolArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = false;
						break;
					case ElementType.kElementTypeI1:
						metaField = new igCharArrayMetaField();
						((igCharArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (sbyte)0;
						break;
					case ElementType.kElementTypeU1:
						metaField = new igUnsignedCharArrayMetaField();
						((igUnsignedCharArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (byte)0;
						break;
					case ElementType.kElementTypeI2:
						metaField = new igShortArrayMetaField();
						((igShortArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (short)0;
						break;
					case ElementType.kElementTypeU2:
						metaField = new igUnsignedShortArrayMetaField();
						((igUnsignedShortArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (ushort)0;
						break;
					case ElementType.kElementTypeI4:
					case ElementType.kElementTypeValueType:
						metaField = new igIntArrayMetaField();
						((igIntArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (int)0;
						break;
					case ElementType.kElementTypeU4:
						metaField = new igUnsignedIntArrayMetaField();
						((igUnsignedIntArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (uint)0;
						break;
					case ElementType.kElementTypeI8:
						metaField = new igLongArrayMetaField();
						((igLongArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (long)0;
						break;
					case ElementType.kElementTypeU8:
						metaField = new igUnsignedLongArrayMetaField();
						((igUnsignedLongArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (ulong)0;
						break;
					case ElementType.kElementTypeR4:
						metaField = new igFloatArrayMetaField();
						((igFloatArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = (float)0;
						break;
					case ElementType.kElementTypeString:
						metaField = new igStringArrayMetaField();
						((igStringArrayMetaField)metaField)._num = (short)field._default;
						metaField._default = null;
						break;
					case ElementType.kElementTypeClass:
					case ElementType.kElementTypeObject:
						if((field._flags & DotNetFieldDefinition.FieldDefFlags.kHandle) == 0)
						{
							metaField = new igObjectRefArrayMetaField();
							((igObjectRefArrayMetaField)metaField)._num = (short)field._default;
							((igObjectRefArrayMetaField)metaField)._metaObject = igArkCore.GetObjectMeta("igObject");	//replace this with Core.igObject._Meta once that's implemented

						}
						else
						{
							metaField = new igHandleArrayMetaField();
							((igHandleArrayMetaField)metaField)._num = (short)field._default;
						}
						metaField._default = null;
						break;
				}
			}
			else
			{
				switch(field._fieldType)
				{
					case ElementType.kElementTypeBoolean:
						metaField = new igBoolMetaField();
						//metaField._default = *(bool*)&field._default;
						break;
					case ElementType.kElementTypeI1:
						metaField = new igCharMetaField();
						//metaField._default = *(sbyte*)&field._default;
						break;
					case ElementType.kElementTypeU1:
						metaField = new igUnsignedCharMetaField();
						//metaField._default = *(byte*)&field._default;
						break;
					case ElementType.kElementTypeI2:
						metaField = new igShortMetaField();
						//metaField._default = *(short*)&field._default;
						break;
					case ElementType.kElementTypeU2:
						metaField = new igUnsignedShortMetaField();
						//metaField._default = *(ushort*)&field._default;
						break;
					case ElementType.kElementTypeI4:
					case ElementType.kElementTypeValueType:
						metaField = new igIntMetaField();
						//metaField._default = (int)field._default;
						break;
					case ElementType.kElementTypeU4:
						metaField = new igUnsignedIntMetaField();
						//metaField._default = (uint)field._default;
						break;
					case ElementType.kElementTypeI8:
						metaField = new igLongMetaField();
						//metaField._default = (long)0;
						break;
					case ElementType.kElementTypeU8:
						metaField = new igUnsignedLongMetaField();
						//metaField._default = (ulong)0;
						break;
					case ElementType.kElementTypeR4:
						metaField = new igFloatMetaField();
						//metaField._default = (float)0;
						break;
					case ElementType.kElementTypeString:
						metaField = new igStringMetaField();
						//metaField._default = null;
						break;
					case ElementType.kElementTypeClass:
					case ElementType.kElementTypeObject:
						bool importSuccess = false;
						igDotNetTypeReference typeRef = new igDotNetTypeReference(resolver, field._isArray != 0, field._fieldType, ReadVvlString(strings, field._refTypeName));
						DotNetType dntRef = new DotNetType();
						while(!importSuccess)
						{
							importSuccess = typeRef.TryResolveObject(out dntRef);
							if(!importSuccess)
							{
								int nsIndex = typeRef._name.IndexOf('.');
								if(nsIndex >= 0) typeRef._name = typeRef._name.Substring(nsIndex+1);
								//else throw new TypeLoadException($"Failed to find class {typeName}");
								else break;
							}
						}
						if(!importSuccess)
						{
							dntRef._baseMeta = igArkCore.GetObjectMeta("Object");
						}

						if(dntRef._baseMeta is igMetaEnum metaEnum)
						{
							metaField = new igDotNetEnumMetaField();
							((igDotNetEnumMetaField)metaField)._metaEnum = metaEnum;
							((igDotNetEnumMetaField)metaField)._definedMetaEnum = metaEnum;
						}
						else if((field._flags & DotNetFieldDefinition.FieldDefFlags.kHandle) == 0)
						{
							metaField = new igObjectRefMetaField();
							((igObjectRefMetaField)metaField)._metaObject = (igMetaObject)dntRef._baseMeta;	//replace this with Core.igObject._Meta once that's implemented
							//default is handled with a handle? didn't expect that
						}
						else
						{
							metaField = new igHandleMetaField();
							((igHandleMetaField)metaField)._metaObject = (igMetaObject)dntRef._baseMeta;	//replace this with Core.igObject._Meta once that's implemented
						}
						//metaField._default = null;
						break;
				}
			}
			metaField._fieldName = ReadVvlString(strings, field._name);
			if(metaField._fieldName[0] == '<')	//Why would this happen??
			{
				int endIndex = metaField._fieldName.IndexOf('>');
				metaField._fieldName = metaField._fieldName.Substring(1, endIndex-1);
				//Ignoring the rest for now
			}

			//Read attributes
			return metaField;
		}
		public static void ResolveMethod(igDotNetLoadResolver resolver, out DotNetMethodDefinition methodDef, string typeName, string methodName)
		{
			igDotNetTypeReference dntr = new igDotNetTypeReference(resolver, false, ElementType.kElementTypeObject, typeName);
			string trimmedTypeName = typeName;
			DotNetType declaringType;
			while(true)
			{
				if(!dntr.TryResolveObject(out declaringType))
				{
					int nsIndex = dntr._name.IndexOf('.');
					if(nsIndex >= 0) dntr._name = dntr._name.Substring(nsIndex+1);
					else throw new TypeLoadException($"Failed to load referenced type {typeName}");
					continue;
				}
				else break;
			}

			igDotNetDynamicMetaObject dndmo = (igDotNetDynamicMetaObject)declaringType._baseMeta;
			for(int i = 0; i < dndmo._owner._methodDefs._count; i++)
			{
				if(dndmo._owner._methodDefs[i]._name == $"{typeName}.{methodName}")
				{
					methodDef = dndmo._owner._methodDefs[i];
					return;
				}
			}
			if(methodName == ".ctor")
			{
				methodDef = new DotNetMethodDefinition();
				methodDef._declaringType = declaringType;
				methodDef._owner = dndmo._owner;
				methodDef._stackHeight = 1;
				methodDef._name = methodName;
				methodDef._parameters.Append(declaringType);
				methodDef._methodMeta = new DotNetMethodMeta();
				methodDef._methodMeta._parameters.Append(new DotNetParameterMeta() { _name = "this" });
				methodDef._retType = new DotNetType();
				methodDef._flags = (uint)DotNetMethodSignature.FlagTypes.Constructor;
				methodDef._owner._methodDefs.Append(methodDef);
				methodDef._IL.Append(0x2A);	//Ret
				return;
			}
			throw new MissingMethodException($"Failed to load method {methodName} from class {typeName}");
		}
		public static DotNetMethodDefinition? ConvertMethodDef(igDotNetLoadResolver resolver, VvlMethodDef methodDef, StreamHelper strings, out int methodIndex, DotNetLibrary library, StreamHelper? IL)
		{
			if(!TypeResolverHelper(resolver, false, ElementType.kElementTypeObject, ReadVvlString(strings, methodDef._declaringTypeName), out DotNetType declaringType))
			{
				methodIndex = 0;
				return null;
			}

			DotNetMethodDefinition dnMethodDef = new DotNetMethodDefinition();
			dnMethodDef._name = ReadVvlString(strings, methodDef._methodName);
			dnMethodDef._declaringType = declaringType;

			if(!TypeResolverHelper(resolver, methodDef._isReturnArray != 0, methodDef._returnElementType, ReadVvlString(resolver._stringTable, methodDef._returnTypeName), out dnMethodDef._retType))
			{
				dnMethodDef._retType._baseMeta = igArkCore.GetObjectMeta("Object");
				dnMethodDef._retType._flags = (uint)ElementType.kElementTypeObject;
			}

			dnMethodDef._stackHeight = methodDef._stackHeight;
			dnMethodDef._flags = methodDef._flags;
			dnMethodDef._methodIndex = methodDef._methodIndex;
			if(IL != null)
			{
				dnMethodDef._IL = new igVector<byte>();
				dnMethodDef._IL.SetCapacity((int)methodDef._ILCount);
				IL.Seek(methodDef._ILOffset);
				for(int i = 0; i < methodDef._ILCount; i++)
				{
					dnMethodDef._IL.Append(IL.ReadByte());
				}
			}
			methodIndex = dnMethodDef._methodIndex;
			dnMethodDef._owner = library;
			dnMethodDef._parameters.SetCapacity(methodDef._paramCount);
			for(int i = 0; i < methodDef._paramCount; i++)
			{
				dnMethodDef._parameters.Append(library._referencedTypes[methodDef._paramStartIndex + i]);
			}
			dnMethodDef._locals.SetCapacity(methodDef._localCount);
			for(int i = 0; i < methodDef._localCount; i++)
			{
				dnMethodDef._locals.Append(library._referencedTypes[methodDef._localStartIndex + i]);
			}
			return dnMethodDef;
		}
		public static bool TypeResolverHelper(igDotNetLoadResolver resolver, bool isArray, ElementType elementType, string name, out DotNetType dnt)
		{
			if(elementType != ElementType.kElementTypeObject || isArray)
			{
				dnt = new DotNetType();
				dnt._flags = 0;
				if(elementType != ElementType.kElementTypeObject)
				{
					dnt._flags = (uint)DotNetType.Flags.kIsSimple;
				}
				dnt._flags |= (uint)elementType | (isArray ? 0 : (uint)DotNetType.Flags.kIsArray);
				return true;
			}
			igDotNetTypeReference dntr = new igDotNetTypeReference(resolver, isArray, elementType, name);
			while(true)
			{
				if(!dntr.TryResolveObject(out dnt))
				{
					int nsIndex = dntr._name.IndexOf('.');
					if(nsIndex >= 0) dntr._name = dntr._name.Substring(nsIndex+1);
					//else throw new TypeLoadException($"Failed to find class {typeName}");
					else
					{
						return false;
					}
				}
				else break;
			}
			if(dntr._name != name)
			{
				dntr._resolver._aliases.TryAdd(name, dntr._name);
			}
			return true;
		}

		[StructLayout(LayoutKind.Explicit, Size = 0x28)]
		public struct VvlHeader
		{
			[FieldOffset(0x00)] public uint unk00;
			[FieldOffset(0x04)] public uint _sizeofSize;
			[FieldOffset(0x08)] public uint _stringTableLength;
			[FieldOffset(0x0C)] public uint _numMethodRefs;
			[FieldOffset(0x10)] public uint _fieldCount;
			[FieldOffset(0x14)] public uint _methodCount;
			[FieldOffset(0x18)] public uint _parameterCount;
			[FieldOffset(0x1C)] public uint _attributeCount;
			[FieldOffset(0x20)] public uint unk20;
			[FieldOffset(0x24)] public uint _typeDetailLength;
		}
		[StructLayout(LayoutKind.Explicit, Size = 0x18)]
		public struct SDotNetMethodMeta
		{
			[FieldOffset(0x00)] public uint _sizeofSize;
			[FieldOffset(0x04)] public uint _importTag;
			[FieldOffset(0x08)] public uint _methodName;
			[FieldOffset(0x0C)] public uint _entryPoint;
			[FieldOffset(0x10)] public uint _retParamIndex;
			[FieldOffset(0x14)] public uint _paramIndex;
		}
	[StructLayout(LayoutKind.Explicit, Size = 0x54)]
	public struct VvlMethodDef
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _declaringTypeName;
		[FieldOffset(0x10)] public uint _methodName;
		[FieldOffset(0x20)] public int _paramCount;
		[FieldOffset(0x24)] public int _localCount;
		[FieldOffset(0x28)] public int _stackHeight;
		[FieldOffset(0x2C)] public ElementType _returnElementType;
		[FieldOffset(0x30)] public int _isReturnArray;
		[FieldOffset(0x34)] public uint _returnTypeName;
		[FieldOffset(0x40)] public int _paramStartIndex;
		[FieldOffset(0x44)] public int _localStartIndex;
		[FieldOffset(0x48)] public uint _ILOffset;
		[FieldOffset(0x4C)] public uint _ILCount;
		[FieldOffset(0x50)] public ushort _flags;
		[FieldOffset(0x52)] public ushort _methodIndex;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x30)]
	public struct VvlFieldDefinition
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x08)] public DotNetFieldDefinition.FieldDefFlags _flags;
		[FieldOffset(0x18)] public int _default;
		[FieldOffset(0x20)] public ElementType _fieldType;
		[FieldOffset(0x24)] public uint _isArray;
		[FieldOffset(0x28)] public uint _refTypeName;
		[FieldOffset(0x2C)] public DotNetData.DataRepresentation _dataRep;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x10)]
	public struct VvlParameterMeta
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x08)] public uint _attributeIndex;
		[FieldOffset(0x0C)] public uint _attributeCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x14)]
	public struct VvlAttribute
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _name;
		[FieldOffset(0x0C)] public uint unk0C;
		[FieldOffset(0x10)] public uint unk10;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x2C)]
	public struct VvlTypeDetails
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public ElementType _elementType;
		[FieldOffset(0x08)] public uint _isArray;
		[FieldOffset(0x0C)] public uint _typeReferenceName;
		[FieldOffset(0x10)] public int _interfaceOffset;
		[FieldOffset(0x14)] public int _interfaceCount;
		[FieldOffset(0x18)] public uint _name;
		[FieldOffset(0x1C)] public ushort _typeFlag;
		[FieldOffset(0x1E)] public ushort _memberCount;
		[FieldOffset(0x20)] public uint _fieldStartIndex;
		[FieldOffset(0x24)] public uint _attributeIndex;
		[FieldOffset(0x28)] public uint _attributeCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x1C)]
	public struct VvlGenericTypeDetails
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _typeSpecName;
		[FieldOffset(0x08)] public ElementType _elementType;
		[FieldOffset(0x0C)] public uint _isArray;
		[FieldOffset(0x10)] public uint _someTypeReference;
		[FieldOffset(0x14)] public int _templateParameterOffset;
		[FieldOffset(0x18)] public int _templateParameterCount;
	}
	[StructLayout(LayoutKind.Explicit, Size = 0x50)]
	public struct VvlTypeDetailHeader
	{
		[FieldOffset(0x00)] public uint _sizeofSize;
		[FieldOffset(0x04)] public uint _stringTableLength;
		[FieldOffset(0x0C)] public uint _someOffset;
		[FieldOffset(0x10)] public uint _ILOffset;
		[FieldOffset(0x14)] public uint _ILCount;
		[FieldOffset(0x18)] public uint _methodDefOffset;
		[FieldOffset(0x1C)] public uint _methodCount;
		[FieldOffset(0x20)] public uint _methodRefCount;
		[FieldOffset(0x24)] public uint _ownedTypeOffset;
		[FieldOffset(0x28)] public int _ownedTypeCount;
		[FieldOffset(0x2C)] public uint _referencedTypeOffset;
		[FieldOffset(0x30)] public int _referencedTypeCount;
		[FieldOffset(0x34)] public uint _staticFieldOffset;
		[FieldOffset(0x38)] public int _staticFieldCount;
		[FieldOffset(0x3C)] public uint _fieldOffset;
		[FieldOffset(0x40)] public uint _fieldCount;
		[FieldOffset(0x44)] public uint _genericTypeOffset;
		[FieldOffset(0x48)] public int _genericTypeCount;
	}
	}
}