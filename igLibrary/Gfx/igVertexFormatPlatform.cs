/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Gfx
{
	public class igVertexFormatPlatform : igObject
	{
		public bool _disableSoftwareBlending;
	}
	public class igVertexFormatAspen : igVertexFormatPlatform {}
	public class igVertexFormatCafe : igVertexFormatPlatform {}
	public class igVertexFormatDurango : igVertexFormatPlatform {}
	public class igVertexFormatDX : igVertexFormatPlatform {}
	public class igVertexFormatMetal : igVertexFormatPlatform {}
	public class igVertexFormatPS3 : igVertexFormatPlatform {}
	public class igVertexFormatWii : igVertexFormatPlatform {}
	public class igVertexFormatXenon : igVertexFormatPlatform {}
	public class igVertexFormatOSX : igVertexFormatPlatform {}
	public class igVertexFormatDX11 : igVertexFormatPlatform {}
	public class igVertexFormatRaspi : igVertexFormatPlatform {}
	public class igVertexFormatNull : igVertexFormatPlatform {}
	public class igVertexFormatAndroid : igVertexFormatPlatform {}
	public class igVertexFormatWgl : igVertexFormatPlatform {}
	public class igVertexFormatLGTV : igVertexFormatPlatform {}
	public class igVertexFormatPS4 : igVertexFormatPlatform {}
	public class igVertexFormatLinux : igVertexFormatPlatform {}
}