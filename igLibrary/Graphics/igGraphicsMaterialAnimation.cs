/*
	Copyright (c) 2022-2026, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Sg;

namespace igLibrary.Graphics
{
	public class igGraphicsMaterialAnimation : igObject
	{
		public igAnimatedTransformSource _transform;
		public igGraphicsMaterialAnimationConstantType _constantType;
		public string _constantName;
		public ulong _resource;
	}

	public class igGraphicsMaterialAnimationList : igTObjectList<igGraphicsMaterialAnimation>
	{
	}
}