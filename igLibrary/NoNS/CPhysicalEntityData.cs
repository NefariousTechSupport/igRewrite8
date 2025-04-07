/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary
{
	public class CPhysicalEntityData : CGameEntityData
	{
		public uint _physicalEntityFlags = 128;
		public int _health = 100;
		public int _healthMax = 100;
		public EVulnerability _vulnerability = EVulnerability.eV_Invulnerable;

		public CPhysicalEntityData()
		{
			_entityFlags = 2879488;
			_gameEntityFlags = 512;
		}
	}
}