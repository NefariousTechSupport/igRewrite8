/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public enum ECharacterCollisionPriority
	{
		eCCP_NonGameplayCritical = 0,
		eCCP_None = 1,
		eCCP_Low = 2,
		eCCP_NormalSelfPushable = 3,
		eCCP_Normal = 4,
		eCCP_JumpingPlayer = 5,
		eCCP_High = 6,
		eCCP_Charger = 7,
		eCCP_PlayerTraversable = 8,
		eCCP_Immobile = 9,
	};
}