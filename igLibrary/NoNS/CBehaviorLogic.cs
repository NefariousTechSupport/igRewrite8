namespace igLibrary
{
	public class CBehaviorLogic : CBaseBehaviorLogic
	{
		public igStringStringHashTable _activators;
		public igStringStringHashTable _excludeActivators;
		public CBaseUpgradeFilter _skillUpgradeFilter;
		public CBaseVehicleModeFilter _vehicleModeFilter;
		public bool _playerOnly;
		public bool _useProxy;
		public bool _useProxyInputOnly;
		public bool _useProxyPassengerOnly;
		public bool _disable;
		public igMetaObject _meta;
	}

	public class CBehaviorLogicDataTable : igTUHashTable<igObject, string> {}
}
