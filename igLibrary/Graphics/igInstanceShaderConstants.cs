namespace igLibrary.Graphics
{
	/// <summary>
	/// represents the values in igModelDrawCallData::_instanceShaderConstants
	/// </summary>
	[Flags]
	public enum igInstanceShaderConstants
	{
		kHasVertexColor = 1,
		kHasBlendIndices = 2,
		kHasMorphTargets = 4,
		kPS3EdgeSkinned = 8
	};
}
