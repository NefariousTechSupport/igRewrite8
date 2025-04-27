using System.Globalization;


namespace igLibrary
{
	/// <summary>
	/// Localisation constatants
	/// </summary>
	internal static class Localisation
	{
		// These exist to fix #102, otherwise people in other countries may get weird bugs
		public const NumberStyles kENNumberStyles = NumberStyles.AllowDecimalPoint; 
		public static readonly CultureInfo  kENCultureInfo  = CultureInfo.CreateSpecificCulture("en");
	}
}