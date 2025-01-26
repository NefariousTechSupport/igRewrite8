/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Reflection;
using System.Xml;
using igLibrary.DotNet;

namespace igLibrary.Core
{
	/// <summary>
	/// Class for loading xml files containing ark metadata
	/// </summary>
	public class igArkCoreXmlFile
	{
		/// <summary>
		/// Class for returning errors when parsing the xml
		/// </summary>
		public class ArkCoreXmlError
		{
			public string Message => _message;

			private string _message;


			/// <summary>
			/// Constructor for ArkCoreXmlError, applies string.Format on the message
			/// </summary>
			/// <param name="message">The format string for the message</param>
			/// <param name="args">The arguments for formatting</param>
			public ArkCoreXmlError(string message, params object?[] args)
			{
				_message = string.Format(message, args);
			}
		}


		// lookup for value names to integer values
		private readonly Dictionary<string, int> kFieldMethodLookup = new Dictionary<string, int>()
		{
			{ "value",     0 },
			{ "reference", 1 },
			{ "none",      2 },
			{ "default",   3 },
			{ "max",       4 }
		};


		private XmlDocument _metaobjectDocument;
		private XmlNode _metaobjectRoot;
		private XmlDocument _metaenumDocument;
		private XmlNode _metaenumRoot;
		private XmlDocument _metafieldDocument;
		private XmlNode _metafieldRoot;


		// Lookup tables for looking up types by name
		private Dictionary<string, igMetaObject> _metaobjectLookup;
		private Dictionary<string, igMetaEnum> _metaenumLookup;
		private Dictionary<string, igMetaFieldPlatformInfo> _metafieldLookup;
		private Dictionary<string, igCompoundMetaFieldInfo> _compoundLookup;


		// read only properties for loaded ark data
		public List<igMetaObject> MetaObjects => _metaobjectLookup.Values.ToList();
		public List<igMetaEnum> MetaEnums => _metaenumLookup.Values.ToList();
		public List<igMetaFieldPlatformInfo> MetaFieldPlatformInfos => _metafieldLookup.Values.ToList();
		public List<igCompoundMetaFieldInfo> Compounds => _compoundLookup.Values.ToList();


		/// <summary>
		/// Constructor for igArkCoreXmlFile
		/// </summary>
		internal igArkCoreXmlFile()
		{
			_metaobjectDocument = new XmlDocument();
			_metaenumDocument = new XmlDocument();
			_metafieldDocument = new XmlDocument();
			_metaobjectLookup = new Dictionary<string, igMetaObject>();
			_metaenumLookup = new Dictionary<string, igMetaEnum>();
			_metafieldLookup = new Dictionary<string, igMetaFieldPlatformInfo>();
			_compoundLookup = new Dictionary<string, igCompoundMetaFieldInfo>();
			_metaobjectRoot = null;
		}


		/// <summary>
		/// Loads from the ark core xml files
		/// </summary>
		/// <param name="metaobjectPath">The metaobjects xml file</param>
		/// <param name="metaenumPath">The metaenums xml file</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		public ArkCoreXmlError? Load(string metaobjectPath, string metaenumPath, string metafieldPath)
		{
			ArkCoreXmlError? error = null;

			igArkCore.Reset();

			error = LoadMetaEnums(metaenumPath);
			if (error != null)
			{
				return error;
			}

			error = LoadMetaFieldAlignments(metafieldPath);
			if (error != null)
			{
				return error;
			}

			error = LoadMetaObjectsPass1(metaobjectPath);
			if (error != null)
			{
				return error;
			}

			error = LoadMetaObjectsPass2();
			if (error != null)
			{
				return error;
			}

			return error;
		}


