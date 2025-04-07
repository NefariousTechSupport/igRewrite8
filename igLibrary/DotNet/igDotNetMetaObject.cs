/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class igDotNetMetaObject : igMetaObject
	{
		public NameDotNetMethodDefinitionHashTable _methods;
		public igDotNetMetaObjectList _interfaces;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _vTable;    //DotNetVTable
		public igMetaObject _boundMeta;
		public DotNetTypeList _templateParameters;
		public igMetaObject _patchMeta;
		public object? _dotNetFieldNames;
		public object? _cppFieldNames;
		public int _exposedFieldCount;
		public object? _cppMethods;
		public object? _cppMethodNames;
		public int _cppMethodCount;
		public DotNetLibrary? _wrappedIn;	//Added to aid with interop types
		public bool _isInterface;
		public bool _baseMethodsInherited;
		public static igMetaObjectBindingTable _bindings;
		public static Dictionary<string, igBaseMeta> _aliases = new Dictionary<string, igBaseMeta>();
		public static DotNetType _thisPointer;

		public override void PostUndump()
		{
			base.PostUndump();

			igObjectRefMetaField? metaField = GetFieldByName("_meta") as igObjectRefMetaField;

			if (metaField != null)
			{
				metaField._default = this;
			}
		}


		public override igMetaField? GetFieldByName(string name)
		{
			igMetaField? field = base.GetFieldByName(name);
			if (field == null && _cppFieldNames != null)
			{
				igStringRefList dnNames = (igStringRefList)_dotNetFieldNames!;
				for (int i = 0; i < dnNames._count; i++)
				{
					if (dnNames[i] == name)
					{
						igStringRefList cppNames = (igStringRefList)_cppFieldNames;
						field = base.GetFieldByName(cppNames[i]);
						break;
					}
				}
			}
			return field;
		}


		public static igMetaObject? FindType(string name, DotNetRuntime runtime)
		{
			if(string.IsNullOrEmpty(name)) return null;
			string typeName = runtime._prefix + name;
			if(!_aliases.TryGetValue(typeName, out igBaseMeta? meta))
			{
				meta = igArkCore.GetObjectMeta(name);
			}
			return (igMetaObject?)meta;
		}
		public static igMetaEnum? FindEnum(string name, DotNetRuntime runtime)
		{
			if(string.IsNullOrEmpty(name)) return null;
			string typeName = runtime._prefix + name;
			if(!_aliases.TryGetValue(typeName, out igBaseMeta? meta))
			{
				meta = igArkCore.GetMetaEnum(name);
			}
			return (igMetaEnum?)meta;
		}
	}
	public class igDotNetMetaObjectList : igTObjectList<igDotNetMetaObject>{}
}