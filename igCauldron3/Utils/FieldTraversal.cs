/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.ComponentModel.Design;
using igLibrary.Core;

namespace igCauldron3.Utils
{
	// buggy and incomplete, keeping it around cos it might be useful later
	// - my parents
	public static class FieldTraversal
	{
		private delegate void TraverseFieldAction(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names);
		private static Dictionary<Type, TraverseFieldAction> _lookup = PopulateLookup();


		/// <summary>
		/// Initialises lookup table for traversing different field types
		/// </summary>
		/// <returns></returns>
		private static Dictionary<Type, TraverseFieldAction> PopulateLookup()
		{
			Dictionary<Type, TraverseFieldAction> lookup = new Dictionary<Type, TraverseFieldAction>();
			lookup.Add(typeof(igObjectRefMetaField), TraverseObjectRef);
			lookup.Add(typeof(igObjectRefArrayMetaField), TraverseObjectRefArr);
			lookup.Add(typeof(igMemoryRefMetaField), TraverseMemoryRef);
			lookup.Add(typeof(igMemoryRefHandleMetaField), TraverseMemoryRef);
			lookup.Add(typeof(igMemoryRefArrayMetaField), TraverseMemoryRefArr);
			lookup.Add(typeof(igMemoryRefHandleArrayMetaField), TraverseMemoryRefArr);
			lookup.Add(typeof(igVectorMetaField), TraverseVector);
			lookup.Add(typeof(igVectorArrayMetaField), TraverseVectorArr);
			return lookup;
		}


		/// <summary>
		/// Traverse a specific field of an object
		/// </summary>
		/// <param name="obj">The value of the field</param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The names of the traversed objects</param>
		private static void TraverseField(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			if(_lookup.TryGetValue(field.GetType(), out TraverseFieldAction? action))
			{
				action.Invoke(obj, name, field, traversed, names);
			}
			else if(field is igCompoundMetaField)
			{
				TraverseCompound(obj, name, field, traversed, names);
			}
		}


		/// <summary>
		/// Traverses the objects in a directory, filtering based on a directory type
		/// </summary>
		/// <param name="directory">The directory</param>
		/// <param name="filterType">The metaobject to filter by</param>
		/// <param name="curDirObjects">the output list of filtered objects</param>
		/// <param name="curDirNames">the output list of names of filtered objects</param>
		public static void TraverseObjectDir(igObjectDirectory directory, igMetaObject filterType, out List<igObject> curDirObjects, out List<string> curDirNames)
		{
			List<igObject> traversedObjs = new List<igObject>();
			List<string> traversedNames = new List<string>();
			for(int i = 0; i < directory._objectList._count; i++)
			{
				TraverseObject(directory._objectList[i], directory._nameList![i]._string, traversedObjs, traversedNames);
			}
			curDirObjects = new List<igObject>();
			curDirNames = new List<string>();
			for(int i = 0; i < traversedObjs.Count; i++)
			{
				if(traversedObjs[i].GetMeta().CanBeAssignedTo(filterType))
				{
					curDirObjects.Add(traversedObjs[i]);
					curDirNames.Add(traversedNames[i]);
				}
			}
		}


		/// <summary>
		/// Traverses the fields of a single object
		/// </summary>
		/// <param name="obj">The object to traverse</param>
		/// <param name="name">The name of the field</param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		public static void TraverseObject(igObject? obj, string name, List<igObject> traversed, List<string> names)
		{
			if(obj == null || obj is igMetaObject || obj is igMetaField || traversed.Contains(obj)) return;

			traversed.Add(obj);
			names.Add(name);

			igMetaObject meta = obj.GetMeta();
			for(int i = 0; i < meta._metaFields.Count; i++)
			{
				igMetaField field = meta._metaFields[i];
				if(field is igPropertyFieldMetaField) continue;
				if(field is igStaticMetaField)        continue;
				if(field is igBitFieldMetaField)      continue;
				TraverseField(field._fieldHandle!.GetValue(obj), name + "->" + field._fieldName, field, traversed, names);
			}
		}


		/// <summary>
		/// Traverse an object ref
		/// </summary>
		/// <param name="obj">Traverse an <c>igObjectRefMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseObjectRef(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			TraverseObject((igObject?)obj, name, traversed, names);
		}


		/// <summary>
		/// Traverse an object ref array
		/// </summary>
		/// <param name="obj">Traverse an <c>igObjectRefArrayMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseObjectRefArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseObject((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", traversed, names);
			}
		}


		/// <summary>
		/// Traverse a memory ref
		/// </summary>
		/// <param name="obj">Traverse an <c>igObjectRefArrayMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseMemoryRef(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			igMetaField memType = field is igMemoryRefMetaField ? ((igMemoryRefMetaField)field)._memType : ((igMemoryRefHandleMetaField)field)._memType;
			if(!_lookup.TryGetValue(memType.GetType(), out TraverseFieldAction? action)) return;

			IigMemory mem = (IigMemory)obj!;
			Array data = mem.GetData();
			if(data == null) return;
			for(int i = 0; i < data.Length; i++)
			{
				action.Invoke(data.GetValue(i), name + "[" + i.ToString() + "]", memType, traversed, names);
			}
		}


		/// <summary>
		/// Traverse a memory ref array
		/// </summary>
		/// <param name="obj">Traverse an <c>igMemoryRefArrayMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseMemoryRefArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseMemoryRef((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", field, traversed, names);
			}
		}


		/// <summary>
		/// Traverse a vector
		/// </summary>
		/// <param name="obj">Traverse an <c>igVectorMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseVector(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			igMetaField memType = field.GetTemplateParameter(0)!;
			if(!_lookup.TryGetValue(memType.GetType(), out TraverseFieldAction? action)) return;

			igVectorCommon vec = (igVectorCommon)obj!;
			IigMemory mem = vec.GetData();
			Array data = mem.GetData();
			if(data == null) return;
			for(int i = 0; i < data.Length; i++)
			{
				action.Invoke(data.GetValue(i), name + "[" + i.ToString() + "]", memType, traversed, names);
			}
		}


		/// <summary>
		/// Traverse a vector array
		/// </summary>
		/// <param name="obj">Traverse an <c>igVectorArrayMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseVectorArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseVector((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", field, traversed, names);
			}
		}


		/// <summary>
		/// Traverse a compound field
		/// </summary>
		/// <param name="obj">Traverse an <c>igCompoundMetaField</c></param>
		/// <param name="name">The name of the field</param>
		/// <param name="field">The <c>igMetaField</c></param>
		/// <param name="traversed">The list of traversed objects</param>
		/// <param name="names">The list of names of traversed objects</param>
		private static void TraverseCompound(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			List<igMetaField> fields = ((igCompoundMetaField)field).GetFieldList();
			for(int i = 0; i < fields.Count; i++)
			{
				TraverseField(fields[i]._fieldHandle!.GetValue(obj), name + "." + fields[i]._fieldName, fields[i], traversed, names);
			}
		}
	}
}