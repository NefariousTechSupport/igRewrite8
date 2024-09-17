using System.ComponentModel.Design;
using igLibrary.Core;

namespace igCauldron3.Utils
{
	//buggy and incomplete, keeping it around cos it might be useful later
	// - my parents
	public static class FieldTraversal
	{
		private delegate void TraverseFieldAction(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names);
		private static Dictionary<Type, TraverseFieldAction> _lookup = PopulateLookup();
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
		private static void TraverseObjectRef(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			TraverseObject((igObject?)obj, name, traversed, names);
		}
		private static void TraverseObjectRefArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseObject((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", traversed, names);
			}
		}
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
		private static void TraverseMemoryRefArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseMemoryRef((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", field, traversed, names);
			}
		}
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
		private static void TraverseVectorArr(object? obj, string name, igMetaField field, List<igObject> traversed, List<string> names)
		{
			Array arr = (Array)obj!;
			for(int i = 0; i < arr.Length; i++)
			{
				TraverseVector((igObject?)arr.GetValue(i), name + "[" + i.ToString() + "]", field, traversed, names);
			}
		}
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