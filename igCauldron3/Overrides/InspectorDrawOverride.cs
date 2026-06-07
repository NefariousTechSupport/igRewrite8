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


		/// <summary>
		/// Simpler way of rendering a field
		/// </summary>
		/// <param name="id">The id of the ui element</param>
		/// <param name="target">The target of the operation</param>
		/// <param name="field">The field to render of the target</param>
		protected void RenderField(string id, igObject target, igMetaField field)
		{
			FieldRenderer.RenderField(id, field._fieldName!, field._fieldHandle!.GetValue(target), field, (newValue) =>
			{
				field._fieldHandle!.SetValue(target, newValue);
			});
		}
	}
}