/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public struct igTime
	{
		public float ElapsedSeconds
		{
			get => _elapsedDays / 8192;
			set => _elapsedDays = value / 8192;
		}
		public float ElapsedDays
		{
			get => _elapsedDays;
			set => _elapsedDays = value;
		}

		public float _elapsedDays;

		public igTime(float elapsedDays)
		{
			_elapsedDays = elapsedDays;
		}
	}
}