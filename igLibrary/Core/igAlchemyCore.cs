/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// Represents the core of the library and contains common utilities
	/// </summary>
	public static class igAlchemyCore
	{
		/// <summary>
		/// Sets up the entire library
		/// </summary>
		public static void InitializeSystems()
		{
			CClient.Singleton.AddManager<CPrecacheManager>();
			CClient.Singleton.AddManager<CDotNetaManager>();

			Gfx.igGfx.Initialize();
			
			igObjectLoader.RegisterClass<igIGZObjectLoader>();
			igObjectLoader.RegisterClass<CLngObjectLoader>();
			igObjectLoader.RegisterClass<DotNet.igDotNetMetaOnlyLibraryLoader>();

			igExternalReferenceSystem.Singleton._globalSet.Add("metaobject", new igMetaObjectReferenceResolver());
			igExternalReferenceSystem.Singleton._globalSet.Add("metafield", new igMetaFieldReferenceResolver());

			CArchive.s_bCacheEnabled = igRegistry.GetRegistry()._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN || igRegistry.GetRegistry()._platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64;
		}


		/// <summary>
		/// Is the specified platform 64 bit
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <returns>Whether the platform's 64 bit</returns>
		public static bool isPlatform64Bit(IG_CORE_PLATFORM platform)
		{
			switch(platform)
			{
				default:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX:
					return false;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX:
					return true;
			}
		}


		/// <summary>
		/// Gets the size of the pointer
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <returns>Gets the size of the pointer</returns>
		public static uint GetPointerSize(IG_CORE_PLATFORM platform) => isPlatform64Bit(platform) ? 8u : 4u;


		/// <summary>
		/// Whether the specified platform is big endian
		/// </summary>
		/// <param name="platform">the platform</param>
		/// <returns>whether the platform is big endian</returns>
		public static bool isPlatformBigEndian(IG_CORE_PLATFORM platform)
		{
			switch(platform)
			{
				default:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEPRECATED:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_NGP:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MARMALADE:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_MAX:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX:
					return false;
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE:
					return true;
			}
		}


		/// <summary>
		/// Gets the name of the platform
		/// </summary>
		/// <param name="platform">The platform</param>
		/// <returns>The name of the platform</returns>
		public static string GetPlatformString(IG_CORE_PLATFORM platform)
		{
			     if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32)   return "win";
			else if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN)   return "aspenLow";
			else if(platform == IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64) return "aspenHigh";
			else
			switch(platform)
			{
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DEFAULT: return "unknown";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN32:   return "win32";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WII:     return "wii";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_DURANGO: return "durango";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN:   return "aspen";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_XENON:   return "xenon";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS3:     return "ps3";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_OSX:     return "osx";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WIN64:   return "win64";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_CAFE:    return "cafe";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_RASPI:   return "raspi";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ANDROID: return "android";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_ASPEN64: return "aspen64";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LGTV:    return "lgtv";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_PS4:     return "ps4";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_WP8:     return "wp8";
				case IG_CORE_PLATFORM.IG_CORE_PLATFORM_LINUX:   return "linux";
				default:                       return string.Empty;
			}
		}
	}
}