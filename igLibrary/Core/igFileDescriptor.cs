/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public class igFileDescriptor
	{
		public string _path;
		public ulong _position;
		public ulong _size;
		public igStorageDevice _device;
		public Stream _handle;
		//public igSignal _signal;
		public uint _flags;
		public int _workItemActiveCount;
		public static int _counter;
		public StreamHelper _stream;	//Should work on deprecating this, _handle is now a Stream

		public igFileDescriptor(){}
		public igFileDescriptor(Stream data, string path, StreamHelper.Endianness endianness = StreamHelper.Endianness.Little)
		{
			_stream = new StreamHelper(data, endianness);
			_path = path;
		}
	}
}