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
	/// The base class for every igObject
	/// </summary>
	public class __internalObjectBase
	{
		public uint refCount;
		public igMemoryPool internalMemoryPool;
		internal bool dynamicMeta;
		internal igMetaObject? internalMeta;


		/// <summary>
		/// Grab the current igMetaObject
		/// </summary>
		/// <returns>the metaobject</returns>
		/// <exception cref="TypeLoadException">Thrown when it cannot figure out what the type is</exception>
		public virtual igMetaObject GetMeta()
		{
			if(internalMeta == null)
			{
				internalMeta = igArkCore.GetObjectMeta(GetType().Name);
				if(internalMeta == null) throw new TypeLoadException($"Failed to load meta for {GetType().Name}");
			}
			return internalMeta;
		}
	}
}