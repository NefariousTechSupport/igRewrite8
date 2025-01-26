/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using System.Xml.Serialization;

namespace igLibrary
{
	public class CResourcePrecacher : igObject
	{
		public static EMemoryPoolID mDestMemoryPoolId;
		public virtual void Precache(string filePath)
		{

		}
		public virtual void Recache(string filePath)
		{

		}
		public virtual void Uncache(EMemoryPoolID poolId)
		{

		}
	}
	public class CResourcePrecacherList : igTObjectList<CResourcePrecacher>{}
}