/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Vfx
{
	public unsafe struct igVfxRangedCurve
	{
		public readonly igVfxCurveKeyframe[] _keyframes;
		public igVfxModulationHelper _modulationHelper;
		public ushort _field_0x50;
		public ushort _field_0x52;

		public igVfxRangedCurve()
		{
			_keyframes = new igVfxCurveKeyframe[5];
			_modulationHelper = default;
			_field_0x50 = default;
			_field_0x52 = default;
		}
	}
}