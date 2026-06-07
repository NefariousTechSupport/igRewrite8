/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Entity
{
	public class igEntityData : igObject
	{
		public igComponentDataTable _componentData;
		public float _scale;



		/// <summary>
		/// Gets a component of a specified type
		/// </summary>
		/// <typeparam name="T">The type of component</typeparam>
		/// <param name="index">The index of the component of that type</param>
		/// <returns>The component, or null if it doesn't exist</returns>
		public T? GetComponentData<T>(int index = 0) where T : igComponentData
		{
			int counter = 0;
			foreach (KeyValuePair<string, igObject> kvp in _componentData)
			{
				if (kvp.Value.GetType().IsAssignableTo(typeof(T)))
				{
					if (counter == index)
					{
						return (T)kvp.Value;
					}
					counter++;
				}
			}

			return null;
		}


		/// <summary>
		/// Gets a component of a specified type
		/// </summary>
		/// <param name="metaobject">The type of component</typeparam>
		/// <param name="index">The index of the component of that type</param>
		/// <returns>The component, or null if it doesn't exist</returns>
		public igComponentData? GetComponentData(igMetaObject metaobject, int index = 0)
		{
			int counter = 0;
			foreach (KeyValuePair<string, igObject> kvp in _componentData)
			{
				if (kvp.Value.GetMeta().CanBeAssignedTo(metaobject))
				{
					if (counter == index)
					{
						return (igComponentData)kvp.Value;
					}
					counter++;
				}
			}

			return null;
		}
	}
}