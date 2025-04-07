/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


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