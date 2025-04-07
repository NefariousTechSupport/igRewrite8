/*
	Copyright (c) 2022-2025, The igCauldron Contributors.
	igCauldron and its libraries are free software: You can redistribute it and
	its libraries under the terms of the Apache License 2.0 as published by
	The Apache Software Foundation.
	Please see the LICENSE file for more details.
*/


namespace igCauldron3
{
	/// <summary>
	/// Abstract class for a ui "frame" aka a portion of the ui
	/// </summary>
	public abstract class Frame
	{
		protected List<Frame> _children = new List<Frame>();
		protected Window _wnd;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="wnd">The window this frame belongs to</param>
		/// <exception cref="ArgumentNullException">wnd must be non null</exception>
		public Frame(Window wnd)
		{
			if(wnd == null) throw new ArgumentNullException(nameof(wnd));

			_wnd = wnd;
		}


		/// <summary>
		/// Renders the UI
		/// </summary>
		public virtual void Render()
		{
			for(int i = 0; i < _children.Count; i++)
			{
				_children[i].Render();
			}
		}


		/// <summary>
		/// Closes the frame
		/// </summary>
		public virtual void Close()
		{
			_wnd._frames.Remove(this);
		}


		/// <summary>
		/// Adds a child frame
		/// </summary>
		/// <param name="child"></param>
		public void AddChild(Frame child)
		{
			_children.Add(child);
		}
	}
}