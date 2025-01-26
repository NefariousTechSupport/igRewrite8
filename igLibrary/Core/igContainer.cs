/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//Credit to LG-RZ
	public class igContainer : igObject
	{
		public static int ComputeCapacityForCount(int count, int capacity)
		{
			if (capacity < 1)
				capacity = 1;
			if (count <= capacity)
				return capacity;
			if (count > 0x80)
				return (int)(count + 0x7F & 0xFFFFFF80);
			if (count < 8)
				return count;
			int newCapacity = count - 1 | count - 1 >> 1;
			newCapacity = newCapacity | newCapacity >> 2;
			newCapacity = newCapacity | newCapacity >> 4;
			newCapacity = newCapacity | newCapacity >> 8;
			return (newCapacity | newCapacity >> 0x10) + 1;
		}
		
	}
}