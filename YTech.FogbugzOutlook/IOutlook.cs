using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;

namespace YTech.FogbugzOutlook
{
    public interface IOutlook
    {
	    event EventHandler<TaskChangedEventArgs> TaskChanged;
        IEnumerable<TaskItem> GetTasksFromDefaultList();
        TaskItem CreateTask();
        void SaveTask(TaskItem task);
        void DeleteTask(TaskItem task);
        bool IsOnline { get; }
	    void SubscribeToTaskChanges();
    }
}
