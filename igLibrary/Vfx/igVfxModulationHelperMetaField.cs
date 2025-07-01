namespace igLibrary.Vfx
{
	public class igVfxModulationHelperMetaField : igCompoundMetaField
	{
		public static igVfxModulationHelperMetaField _MetaField { get; private set; } = new igVfxModulationHelperMetaField();

		public igVfxModulationHelperMetaField()
		{
			// Can be null
			_compoundFieldInfo = igArkCore.GetCompoundFieldInfo(nameof(igVfxModulationHelperMetaField));
		}
	}
}