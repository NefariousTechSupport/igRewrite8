/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/



namespace igLibrary.Tfb.Core
{
	public class tfbEulerTransformMetaField : igMetaField
	{
		public igVec3f _position;
		public igVec3f _angle;

		public override uint GetAlignment(IG_CORE_PLATFORM platform)
		{
			return 4;
		}

		public override Type GetOutputType()
		{
			return typeof(tfbEulerTransform);
		}

		public override uint GetSize(IG_CORE_PLATFORM platform)
		{
			return 0x18;
		}

		public override object? ReadIGZField(igIGZLoader loader)
		{
			tfbEulerTransform eulerTransform = new tfbEulerTransform();
			eulerTransform._position._x = loader._stream.ReadSingle();
			eulerTransform._position._y = loader._stream.ReadSingle();
			eulerTransform._position._z = loader._stream.ReadSingle();
			eulerTransform._angle._x    = loader._stream.ReadSingle();
			eulerTransform._angle._y    = loader._stream.ReadSingle();
			eulerTransform._angle._z    = loader._stream.ReadSingle();
			return eulerTransform;
		}

		public override void WriteIGZField(igIGZSaver saver, igIGZSaver.SaverSection section, object? value)
		{
			tfbEulerTransform eulerTransform = (tfbEulerTransform)value!;
			section._sh.WriteSingle(eulerTransform._position._x);
			section._sh.WriteSingle(eulerTransform._position._y);
			section._sh.WriteSingle(eulerTransform._position._z);
			section._sh.WriteSingle(eulerTransform._angle._x);
			section._sh.WriteSingle(eulerTransform._angle._y);
			section._sh.WriteSingle(eulerTransform._angle._z);
		}
	}
}