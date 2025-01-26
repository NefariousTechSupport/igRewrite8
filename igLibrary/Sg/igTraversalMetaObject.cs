/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Sg
{
	public class igTraversalMetaObject : igMetaObject
	{
		[Obsolete("This exists for the reflection system, do not use.")] public object? _nodeProperties;             //igVector<igTraversalNodeProperties>
		[Obsolete("This exists for the reflection system, do not use.")] public object? _propagatedNodeProperties;   //igVector<igTraversalNodeProperties>
		[Obsolete("This exists for the reflection system, do not use.")] public bool _root;
	}
}