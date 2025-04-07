/*
	Copyright (c) 2022-2025, The igLibrary Contributors.
	igLibrary and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igLibrary.Core
{
	/// <summary>
	/// The archive mount manager
	/// </summary>
	public class igArchiveMountManager : igFileWorkItemProcessor
	{
		public bool _isWorkPending;


		/// <summary>
		/// Process a work item
		/// </summary>
		/// <param name="workItem">the work item</param>
		public override void Process(igFileWorkItem workItem)
		{
			SendToNextProcessor(workItem);
		}


		/// <summary>
		/// Mount a specific archive
		/// </summary>
		/// <param name="archive">The archive</param>
		public void MountArchive(igArchive archive)
		{
			igFileWorkItem workItem = igFileContext.Singleton.AllocateWorkItem();
			workItem._buffer = archive;
			workItem._type = igFileWorkItem.WorkType.kTypeInvalid;
			workItem._priority = igFileWorkItem.Priority.kPriorityNormal;
			workItem._status = igFileWorkItem.Status.kStatusInactive;
		}
	}
}