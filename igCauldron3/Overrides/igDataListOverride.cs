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
	/// UI override for rendering data lists
	/// </summary>
	public class igDataListOverride : InspectorDrawOverride
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public igDataListOverride()
		{
			_t = typeof(IigDataList);
		}


		/// <summary>
		/// Renders the ui
		/// </summary>
		/// <param name="dirFrame">The directory manager frame</param>
		/// <param name="id">the id to render with</param>
		/// <param name="obj">the object</param>
		/// <param name="meta">the type of the object</param>
		public override void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta)
		{
			IigDataList dataList = (IigDataList)obj;
			IigMemory memValue = dataList.GetData();
			object? castedObject = memValue;
			FieldRenderer.RenderField(id, "Data", castedObject, meta._metaFields[2], (value) => {
				dataList.SetData(memValue);
				dataList.SetCount(memValue.GetData().Length);
				dataList.SetCapacity(memValue.GetData().Length);
			});
		}
	}
}