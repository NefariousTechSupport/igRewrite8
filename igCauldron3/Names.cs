using igLibrary.Core;

namespace igCauldron3
{
	public static class Names
	{
		public static (IG_CORE_PLATFORM, string)[] PlatformNames = new (IG_CORE_PLATFORM, string)[]
		{
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID,    "Android 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN,      "iOS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64,    "iOS 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX,      "Linux"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV,       "LG Smart TV"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX,        "Mac OS 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE,  "Marmalade"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP,        "PSVita"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3,        "PS3"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4,        "PS4"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI,      "Raspberry Pi"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII,        "Wii"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE,       "Wii U"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32,      "Windows 32-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64,      "Windows 64-bit"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8,        "Windows Phone"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON,      "Xbox 360"),
			(IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO,    "Xbox One")
		};
		public static (igArkCore.EGame, string)[] GameNames = new (igArkCore.EGame, string)[]
		{
			(igArkCore.EGame.EV_SkylandersSuperchargers,   "Skylanders Superchargers 1.6.X"),
			(igArkCore.EGame.EV_SkylandersImaginators,     "Skylanders Imaginators 1.1.X")
		};
	}
}