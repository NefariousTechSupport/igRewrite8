using igLibrary.Core;
using Assimp;

namespace igLibrary.AssetConversion.Models
{
	public abstract class ModelImportExport<ModelType, SkinType>
		where ModelType : igObject
		where SkinType : igObject
	{
		public abstract ModelData ExportModel(ModelType gameAsset);
		public abstract ModelType ImportModel(Scene model);

		public abstract ModelData ExportActor(SkinType gameAsset);
		public abstract SkinType ImportActor(Scene model);
	}
}