		/// <summary>
		/// Loads the metafields.xml file
		/// </summary>
		/// <param name="filePath">the path to the metafields.xml file</param>
		/// <returns></returns>
		private ArkCoreXmlError? LoadMetaFieldAlignments(string filePath)
		{
			_metafieldDocument.Load(filePath);

			if (_metafieldDocument.FirstChild != _metafieldDocument.LastChild)
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node!", filePath);
			}
			_metafieldRoot = _metafieldDocument.FirstChild!;
			if (_metafieldRoot.Name != "metafields")
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node named \"metafields\"!", filePath);
			}


			for (XmlNode? node = _metafieldRoot.FirstChild; node != null; node = node.NextSibling)
			{
				if (node.Name != "metafield")
				{
					return new ArkCoreXmlError("All root metafield platform info elements must be named \"metafield\".");
				}

				igMetaFieldPlatformInfo platformInfo = new igMetaFieldPlatformInfo();

				XmlNode? nameAttr = node.Attributes!.GetNamedItem("name");
				if (nameAttr == null)
				{
					return new ArkCoreXmlError("All metafield platform info nodes must have a \"name\" attribute");
				}

				platformInfo._name = nameAttr.Value!;
				int capacity = node.ChildNodes.Count;
				platformInfo._sizes.EnsureCapacity(capacity);
				platformInfo._alignments.EnsureCapacity(capacity);

				for (XmlNode? platformNode = node.FirstChild; platformNode != null; platformNode = platformNode.NextSibling)
				{
					if (platformNode.Name != "platforminfo")
					{
						return new ArkCoreXmlError("All metafield platform info subnodes must be named \"platforminfo\"");
					}

					XmlNode? platformAttr = platformNode.Attributes!.GetNamedItem("platform");
					if (platformAttr == null)
					{
						return new ArkCoreXmlError("All platforminfo nodes must have a \"platform\" attribute");
					}
					if (!Enum.TryParse<IG_CORE_PLATFORM>(platformAttr.Value, out IG_CORE_PLATFORM platform))
					{
						return new ArkCoreXmlError("All platforminfo nodes must reference a platform that exists!");
					}

					ParseMetaFieldPropertyUInt(platformNode, "align", out uint align, 0);
					// Allow 0 cos bitfields
					if (align > 0x8000 || (align & (align - 1)) != 0)
					{
						return new ArkCoreXmlError("align field of platforminfo nodes must be between 0 and 0x8000 and must be a power of 2");
					}

					ParseMetaFieldPropertyUInt(platformNode, "size", out uint size, 0);
					// Allow 0 cos bitfields
					if (size > 0xFFFF)
					{
						return new ArkCoreXmlError("size field of platforminfo nodes must be between 0 and 0xFFFF");
					}

					// Don't treat duplicates as an error
					platformInfo._alignments.TryAdd(platform, (ushort)align);
					platformInfo._sizes.TryAdd(platform, (ushort)size);
				}

				_metafieldLookup.Add(platformInfo._name, platformInfo);
			}

			return null;
		}


		/// <summary>
		/// Load metaenums.xml
		/// </summary>
		/// <param name="filePath">The filepath to load the xml from</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? LoadMetaEnums(string filePath)
		{
			_metaenumDocument.Load(filePath);

			if (_metaenumDocument.FirstChild == null)
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node!", filePath);
			}
			_metaenumRoot = _metaenumDocument.FirstChild!;
			if (_metaenumRoot.Name != "metaenums")
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node named \"metaenums\"!", filePath);
			}

			for (XmlNode? node = _metaenumRoot.FirstChild; node != null; node = node.NextSibling)
			{
				if (node.Name != "metaenum")
				{
					return new ArkCoreXmlError("All root metaenum elements must be named \"metaenum\".");
				}

				igMetaEnum metaEnum = new igMetaEnum();

				XmlNode? refnameAttr = node.Attributes!.GetNamedItem("refname");
				if (refnameAttr == null)
				{
					return new ArkCoreXmlError("All metaenum nodes must have a \"refname\" attribute");
				}

				metaEnum._name = refnameAttr.Value;
				metaEnum._names.Capacity  = node.ChildNodes.Count;
				metaEnum._values.Capacity = metaEnum._names.Capacity;
				_metaenumLookup.Add(metaEnum._name!, metaEnum);

				for (XmlNode? memberNode = node.FirstChild; memberNode != null; memberNode = memberNode.NextSibling)
				{
					if (memberNode.Name != "value")
					{
						return new ArkCoreXmlError("All children of \"metaenum\" nodes must be named \"value\"");
					}

					XmlNode? memberName = memberNode.Attributes!.GetNamedItem("name");
					if (memberName == null)
					{
						return new ArkCoreXmlError("All \"value\" nodes must have an \"name\" attribute");
					}

					XmlNode? memberValue = memberNode.Attributes!.GetNamedItem("value");
					if (memberValue == null)
					{
						return new ArkCoreXmlError("All \"value\" nodes must have an \"value\" attribute");
					}

					metaEnum._names.Add(memberName.Value!);

					if (!int.TryParse(memberValue.Value, out int value))
					{
						return new ArkCoreXmlError("The \"value\" attribute of a \"value\" node must be a signed integer");
					}
					metaEnum._values.Add(value);
				}

				metaEnum.PostUndump();
			}

			return null;
		}


		/// <summary>
		/// Pass one of metaobjects.xml, this instantiates all metaobjects and adds them
		/// to a lookup table, the base classes and fields are not loading fully until pass 2
		/// </summary>
		/// <param name="filePath">The filepath to load the xml from</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? LoadMetaObjectsPass1(string filePath)
		{
			_metaobjectDocument.Load(filePath);

			if (_metaobjectDocument.FirstChild != _metaobjectDocument.LastChild)
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node!", filePath);
			}

			// Can't be null
			_metaobjectRoot = _metaobjectDocument.FirstChild!;
			if (_metaobjectRoot.Name != "metaobjects")
			{
				return new ArkCoreXmlError("\"{0}\" must have exactly one root node named \"metaobjects\"!", filePath);
			}

			ArkCoreXmlError? error = null;

			for(XmlNode? node = _metaobjectRoot.FirstChild; node != null; node = node.NextSibling)
			{
				error = ParseMetaObjectNodePass1(node);
				if (error != null)
				{
					break;
				}
			}

			return error;
		}


		/// <summary>
		/// Pass one for a single metaobject node, this instantiates the mode and adds it
		/// to the lookup table, the base class and fields are not assigned until pass 2
		/// </summary>
		/// <param name="node">The xml node corresponding to this metaobject</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaObjectNodePass1(XmlNode node)
		{
			if (node.Name != "metaobject")
			{
				return new ArkCoreXmlError("All root elements must be named \"metaobject\".");
			}

			XmlNode? typeAttribute = node.Attributes!.GetNamedItem("type");
			if (typeAttribute == null)
			{
				return new ArkCoreXmlError("metaobject is missing \"type\" attribute");
			}

			XmlNode? refnameAttribute = node.Attributes!.GetNamedItem("refname");
			if (refnameAttribute == null)
			{
				return new ArkCoreXmlError("metaobject is missing \"refname\" attribute");
			}

			Type? metaobjectType = igArkCore.GetObjectDotNetType(typeAttribute.Value!);
			if (metaobjectType == null)
			{
				return new ArkCoreXmlError("failed to locate metaobject of type \"{0}\"", typeAttribute.Value!);
			}
			if (!metaobjectType.IsAssignableTo(typeof(igMetaObject)))
			{
				return new ArkCoreXmlError("type \"{0}\" is not assignable to igMetaObject, ensure the metaobject type is a class assignable to igMetaObject", typeAttribute.Value);
			}
			if (metaobjectType.IsAbstract)
			{
				return new ArkCoreXmlError("type \"{0}\" is abstract and cannot be instantiated, ensure the metaobject type is instantiable", typeAttribute.Value);
			}

			igMetaObject metaobject = (igMetaObject?)Activator.CreateInstance(metaobjectType)!;
			metaobject._name = refnameAttribute.Value;
			_metaobjectLookup.Add(metaobject._name!, metaobject);

			for (XmlNode? compoundNode = node.FirstChild; compoundNode != null; compoundNode = compoundNode.NextSibling)
			{
				// This is compound pass 1
				if (compoundNode.Name == "compoundfields")
				{
					igCompoundMetaFieldInfo compoundInfo = new igCompoundMetaFieldInfo();
					compoundInfo._name = refnameAttribute.Value;
					_compoundLookup.Add(compoundInfo._name!, compoundInfo);
				}
			}

			return null;
		}


		/// <summary>
		/// Pass 2 for metaobjects.
		/// Assigns base types for each metaobject and assigns fields.
		/// </summary>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? LoadMetaObjectsPass2()
		{
			ArkCoreXmlError? error = null;

			for(XmlNode? metaobjectNode = _metaobjectRoot.FirstChild; metaobjectNode != null; metaobjectNode = metaobjectNode.NextSibling)
			{
				error = ParseMetaObjectNodePass2(metaobjectNode, _metaobjectLookup[metaobjectNode.Attributes!.GetNamedItem("refname")!.Value!]);
				if (error != null)
				{
					break;
				}
			}

			return error;
		}


		/// <summary>
		/// Pass 2 for metaobjects.
		/// Assigns base types on each igMetaObject, and assigns fields.
		/// </summary>
		/// <param name="node">The xml node to parse</param>
		/// <param name="metaobject">The corresponding metaobject</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaObjectNodePass2(XmlNode node, igMetaObject metaobject)
		{
			XmlNode? basetypeAttr = node.Attributes!.GetNamedItem("basetype");
			if (basetypeAttr != null)
			{
				if (_metaobjectLookup.TryGetValue(basetypeAttr.Value!, out igMetaObject? baseType))
				{
					metaobject._parent = baseType;
					metaobject.InheritFields();

					if (baseType._name == "igDataList" || metaobject._parent._name == "igObjectList" || metaobject._parent._name == "igNonRefCountedObjectList")
					{
						metaobject._metaFields[0] = metaobject._metaFields[0].CreateFieldCopy();
						metaobject._metaFields[1] = metaobject._metaFields[1].CreateFieldCopy();
					}
					else if(metaobject._parent._name == "igHashTable")
					{
						metaobject._metaFields[2] = metaobject._metaFields[2].CreateFieldCopy();
						metaobject._metaFields[3] = metaobject._metaFields[3].CreateFieldCopy();
						metaobject._metaFields[4] = metaobject._metaFields[4].CreateFieldCopy();
					}
				}
				else
				{
					return new ArkCoreXmlError("the \"basetype\" attribute of the node for {0} does not reference an existing metaobject", basetypeAttr.Value!);
				}
			}

			ArkCoreXmlError? error = null;

			for (XmlNode? childNode = node.FirstChild; childNode != null; childNode = childNode.NextSibling)
			{
				if (childNode.Name == "metafields")
				{
					for (XmlNode? fieldNode = childNode.FirstChild; fieldNode != null; fieldNode = fieldNode.NextSibling)
					{
						error = ParseMetaFieldNode(fieldNode, out igMetaField? field, metaobject);
						if (error != null) return error;

						metaobject._metaFields.Add(field!);
					}
				}
				else if (childNode.Name == "overriddenmetafields")
				{
					for (XmlNode? fieldNode = childNode.FirstChild; fieldNode != null; fieldNode = fieldNode.NextSibling)
					{
						error = ParseMetaFieldNode(fieldNode, out igMetaField? field, metaobject);
						if (error != null) return error;

						int fieldIndex = metaobject.GetFieldIndexByName(field!._fieldName!);

						if (fieldIndex < 0)
						{
							return new ArkCoreXmlError("Attempted to replace field with name \"{0}\" on metaobject \"{1}\", no such field exists on this object", field!._fieldName, metaobject._name);
						}

						metaobject.ValidateAndSetField(fieldIndex, field!);
					}
				}
				else if (childNode.Name == "objectlist")
				{
					XmlNode? elementtypeAttr = childNode.Attributes!.GetNamedItem("elementtype");
					if (elementtypeAttr == null)
					{
						return new ArkCoreXmlError("\"objectlist\" node must have an \"elementtype\" attribute!");
					}
					igMemoryRefMetaField _data = (igMemoryRefMetaField)metaobject._metaFields[2].CreateFieldCopy();
					((igObjectRefMetaField)_data._memType)._metaObject = _metaobjectLookup[elementtypeAttr.Value!];
					metaobject.ValidateAndSetField(2, _data);
				}
				else if (childNode.Name == "hashtable")
				{
					XmlNode? invalidvalueAttr = childNode.Attributes!.GetNamedItem("invalidvalue");
					if (invalidvalueAttr == null)
					{
						return new ArkCoreXmlError("\"hashtable\" node must have an \"invalidkey\" attribute!");
					}
					igMemoryRefMetaField _values = (igMemoryRefMetaField)metaobject._metaFields[0].CreateFieldCopy();
					_values._memType.SetMemoryFromString(ref _values._memType._default, invalidvalueAttr.Value!);
					metaobject.ValidateAndSetField(0, _values);


					XmlNode? invalidkeyAttr = childNode.Attributes!.GetNamedItem("invalidkey");
					if (invalidkeyAttr == null)
					{
						return new ArkCoreXmlError("\"hashtable\" node must have an \"invalidkey\" attribute!");
					}
					igMemoryRefMetaField _keys = (igMemoryRefMetaField)metaobject._metaFields[1].CreateFieldCopy();
					_keys._memType.SetMemoryFromString(ref _keys._memType._default, invalidkeyAttr.Value!);
					metaobject.ValidateAndSetField(1, _keys);
				}
				else if (childNode.Name == "compoundfields")
				{
					error = ParseCompoundNodePass2(childNode, _compoundLookup[metaobject._name!]);
					if (error != null) return null;
				}
				else if (childNode.Name == "dotnetfields"
				      && metaobject is igDotNetMetaObject dnmo // This only applies to igDotNetMetaObject
				      && childNode.FirstChild != null)         // If there are no children then do nothing
				{
					igStringRefList cppFields = new igStringRefList();
					igStringRefList dnFields = new igStringRefList();

					for (XmlNode? dnFieldNode = childNode.FirstChild; dnFieldNode != null; dnFieldNode = dnFieldNode.NextSibling)
					{
						if (dnFieldNode.Name != "field")
						{
							return new ArkCoreXmlError("\"dotnetfields\" node must have only \"field\" children");
						}

						XmlNode? cppNameAttr = dnFieldNode.Attributes!.GetNamedItem("cppName");
						XmlNode? dnNameAttr  = dnFieldNode.Attributes!.GetNamedItem("dnName");

						if (cppNameAttr == null || dnNameAttr == null)
						{
							return new ArkCoreXmlError("\"dotnetfields\".\"field\" node must have \"cppName\" and \"dnName\" string attributes");
						}

						cppFields.Append(cppNameAttr.Value!);
						dnFields.Append(dnNameAttr.Value!);
					}

					dnmo._cppFieldNames = cppFields;
					dnmo._dotNetFieldNames = dnFields;
					dnmo._exposedFieldCount = cppFields._count;
				}
			}

			return error;
		}


		/// <summary>
		/// Second pass of compound nodes, creates the fields and appends them
		/// </summary>
		/// <param name="node">The xml node for the compound field</param>
		/// <param name="compoundInfo">the compound info to read into</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseCompoundNodePass2(XmlNode node, igCompoundMetaFieldInfo compoundInfo)
		{
			if (node.Name != "compoundfields")
			{
				return new ArkCoreXmlError("All compound field elements must be named \"compoundfields\".");
			}

			for (XmlNode? fieldNode = node.FirstChild; fieldNode != null; fieldNode = fieldNode.NextSibling)
			{
				ArkCoreXmlError? error = ParseMetaFieldNode(fieldNode, out igMetaField? field, compoundInfo);
				if (error != null) return error;

				compoundInfo._fieldList.Add(field!);
			}

			return null;
		}


		/// <summary>
		/// Parses a "metafield" node
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="metafield">an out parameter for the igMetaField</param>
		/// <param name="parentMeta">The metaobject that the metafield belongs to</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldNode(XmlNode node, out igMetaField? metafield, igBaseMeta? parentMeta = null)
		{
			metafield = null;

			if (node.Name == "null")
			{
				// Case to deal with null metafields, only used for template parameters of
				// generic igOrderedMapMetaField
				return null;
			}

			if (node.Name != "metafield")
			{
				return new ArkCoreXmlError("All metafield nodes must be named \"metafield\"");
			}

			XmlNode? typeAttr = node.Attributes!.GetNamedItem("type");
			if (typeAttr == null)
			{
				return new ArkCoreXmlError("All metafield nodes must have a \"type\" attribute");
			}
			Type? metafieldType = igArkCore.GetObjectDotNetType(typeAttr.Value!);
			igCompoundMetaFieldInfo? compoundInfo = null;
			if (metafieldType == null)
			{
				metafieldType = typeof(igPlaceHolderMetaField);
				if (_compoundLookup.TryGetValue(typeAttr.Value!, out compoundInfo))
				{
					metafieldType = typeof(igCompoundMetaField);
				}
				else if(typeAttr.Value!.EndsWith("ArrayMetaField")
				     && _compoundLookup.TryGetValue(typeAttr.Value!.Replace("ArrayMetaField", "MetaField"), out compoundInfo))
				{
					metafieldType = typeof(igCompoundArrayMetaField);
				}
			}
			if (!metafieldType.IsAssignableTo(typeof(igMetaField)))
			{
				return new ArkCoreXmlError("type \"{0}\" is not assignable to igMetaField, ensure the metafield type is a class assignable to igMetaField", typeAttr.Value);
			}
			if (metafieldType.IsAssignableTo(typeof(igPlaceHolderMetaField)))
			{
				Logging.Warn("Failed to load metafield type information for {0}, resorting to placeholder type", typeAttr.Value!);
			}
			if (metafieldType.IsAbstract)
			{
				return new ArkCoreXmlError("type \"{0}\" is abstract and cannot be instantiated, ensure the metafield type is instantiable", typeAttr.Value);
			}

			metafield = (igMetaField)Activator.CreateInstance(metafieldType)!;
			metafield._parentMeta = parentMeta;
			if(metafield is igPlaceHolderMetaField placeHolder)
			{
				placeHolder._platformInfo = _metafieldLookup[typeAttr.Value!];
			}
			else if(metafield is igCompoundMetaField compoundField)
			{
				if (compoundInfo == null)
				{
					// The only thing that should make it here is igOrderedMapMetaField
					_compoundLookup.TryGetValue(typeAttr.Value!, out compoundInfo);
				}
				compoundField._compoundFieldInfo = compoundInfo!;
			}
			if (metafield.IsArray)
			{
				FieldInfo numField = metafieldType.GetField("_num")!;
				ParseMetaFieldPropertyUShort(node, "num", out ushort num, 1);
				numField.SetValue(metafield, unchecked((short)num));
			}

			ParseMetaFieldPropertyString(node, "name", out metafield._fieldName, null);

			ArkCoreXmlError? error = null;
			error = ParseMetaFieldPropertyUShort(node, "offset",           out metafield._offset, 0);
			if (error != null) return error;
			error = ParseMetaFieldPropertyEnum(node, "copyMethod",         out metafield._properties._copyMethod);
			if (error != null) return error;
			error = ParseMetaFieldPropertyEnum(node, "resetMethod",        out metafield._properties._resetMethod);
			if (error != null) return error;
			error = ParseMetaFieldPropertyEnum(node, "isAlikeMethod",      out metafield._properties._isAlikeCompareMethod);
			if (error != null) return error;
			error = ParseMetaFieldPropertyEnum(node, "itemsCopyMethod",    out metafield._properties._itemsCopyMethod);
			if (error != null) return error;
			error = ParseMetaFieldPropertyEnum(node, "keysCopyMethod",     out metafield._properties._keysCopyMethod);
			if (error != null) return error;
			error = ParseMetaFieldPropertyBool(node, "persistent",         out metafield._properties._persistent,        true);
			if (error != null) return error;
			error = ParseMetaFieldPropertyBool(node, "hasInvariance",      out metafield._properties._hasInvariance,     false);
			if (error != null) return error;
			error = ParseMetaFieldPropertyBool(node, "hasPoolName",        out metafield._properties._hasPoolName,       false);
			if (error != null) return error;
			error = ParseMetaFieldPropertyBool(node, "mutable",            out metafield._properties._mutable,           false);
			if (error != null) return error;
			error = ParseMetaFieldPropertyBool(node, "implicitAlignment",  out metafield._properties._implicitAlignment, true);
			if (error != null) return error;
			error = ParseMetaFieldPropertyByte(node, "requiredAlignment",  out metafield._properties._requiredAlignment, 0x00);
			if (error != null) return error;

			if (metafield is igRefMetaField refMetaField)
			{
				error = ParseMetaFieldPropertyBool(node, "construct",   out refMetaField._construct,   true);
				if (error != null) return error;
				error = ParseMetaFieldPropertyBool(node, "destruct",    out refMetaField._destruct,    false);
				if (error != null) return error;
				error = ParseMetaFieldPropertyBool(node, "reconstruct", out refMetaField._reconstruct, false);
				if (error != null) return error;
				error = ParseMetaFieldPropertyBool(node, "refCounted",  out refMetaField._refCounted,  true);
				if (error != null) return error;
			}

			if (metafield is igObjectRefMetaField || metafield is igHandleMetaField)
			{
				XmlNode? metaobjectNode = node.Attributes.GetNamedItem("metaobject");
				if (metaobjectNode == null)
				{
					return new ArkCoreXmlError("{0} metafield node missing \"metaobject\" attribute", metafield.GetType().Name);
				}
				igMetaObject? referencedMetaObject = null;
				if (!_metaobjectLookup.TryGetValue(metaobjectNode.Value!, out referencedMetaObject))
				{
					if (metaobjectNode.Value! == "(null)")
					{
						referencedMetaObject = _metaobjectLookup["igObject"];
					}
					else
					{
						return new ArkCoreXmlError("{0} metafield node is referencing a nonexistent metaobject {1}", metafield.GetType().Name, metaobjectNode.Value!);
					}
				}
				     if (metafield is igHandleMetaField    handleMetaField)    handleMetaField._metaObject    = referencedMetaObject;
				else if (metafield is igObjectRefMetaField objectRefMetaField) objectRefMetaField._metaObject = referencedMetaObject;
			}

			if (metafield is igMemoryRefMetaField || metafield is igMemoryRefHandleMetaField)
			{
				// needs to have a value assigned
				ref igMetaField? memType = ref metafield;

				     if (metafield is igMemoryRefHandleMetaField memoryRefMetaField)       memType = ref memoryRefMetaField._memType!;
				else if (metafield is igMemoryRefMetaField       memoryRefHandleMetaField) memType = ref memoryRefHandleMetaField._memType!;

				ParseMetaFieldPropertyMetaField(node, "memType", out memType);
			}

			if (metafield is igBitFieldMetaField bitFieldMetaField)
			{
				error = ParseMetaFieldPropertyUInt(node, "shift", out bitFieldMetaField._shift, 0);
				if (error != null) return error;
				error = ParseMetaFieldPropertyUInt(node, "bits", out bitFieldMetaField._bits, 0);
				if (error != null) return error;
				error = ParseMetaFieldPropertyString(node, "storageField", out string? storageFieldName, null);
				if (error != null) return error;
				bitFieldMetaField._storageMetaField = metafield._parentMeta.GetFieldByName(storageFieldName!);
				if (bitFieldMetaField._storageMetaField == null)
				{
					return new ArkCoreXmlError("Failed to find metafield {0} in metaobject {1}, referenced by {2}", storageFieldName, metafield._parentMeta._name, metafield._fieldName);
				}
				error = ParseMetaFieldPropertyMetaField(node, "assignmentField", out bitFieldMetaField._assignmentMetaField!);
				if (error != null) return error;

				// bit (hehe) of a hack to get bitfields to be written out
				bitFieldMetaField._properties._persistent = true;
			}

			if (metafield is igEnumMetaField enumMetaField)
			{
				error = ParseMetaFieldPropertyString(node, "metaenum", out string? metaenumName, "(null)");
				if (error != null) return error;

				if (metaenumName != "(null)"
				 && !_metaenumLookup.TryGetValue(metaenumName!, out enumMetaField._metaEnum!))
				{
					return new ArkCoreXmlError("Failed to find metaenum {0} in metaobject {1}, referenced by {2}", metaenumName, metafield._parentMeta._name, metafield._fieldName);
				}
			}

			if (metafield is igStaticMetaField staticMetaField)
			{
				error = ParseMetaFieldPropertyMetaField(node, "storageMetaField", out staticMetaField._storageMetaField!);
				if (error != null) return error;
			}

			if (metafield is igPropertyFieldMetaField propertyFieldMetaField)
			{
				error = ParseMetaFieldPropertyMetaField(node, "innerMetaField", out propertyFieldMetaField._innerMetaField!);
				if (error != null) return error;
			}

			if (metafield is igStructMetaField structMetaField)
			{
				error = ParseMetaFieldPropertyUShort(node, "typeSize", out ushort typeSize, 0);
				if (error != null) return error;

				if (typeSize == 0)
				{
					return new ArkCoreXmlError("Error parsing \"typeSize\" property on struct, expected non-zero size");
				}

				structMetaField._sizes.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, typeSize);

				error = ParseMetaFieldPropertyUShort(node, "align", out ushort align, 0);
				if (error != null) return error;

				if (align != 0 && (align > 0x8000 || (align & (align - 1)) != 0))
				{
					return new ArkCoreXmlError("Error parsing \"align\" property on struct, expected 0 < align <= 0x8000, where align is a power of 2");
				}

				structMetaField._alignments.Add(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT, align);
			}

			XmlNode? templateArgsNode = null;
			for (XmlNode? candidateNode = node.FirstChild; candidateNode != null; candidateNode = candidateNode.NextSibling)
			{
				if (candidateNode.Name == "templateargs")
				{
					templateArgsNode = candidateNode;
					continue;
				}
			}

			if (templateArgsNode != null)
			{
				uint t = 0;
				for (XmlNode? templateArgNode = templateArgsNode.FirstChild; templateArgNode != null; templateArgNode = templateArgNode.NextSibling, t++)
				{
					error = ParseMetaFieldNode(templateArgNode, out igMetaField? templateArg);
					if (error != null) return error;

					metafield.SetTemplateParameter(t, templateArg!);
				}
			}

			ParseMetaFieldPropertyString(node, "default", out string? defaultValue, "(null)");
			if (defaultValue != "(null)")
			{
				metafield.SetMemoryFromString(ref metafield._default, defaultValue!);
			}

			return null;
		}


		/// <summary>
		/// Parses an attribute of an xml node as a metafield, formatted as fX, where f is just the letter f,
		/// and X is a signed integer corresponding to a child node.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="metaField">The output metafield, null on failure</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyMetaField(XmlNode node, string name, out igMetaField? metaField)
		{
			metaField = null;

			XmlNode? metaFieldAttribute = node.Attributes!.GetNamedItem(name);
			if (metaFieldAttribute != null)
			{
				if (metaFieldAttribute.Value![0] != 'f')
				{
					return new ArkCoreXmlError("Invalid metafield reference attribute, must start with the letter 'f'");
				}
				if (!uint.TryParse(metaFieldAttribute.Value.Substring(1), out uint childIndex))
				{
					return new ArkCoreXmlError("Invalid metafield reference attribute, must be formatted as \"f%u\"");
				}
				XmlNode? fieldRefNode = node.ChildNodes[(int)childIndex];
				if (fieldRefNode == null)
				{
					return new ArkCoreXmlError("Invalid metafield reference attribute, the metafield index must be less than the number of children and above zero");
				}

				return ParseMetaFieldNode(fieldRefNode, out metaField);
			}

			return null;
		}


		/// <summary>
		/// Parses an attribute of an xml node as a string.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The string this is stored into</param>
		/// <param name="defaultValue">The default value for if reading this errors or the attribute is missing</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyString(XmlNode node, string name, out string? output, string? defaultValue)
		{
			output = defaultValue;

			XmlNode? attribute = node.Attributes!.GetNamedItem(name);
			if (attribute != null)
			{
				output = attribute.Value!;
			}

			if (attribute == null && defaultValue == null)
			{
				return new ArkCoreXmlError("\"{0}\" node is missing attribute \"{1}\"", node.Name, name);
			}

			return null;
		}


		/// <summary>
		/// Parses an attribute of an xml node as an unsigned byte.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The byte this is stored into</param>
		/// <param name="defaultValue">The default value for if reading this errors or the attribute is missing</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyByte(XmlNode node, string name, out byte output, byte defaultValue)
		{
			uint temp;
			ArkCoreXmlError? error = ParseMetaFieldPropertyUInt(node, name, out temp, defaultValue);
			output = unchecked((byte)temp);
			return error;
		}


		/// <summary>
		/// Parses an attribute of an xml node as an unsigned short.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The ushort this is stored into</param>
		/// <param name="defaultValue">The default value for if reading this errors or the attribute is missing</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyUShort(XmlNode node, string name, out ushort output, ushort defaultValue)
		{
			uint temp;
			ArkCoreXmlError? error = ParseMetaFieldPropertyUInt(node, name, out temp, defaultValue);
			output = unchecked((ushort)temp);
			return error;
		}


		/// <summary>
		/// Parses an attribute of an xml node as an unsigned int.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The uint this is stored into</param>
		/// <param name="defaultValue">The default value for if reading this errors or the attribute is missing</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyUInt(XmlNode node, string name, out uint output, uint defaultValue)
		{
			output = defaultValue;

			XmlNode? attribute = node.Attributes!.GetNamedItem(name);
			if (attribute != null)
			{
				string value = attribute.Value!;
				System.Globalization.NumberStyles numberStyles = System.Globalization.NumberStyles.None;
				if (value.Length > 2 && value[0] == '0' && value[1] == 'x')
				{
					numberStyles |= System.Globalization.NumberStyles.HexNumber;
					value = value.Substring(2);
				}

				if (!uint.TryParse(value, numberStyles, null, out output))
				{
					return new ArkCoreXmlError("Failed to parse attribute of name {0}", name);
				}
			}

			return null;
		}


		/// <summary>
		/// Parses an attribute of an xml node as a "true"/"false" boolean.
		/// </summary>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The bool this is stored into</param>
		/// <param name="defaultValue">The default value for if reading this errors or the attribute is missing</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyBool(XmlNode node, string name, out bool output, bool defaultValue)
		{
			output = defaultValue;

			XmlNode? methodNode = node.Attributes!.GetNamedItem(name);
			if (methodNode != null)
			{
				output = methodNode.Value == "true";
			}

			return null;
		}


		/// <summary>
		/// Parses an attribute of an xml node as an enum, parsing the name of the enum value.
		/// </summary>
		/// <typeparam name="T">The enum type in question</typeparam>
		/// <param name="node">The xml node in question</param>
		/// <param name="name">The name of the xml attribute</param>
		/// <param name="output">The enum this is stored into</param>
		/// <returns>null on success, and an ArkCoreXmlError containing a message on error</returns>
		private ArkCoreXmlError? ParseMetaFieldPropertyEnum<T>(XmlNode node, string name, out T output) where T : Enum
		{
			output = (T)(object)3;

			XmlNode? methodNode = node.Attributes!.GetNamedItem(name);
			if (methodNode != null)
			{
				if (kFieldMethodLookup.TryGetValue(methodNode.Value!, out int methodValue))
				{
					output = (T)(object)methodValue;
				}
				else
				{
					return new ArkCoreXmlError("Invalid field method type, must be one of the following: \"value\", \"reference\", \"none\", \"default\"");
				}
			}

			return null;
		}
	}
}