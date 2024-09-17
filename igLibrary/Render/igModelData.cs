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