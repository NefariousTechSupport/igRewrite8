/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Core;

namespace igCauldron3
{
	/// <summary>
	/// Abstract class for overriding the ui display for certain <c>igMetaObject</c>s
	/// </summary>
	public abstract class InspectorDrawOverride
	{
		// The type to override
		public Type _t { get; protected set; }


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public abstract void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta);
	}
}