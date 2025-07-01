/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Vfx
{
	public struct igVfxRgbCurve
	{
		// Names are guesses
		// Padding is added just in case it's not actually padding
		public bool _enableInterpolation;
		public bool _enableRandomness;
		public igVfxModulationHelper _modulationHelper;
		public igVec4f _c00;
		public igVec4f _c01;
		public igVec4f _c02;
		public igVec4f _c03;
		public igVec4f _c04;
		public igVec4f _c05;
		public igVec4f _c06;
		public igVec4f _c07;
		public igVec4f _c08;
		public igVec4f _c09;
		public igVec4f _c10;
		public igVec4f _c11;
		public igVec4f _c12;
		public igVec4f _c13;
		public igVec4f _c14;
	}
}