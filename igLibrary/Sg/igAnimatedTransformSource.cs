using igLibrary.Utils;

namespace igLibrary.Sg
{
	public class igAnimatedTransformSource : igObject
    {
		public int _dataStride;
		public igVector<float> _data;
		public igVector<float> _times;
		public float _duration;
		public float _keyframeTimeOffset;
		public uint _componentChannels;
		public byte[] _channelOffsets;
		public byte[] _interpolationMethod;
		public IG_UTILS_PLAY_MODE _playMode;
		public igVec4f _centerOfRotation;
	}
}