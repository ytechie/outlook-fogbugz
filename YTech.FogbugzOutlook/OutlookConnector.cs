using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

namespace YTech.FogbugzOutlook
{
    public class OutlookConnector : IOutlook
    {
				private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    readonly Application _app;

        //This is what Outlook uses to represent "none" for a date
        public static readonly DateTime EmptyDate = new DateTime(4501, 1, 1);

	    public event EventHandler<TaskChangedEventArgs> TaskChanged = (x,y) => { };

        public OutlookConnector(Application app)
        {
            _app = app;
        }

        public IEnumerable<TaskItem> GetTasksFromDefaultList()
        {
						Log.Debug("Loading Outlook namespace");
            var taskFolder = _app.GetNamespace("MAPI").GetDefaultFolder(OlDefaultFolders.olFolderTasks);

            //I use a list instead of yield return to ensure we only load once
            var tasks = new List<TaskItem>();
            foreach (TaskItem task in taskFolder.Items)
            {
                tasks.Add(task);
            }
						Log.DebugFormat("{0} tasks loaded from Outlook", tasks.Count);

            return tasks;
        }

        public void SaveTask(TaskItem task)
        {
						Log.DebugFormat("Saving task with subject '{0}'", task.Subject);
            task.Save();
        }


        public TaskItem CreateTask()
        {
            return (TaskItem)_app.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olTaskItem);
        }


        public void DeleteTask(TaskItem task)
        {
						Log.DebugFormat("Deleting task with subject '{0}'", task.Subject);
            task.Delete();
        }

        public bool IsOnline
        {
            get
            {
                var mode = _app.GetNamespace("MAPI").ExchangeConnectionMode;
								Log.DebugFormat("Outlook connection mode: '{0}'", mode.ToString());
                return mode == OlExchangeConnectionMode.olOnline
                    || mode == OlExchangeConnectionMode.olCachedConnectedFull
                    || mode == OlExchangeConnectionMode.olCachedConnectedHeaders
                    || mode == OlExchangeConnectionMode.olCachedConnectedDrizzle;
            }
        }

	    private List<ItemEvents_10_AfterWriteEventHandler> _taskWriteEvents = new List<ItemEvents_10_AfterWriteEventHandler>();

	    public void SubscribeToTaskChanges()
	    {
		    var tasks = GetTasksFromDefaultList();

		    foreach (var task in tasks)
		    {
			    var t = task;

			    var d = new ItemEvents_10_AfterWriteEventHandler(() =>
				    {
					    Log.Debug("Task Change Event Fired");
					    TaskChanged(this, new TaskChangedEventArgs(t));
				    });
			    task.AfterWrite += d;

			    task.BeforeDelete += (object o, ref bool b) =>
				    {
					    Log.Debug("Removing 'AfterWrite' event handler for item that is being deleted");
					    //Run register the event handler so that it can be properly disposed by outlook
					    t.AfterWrite -= d;
				    };
		    }
	    }
    }
}
