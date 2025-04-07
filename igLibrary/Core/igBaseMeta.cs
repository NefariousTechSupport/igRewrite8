/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// Base reflection class for a meta item
	/// </summary>
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


		/// <summary>
		/// The priority level for building the class
		/// </summary>
		public enum BuildPriority
		{
			Low,
			Normal,
			High
		}


		/// <summary>
		/// Post read stuff
		/// </summary>
		public virtual void PostUndump(){}


		/// <summary>
		/// Get a field by its name
		/// </summary>
		/// <param name="name">The name</param>
		/// <returns>The field</returns>
		public virtual igMetaField? GetFieldByName(string name) => null;


		/// <summary>
		/// Generates only the types that need to be loaded
		/// </summary>
		/// <param name="field">The field to prepare</param>
		protected void ReadyFieldDependancy2(igMetaField field)
		{
			if(field is igObjectRefMetaField objField) objField._metaObject.GatherDependancies();
			else if(field is igMemoryRefMetaField memField) ReadyFieldDependancy2(memField._memType);
			else if(field is igMemoryRefHandleMetaField memHndField) ReadyFieldDependancy2(memHndField._memType);
			else if(field is igStaticMetaField staticField) ReadyFieldDependancy2(staticField._storageMetaField);
		}


		/// <summary>
		/// Goes through each compound field to determine the types to generate
		/// </summary>
		/// <param name="field"></param>
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


		/// <summary>
		/// Gather the dependencies for the object
		/// </summary>
		public virtual void GatherDependancies(){}


		/// <summary>
		/// Starts defining the types
		/// </summary>
		public virtual void DefineType2(){}


		/// <summary>
		/// Actually creates the type
		/// </summary>
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