using System;
using Microsoft.Office.Interop.Outlook;

namespace YTech.FogbugzOutlook
{
		public class TaskChangedEventArgs : EventArgs
		{
			private readonly TaskItem _task;

				public TaskChangedEventArgs(TaskItem task)
				{
					_task = task;
				}

				public TaskItem Task
				{
					get
					{
						return _task;
					}
				}
		}
}
