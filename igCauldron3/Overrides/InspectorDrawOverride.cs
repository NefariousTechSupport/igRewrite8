using igLibrary.Core;

namespace igCauldron3
{
	public abstract class InspectorDrawOverride
	{
		public Type _t { get; protected set; }
		public abstract void Draw2(DirectoryManagerFrame dirFrame, string id, igObject obj, igMetaObject meta);
	}
}