/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public abstract class igReferenceResolver : igNamedObject
	{
		public virtual string MakeReference(igObject reference, igReferenceResolverContext ctx) => null;
		public virtual igObject? ResolveReference(string reference, igReferenceResolverContext ctx) => null;
	}
}