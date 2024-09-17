namespace igLibrary.Gfx
{
	public class igIndexFormat : igObject
	{
		public IG_INDEX_TYPE _indexType = IG_INDEX_TYPE.IG_INDEX_TYPE_INT16;
		public IG_GFX_PLATFORM _platform;
		public uint _headerSize;
		public uint _alignment = 4;
		public bool _hasRestartIndices;
		public bool _dynamic;
		public readonly static string _indexFormatNamespace = "indexformats";
		public readonly static igObjectList _indexFormats = new igObjectList();

		public static string GetFormatName(IG_INDEX_TYPE indexType, IG_GFX_PLATFORM platform, bool dynamic)
		{
			string formatName;
			switch(indexType)
			{
				case IG_INDEX_TYPE.IG_INDEX_TYPE_INT8:  formatName = "i8";  break;
				case IG_INDEX_TYPE.IG_INDEX_TYPE_INT16: formatName = "i16"; break;
				case IG_INDEX_TYPE.IG_INDEX_TYPE_INT32: formatName = "i32"; break;
				default: throw new NotSupportedException($"Index type {indexType} is not supported");
			}

			string platformName = platform.ToString().Substring(15);	// 15 is the length of "IG_GFX_PLATFORM";

			if(dynamic)
			{
				formatName += "d";
			}

			if(platform != IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT)
			{
				formatName += platformName;
			}

			return formatName.ToLower();
		}
		public static igIndexFormat CreateIndexFormat(IG_INDEX_TYPE indexType, IG_GFX_PLATFORM platform, bool dynamic)
		{
			igIndexFormat indexFormat = new igIndexFormat();
			indexFormat._indexType = indexType;
			indexFormat._platform = platform;
			indexFormat._dynamic = dynamic;

			switch(platform)
			{
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_WII:
					indexFormat._headerSize = 4;
					indexFormat._alignment = 0x20;
					break;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_PS3:
					indexFormat._alignment = 0x10;
					indexFormat._hasRestartIndices = false;
					break;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_CAFE:
					indexFormat._alignment = 0x20;
					break;
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DEFAULT:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DX:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_DURANGO:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_ASPEN:
				case IG_GFX_PLATFORM.IG_GFX_PLATFORM_XENON:
					indexFormat._hasRestartIndices = false;
					break;
			}

			return indexFormat;
		}
	}
}