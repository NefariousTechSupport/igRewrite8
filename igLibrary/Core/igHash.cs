/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using HashDepot;
using System.Text;

namespace igLibrary.Core
{
	public static class igHash
	{
		public static uint Hash(byte[] input) => Fnv1a.Hash32(input);
		public static uint Hash(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input));
		}
		public static uint HashI(string input)
		{
			return Fnv1a.Hash32(Encoding.ASCII.GetBytes(input.ToLower()));
		}
	}
}