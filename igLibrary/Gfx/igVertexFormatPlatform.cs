namespace igLibrary.Gfx
{
	// If you're going to implement any of these, move it into its own file

	/// <summary>
	/// Base class for various vertex formats
	/// </summary>
	public class igVertexFormatPlatform : igObject
	{
		public bool _disableSoftwareBlending;
	}
	public class igVertexFormatAspen : igVertexFormatPlatform {}
	public class igVertexFormatCafe : igVertexFormatPlatform {}
	public class igVertexFormatDurango : igVertexFormatPlatform {}
	public class igVertexFormatDX : igVertexFormatPlatform {}
	public class igVertexFormatMetal : igVertexFormatPlatform {}
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