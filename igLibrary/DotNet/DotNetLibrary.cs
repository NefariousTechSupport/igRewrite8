/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class DotNetLibrary : igObject
	{
		public DotNetRuntime _runtime;
		public StreamHelper _stringTable;
		public DotNetMethodDefinitionList _methodRefs = new DotNetMethodDefinitionList();
		public DotNetMethodDefinitionList _methodDefs = new DotNetMethodDefinitionList();
		public igBaseMetaList _ownedTypes = new igBaseMetaList();
		public igBaseMetaList _wrappedTypes = new igBaseMetaList();
		public DotNetTypeList _referencedTypes = new DotNetTypeList();
		public DotNetFieldDefinitionList _staticFields = new DotNetFieldDefinitionList();
		public igMetaFieldList _fields = new igMetaFieldList();
		public igDotNetMetaObjectList _patchClasses = new igDotNetMetaObjectList();
		public bool _isLittleEndian;
		public string _path;
		public DotNetMethodDefinition LookupMethod(uint callToken, out DotNetType[]? templateParameters)
		{
			//callToken structure:
			// shift: 0, bits: 1. if 1, then search method defs, otherwise, method refs
			// shift: 1, bits: 15. method index
			// shift: 16, bits: 16. first template param index

			templateParameters = null;
			if ((callToken & 0xFFFF0000) != 0)
			{
				// For now, we assume there's only one template parameter
				// This is a huge gamble and probably won't work out in the long run
				templateParameters = new DotNetType[1];

				for (int i = 0; i < templateParameters.Length; i++)
				{
					int typeIndex = unchecked((int)(callToken >> 16)) + i;
					if (_referencedTypes._count <= typeIndex)
					{
						// Abort
						templateParameters = null;
						break;
					}

					templateParameters[i] = _referencedTypes[typeIndex];
				}
			}

			if((callToken & 1) == 0)
			{
				return _methodDefs[(int)(callToken >> 1) & 0x7FFF];
			}
			else
			{
				return _methodRefs[(int)(callToken >> 1) & 0x7FFF];
			}
		}
	}
	public class NonRefCountedDotNetLibraryList : igTObjectList<DotNetLibrary>{}	//Technically inherits fro igTNonRefCountedObjectList but in this library there's no point to have that distinction
}