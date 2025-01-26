/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	public abstract class igFileWorkItemProcessor : igObject
	{
		public igFileWorkItemList _workList;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _threadList;    //igThreadList
		[Obsolete("This exists for the reflection system, do not use.")] public object? _workListLock;  //igSemaphore
		[Obsolete("This exists for the reflection system, do not use.")] public object? _workPending;   //igSemaphore
		public igFileWorkItemProcessor _nextProcessor;
		public bool _workerThreadsActive;
		public bool _allowPause;
		[Obsolete("This exists for the reflection system, do not use.")] public object? _pauseSignal;   //igSignal
		public int _pauseCounter;
		public abstract void Process(igFileWorkItem workItem);
		public void SendToNextProcessor(igFileWorkItem workItem)
		{
			if(workItem._status == igFileWorkItem.Status.kStatusComplete) return;
			if(_nextProcessor != null)
			{
				_nextProcessor.Process(workItem);
			}
			/*else
			{
 				throw new IOException($"Ran out of file processors trying to do work of type {workItem._type}, path {workItem._path}");
 			}*/
		}
	}
}