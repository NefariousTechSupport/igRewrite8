namespace igLibrary.Vfx
{
	[igStruct]
	public struct igVfxModulationHelper
	{
		[igEnum]
		public enum Distribution : int
		{
			kDistributionLinear = 0,
			kDistributionBinary = 1,
			kDistributionNormal = 2
		}


		[igEnum]
		public enum ModulationMix : int
		{
			kMixRange = 0,
			kMixAdd = 1
		}


		[igEnum]
		public enum ModulationType : int
		{
			kModulationNone = 0,
			kModulationPoint = 1,
			kModulationLinear = 2,
			kModulationSmooth = 3,
			kModulationWhite = 4,
			kModulationSin = 5,
			kModulationRampUp = 6,
			kModulationRampDown = 7,
			kModulationTriangle = 8
		}



		public float _mixAmount;
		public float _phaseOffset;
		public float _distributionArgument;
		public float _modulationCycles;         // Must be between 1 and 65535
		public ushort _modulationCyclesInt;     // _modulationCycles but rounded to the nearest int
		public bool _hasModulation;             // equal to modulationType != kModulationNone
		public byte _flags;
		public ModulationType _modulationType;
		public Distribution _distribution;
		public ModulationMix _mixType;
		public bool _randomPhase;



		/*public Distribution DistributionFlag
		{
			get => (Distribution)((_flags >> 4) & 0b11);
			set => _flags = (byte)((_flags & 0b11001111u) | (byte)((byte)value << 4));
		}



		public ModulationMix ModulationMixFlag
		{
			get => (ModulationMix)((_flags >> 6) & 0b1);
			set => _flags = (byte)((_flags & 0b10111111u) | (byte)((byte)value << 6));
		}



		public ModulationType ModulationTypeFlag
		{
			get
			{
				return (ModulationType)(_flags & 0b1111);
			}
			set
			{
				_flags = (byte)((_flags & 0b11110000u) | (byte)value);
				_hasModulation = value != ModulationType.kModulationNone;
			}
		}



		public float ModulationCycles
		{
			get
			{
				return _modulationCycles;
			}
			set
			{
				_modulationCycles    = System.Math.Clamp(value, 1f, 65535f);
				_modulationCyclesInt = (ushort)System.Math.Round(_modulationCycles);
			}
		}



		public bool RandomPhase
		{
			get => (_flags & 0b10000000) != 0;
			set => _flags |= (byte)(value ? 0b10000000u : 0u);
		}*/



		public igVfxModulationHelper()
		{
			_mixAmount = 1f;
			_phaseOffset = 0f;
			_distributionArgument = 0.5f;
			_modulationCycles = 8f;
			_modulationCyclesInt = 8;
			_hasModulation = false;
			_flags = 0;
			_modulationType = ModulationType.kModulationNone;
			_distribution = Distribution.kDistributionLinear;
			_mixType = ModulationMix.kMixRange;
			_randomPhase = false;
		}
	}
}