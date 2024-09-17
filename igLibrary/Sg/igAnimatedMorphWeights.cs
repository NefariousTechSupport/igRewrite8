using igLibrary.Utils;

namespace igLibrary.Sg
{
	public class igAnimatedMorphWeights : igObject
	{
		public igVector<float> _data;
		public igVector<float> _times;
		public int _targetCount;
		public float _duration;
		public float _keyframeTimeOffset;
		public IG_UTILS_PLAY_MODE _playMode;
	}
}