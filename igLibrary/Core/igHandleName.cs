/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	[igStruct]
	public struct igHandleName
	{
		public igName _name;
		public igName _ns;


		/// <summary>
		/// Default constructor
		/// </summary>
		public igHandleName()
		{
			_name = default;
			_ns = default;
		}


		/// <summary>
		/// Assigns a handle name from a string matching the syntax "namespace.name"
		/// </summary>
		/// <param name="name">string representing the handle</param>
		public igHandleName(string name)
		{
			int separatorIndex = name.IndexOf('.');
			_name = new igName(name.Substring(0, separatorIndex));
			_ns = new igName(name.Substring(separatorIndex+1, name.Length-separatorIndex-1));
		}
	}
}