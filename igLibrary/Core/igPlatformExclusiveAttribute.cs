/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//This is responsible for marking fields as platform exclusive.
	// For example, the _virtualWalkAndTurnStickThreshold and _virtualRunStickThreshold fields in CPlayerSystemData
	public class igPlatformExclusiveAttribute : igObject
	{
		public IG_CORE_PLATFORM _value;
		public igPlatformExclusiveAttribute(){}
		public igPlatformExclusiveAttribute(IG_CORE_PLATFORM platform)
		{
			_value = platform;
		}
	}
}