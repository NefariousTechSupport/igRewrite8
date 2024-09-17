using igLibrary.Core;
using igLibrary.DotNet;
using Mono.Cecil;

namespace VvlToDll
{
	public class ArkDllExport
	{
		public static Dictionary<igBaseMeta, TypeDefinition> _metaTypeLookup;
		public static Dictionary<igBaseMeta, TypeDefinition> _dynamicMetaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
		public static ModuleDefinition module;

		public static void Create(string outputDir)
		{
			if(module != null) return;
			Directory.CreateDirectory(outputDir);
			module = ModuleDefinition.CreateModule( "Ark.dll", ModuleKind.Dll);
			_metaTypeLookup = new Dictionary<igBaseMeta, TypeDefinition>();
			InstantiateTypes();
			DefineEnums();
			DefineObjects();
			module.Write(Path.Combine(outputDir, "Ark.dll"));
		}
		private static void InstantiateTypes()
		{
			foreach(igMetaEnum metaEnum in igArkCore.MetaEnums)
			{
				TypeDefinition td = new TypeDefinition("Enums", metaEnum._name, TypeAttributes.Public | TypeAttributes.Sealed);
				_metaTypeLookup.Add(metaEnum, td);
				module.Types.Add(td);
			}
			foreach(igMetaObject metaObject in igArkCore.MetaObjects)
			{
				TypeDefinition td = new TypeDefinition("Classes", metaObject._name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoLayout | TypeAttributes.BeforeFieldInit | TypeAttributes.AnsiClass);
				_metaTypeLookup.Add(metaObject, td);
				module.Types.Add(td);
			}
		}
		private static void DefineEnums()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaEnum metaEnum) break;

				TypeDefinition td = kvp.Value;
				td.BaseType = module.ImportReference(typeof(Enum));
				td.Fields.Add(new FieldDefinition("__value", FieldAttributes.Public | FieldAttributes.SpecialName, module.ImportReference(typeof(int))));
				for(int i = 0; i < metaEnum._names.Count; i++)
				{
					FieldDefinition fd = new FieldDefinition(metaEnum._names[i], FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal, td);
					fd.Constant = metaEnum._values[i];
					td.Fields.Add(fd);
				}
			}
		}
		private static void DefineObjects()
		{
			foreach(KeyValuePair<igBaseMeta, TypeDefinition> kvp in _metaTypeLookup)
			{
				if(kvp.Key is not igMetaObject metaObject) continue;

				TypeDefinition td = kvp.Value;
				if(metaObject._parent != null)
				{
					td.BaseType = _metaTypeLookup[metaObject._parent];
				}
				else continue;

				for(int i = metaObject._parent._metaFields.Count; i < metaObject._metaFields.Count; i++)
				{
					AddField(module, td, metaObject, metaObject._metaFields[i], x => x == null ? module.ImportReference(typeof(int)) : _metaTypeLookup[x]);
				}
			}
		}
		public static void AddField(ModuleDefinition md, TypeDefinition target, igMetaObject meta, igMetaField metaField, Func<igBaseMeta, TypeReference> resolver)
		{
			FieldDefinition fd = new FieldDefinition(metaField._fieldName, FieldAttributes.Public, GetFieldTypeRef(md, metaField, resolver));
			if(metaField is igStaticMetaField)
			{
				fd.Attributes |= FieldAttributes.Static;
			}
			target.Fields.Add(fd);
		}
		public static TypeReference GetFieldTypeRef(ModuleDefinition md, igMetaField field, Func<igBaseMeta, TypeReference> resolver)
		{
			     if(field is igIntMetaField)                   return md.TypeSystem.Int32;
			else if(field is igUnsignedIntMetaField)           return md.TypeSystem.UInt32;
			else if(field is igShortMetaField)                 return md.TypeSystem.Int16;
			else if(field is igUnsignedShortMetaField)         return md.TypeSystem.UInt16;
			else if(field is igLongMetaField)                  return md.TypeSystem.Int64;
			else if(field is igUnsignedLongMetaField)          return md.TypeSystem.UInt64;
			else if(field is igCharMetaField)                  return md.TypeSystem.SByte;
			else if(field is igUnsignedCharMetaField)          return md.TypeSystem.Byte;
			else if(field is igFloatMetaField)                 return md.TypeSystem.Single;
			else if(field is igDoubleMetaField)                return md.TypeSystem.Double;
			else if(field is igIntPtrMetaField)                return md.TypeSystem.IntPtr;
			else if(field is igUnsignedIntPtrMetaField)        return md.TypeSystem.UIntPtr;
			else if(field is igBoolMetaField)                  return md.TypeSystem.Boolean;
			else if(field is igStringMetaField)                return md.TypeSystem.String;
			else if(field is igDotNetEnumMetaField dnemf)      return resolver.Invoke(dnemf._definedMetaEnum);
			else if(field is igEnumMetaField emf)              return emf._metaEnum == null ? md.TypeSystem.Int32 : resolver.Invoke(emf._metaEnum);
			else if(field is igMemoryRefMetaField mrmf)        return CreateMemoryRefType(md, GetFieldTypeRef(md, mrmf._memType, resolver));
			else if(field is igMemoryRefHandleMetaField mrhmf) return CreateMemoryRefType(md, GetFieldTypeRef(md, mrhmf._memType, resolver));
			else if(field is igObjectRefMetaField ormf)        return resolver.Invoke(ormf._metaObject);
			else if(field is igHandleMetaField hmf)            return resolver.Invoke(hmf._metaObject);
			else if(field is igStaticMetaField smf)            return GetFieldTypeRef(md, smf._storageMetaField, resolver);
			else                                               return md.TypeSystem.Object;
		}
		private static TypeReference CreateMemoryRefType(ModuleDefinition md, TypeReference typeRef)
		{
			GenericInstanceType memInst = new GenericInstanceType(md.ImportReference(typeof(igMemory<>)));
			memInst.GenericArguments.Add(typeRef);
			return memInst;
		}
		private static TypeReference CreateHandleType(ModuleDefinition md, TypeReference typeRef)
		{
			GenericInstanceType hndInst = new GenericInstanceType(md.ImportReference(typeof(igTHandle<>)));
			hndInst.GenericArguments.Add(typeRef);
			return hndInst;
		}
	}
}