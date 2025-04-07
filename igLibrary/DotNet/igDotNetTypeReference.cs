/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.DotNet
{
	public class igDotNetTypeReference : igObject
	{
		public bool _isArray;
		public string _name;
		public ElementType _elementType;
		public igDotNetLoadResolver _resolver;
		public igDotNetTypeReference(){}
		public igDotNetTypeReference(igDotNetLoadResolver resolver, bool isArray, ElementType elementType, string name)
		{
			_resolver = resolver;
			_isArray = isArray;
			_elementType = elementType;
			_name = name;
		}
		public bool TryResolveObject(out DotNetType dnt) => TryResolveObject(_name, out dnt);
		public bool TryResolveObject(string name, out DotNetType dnt)
		{
			dnt = new DotNetType();
			string typeName = _resolver._runtime._prefix + name;
			if(_resolver._aliases.TryGetValue(typeName, out string alias))
			{
				typeName = alias;
			}
			if(!_resolver._pending.TryGetValue(typeName, out igBaseMeta? meta))
			{
				meta = igDotNetMetaObject.FindType(typeName, _resolver._runtime);
			}
			if(meta == null)
			{
				meta = igDotNetMetaObject.FindEnum(typeName, _resolver._runtime);
			}

			if(meta == null)
			{
				int genericStart = typeName.IndexOf('[');
				int genericEnd = typeName.IndexOf(']');
				int genericCount = typeName.IndexOf('`');
				if(genericStart == -1)	//If there's no generic args in the type name
				{
					if(genericCount > 0) typeName = typeName.Replace('`', '_');
					else return false;
				}
				else
				{
					typeName = typeName.Substring(0, genericStart);
				}
				TryResolveObject(typeName, out dnt);
				meta = dnt._baseMeta;
			}

			//likely missing something
			dnt._baseMeta = meta;
			dnt._flags = (int)ElementType.kElementTypeObject;

			return meta != null;
		}
	}
}