/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


using igLibrary.Sg;

namespace igLibrary.Render
{
	public class igModelData : igNamedObject
	{
		public igVec4f _min;
		public igVec4f _max;
		public igVector<igAnimatedTransform> _transforms;
		public igVector<int> _transformHierarchy;
		public igVector<igModelDrawCallData> _drawCalls;
		public igVector<int> _drawCallTransformIndices;
		public igVector<igAnimatedMorphWeightsTransform> _morphWeightTransforms;
		public igVector<int> _blendMatrixIndices;
	}
}