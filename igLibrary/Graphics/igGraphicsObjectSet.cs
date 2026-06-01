/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Graphics
{
	public class igGraphicsObjectSet : igObject
	{
		// this global list is a hack to avoid duplicates within a single igz
		public static igVector<igGraphicsObject> _globalObjects = new igVector<igGraphicsObject>();
		public igVector<igGraphicsObject> _objects;


		/// <summary>
		/// Gets or adds a graphics object to the vector, similar to emplace in a c++ set
		/// </summary>
		/// <typeparam name="T">The type of object</typeparam>
		/// <param name="candidate">The object to add or get</param>
		/// <returns></returns>
		public T GetOrAddGraphicsObject<T>(T candidate) where T : igGraphicsObject
		{
			return Internal<T>(_objects, Internal<T>(_globalObjects, candidate));
		}

		/// <summary>
		/// Gets or adds a graphics object to the vector, similar to emplace in a c++ set
		/// </summary>
		/// <typeparam name="T">The type of object</typeparam>
		/// <param name="candidate">The object to add or get</param>
		/// <returns></returns>
		private static T Internal<T>(igVector<igGraphicsObject> objects, T candidate) where T : igGraphicsObject
		{
			for (int i = 0; i < objects._count; i++)
			{
				if (candidate.ShallowEquals(objects[i]))
				{
					return (T)objects[i];
				}
			}

			objects.Append(candidate);

			return candidate;
		}


		/// <summary>
		/// Gets the index of a given resource
		/// </summary>
		/// <param name="graphicsObject"></param>
		/// <returns></returns>
		public int FindResourceIndex(igGraphicsObject? graphicsObject)
		{
			if (graphicsObject == null)
			{
				return -1;
			}

			return _objects.IndexOf(graphicsObject);
		}
	}
}