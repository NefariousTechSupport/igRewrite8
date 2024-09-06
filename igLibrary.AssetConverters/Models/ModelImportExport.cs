using igLibrary.Core;
using Assimp;

namespace igLibrary.AssetConversion.Models
{
	public abstract class ModelImportExport<T> where T : igObject
	{
		public abstract ModelData ExportModel(T gameAsset);
		public abstract T ImportModel(Scene model);
	}
}