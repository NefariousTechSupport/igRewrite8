/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	//This is responsible for excluding fields from a platform.
	// For example, the _vehiclePersonalizationSets field in CVehicleSystemData
	public class igPlatformExclusionAttribute : igObject
	{
		public IG_CORE_PLATFORM _value;
		public igPlatformExclusionAttribute(){}
		public igPlatformExclusionAttribute(IG_CORE_PLATFORM platform)
		{
			_value = platform;
		}
	}
}