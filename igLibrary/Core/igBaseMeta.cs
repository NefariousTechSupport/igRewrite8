namespace igLibrary.Core
{
	public class igBaseMeta : igObject
	{
		public string? _name;
		internal Type _internalType;
		protected bool _beganFinalization = false;
		protected bool _beganFinalizationPrep = false;
		protected bool _finishedFinalization = false;
		protected bool _finishedFinalizationPrep = false;
		protected bool _gatheredDependancies = false;
		protected bool _inArkCore = false;
		public BuildPriority _priority = BuildPriority.Normal;
		public enum BuildPriority
		{
			Low,
			Normal,
			High
		}
		public virtual void PostUndump(){}

		public virtual igMetaField? GetFieldByName(string name) => null;

		//The following will be used to generate only the types that need to be loaded
		protected void ReadyFieldDependancy2(igMetaField field)
		{
			if(field is igObjectRefMetaField objField) objField._metaObject.GatherDependancies();
			else if(field is igMemoryRefMetaField memField) ReadyFieldDependancy2(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyFieldDependancy2(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyFieldDependancy2(staticField._storageMetaField);
		}
		protected void ReadyCompoundFieldDependancy(igMetaField field)
		{
			if(field is igOrderedMapMetaField omField)
			{
				ReadyCompoundFieldDependancy(omField._t);
				ReadyFieldDependancy2(omField._t);
				ReadyCompoundFieldDependancy(omField._u);
				ReadyFieldDependancy2(omField._u);
			}
			else if(field is igCompoundMetaField compoundField) compoundField._compoundFieldInfo.GatherDependancies();
			else if(field is igMemoryRefMetaField memField) ReadyCompoundFieldDependancy(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyCompoundFieldDependancy(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyCompoundFieldDependancy(staticField._storageMetaField);
		}
		public virtual void GatherDependancies(){}
		public virtual void DefineType2(){}
		public virtual void CreateType2(){}
		//https://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059
		public static bool IsAssignableToGenericType(Type type, Type genericType)
		{
			return (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
				||  (type.BaseType != null && IsAssignableToGenericType(type.BaseType, genericType));
		}
 	}
	public class igBaseMetaList : igTObjectList<igBaseMeta>{}
}