using System.Collections.Generic;
using System.Linq;
using Microsoft.Office.Interop.Outlook;
using YTech.Fogbugz;

namespace YTech.FogbugzOutlook
{
	public class TaskListSync
	{
		private readonly ITaskSync _taskSync;
		private readonly IOutlook _outlook;
		private readonly IFogbugzConnector _fogbugz;

		public TaskListSync(IOutlook outlook, IFogbugzConnector fogbugz, ITaskSync taskSync)
		{
			_outlook = outlook;
			_fogbugz = fogbugz;
			_taskSync = taskSync;
		}

		public void SyncTasks(IEnumerable<Case> cases, IEnumerable<TaskItem> tasks)
		{
			var availableTasks = tasks.ToList();

			var tasksToDelete = new List<TaskItem>();

			if (availableTasks.Any(x => x.IsConflict))
			{
				throw new TaskConflictException();
			}

			//Find & delete duplicate cases
			var tasksWithACaseId = availableTasks.Where(x => x.GetFogbugzCaseId() != null).ToList();
			var groupedTasks = from t in tasksWithACaseId group t by t.GetFogbugzCaseId() into g select new {g.Key, Tasks = g};
			foreach (var outlookTask in groupedTasks.ToList())
			{
				var count = outlookTask.Tasks.Count();
				if (count > 1)
				{
					//There are duplicates

					//Determine the best to keep
					var taskToKeep = outlookTask.Tasks.Last();
					var oldTasks = outlookTask.Tasks.Where(x => x != taskToKeep);

					foreach (var oldTask in oldTasks)
					{
						availableTasks.Remove(oldTask);
						tasksToDelete.Add(oldTask);
					}
				}
			}

			//Delete tasks that are no longer assigned to you
			var orphanTasks = tasksWithACaseId.Where(x => cases.Count(y => y.CaseId == x.GetFogbugzCaseId()) == 0);
			foreach (var orphanTask in orphanTasks)
			{
				availableTasks.Remove(orphanTask);
				tasksToDelete.Add(orphanTask);
			}

			//Update tasks associated with fogbugz
			foreach (var task in availableTasks)
			{
				var fogbugzCaseId = task.GetFogbugzCaseId();

				if (fogbugzCaseId != null)
				{
					//This is associated with a case in Fogbugz

					var fogbugzCase = cases.SingleOrDefault(x => x.CaseId == fogbugzCaseId);

					//TODO: Handle cases that may no longer belong to the user
					if (fogbugzCase == null)
						continue;
					//    throw new System.Exception("Could not find a case in Fogbugz with ID: " + fogbugzCaseId.ToString());

					_taskSync.SyncTask(fogbugzCase, task);
					_outlook.SaveTask(task);
					_fogbugz.SaveCase(fogbugzCase);
				}
			}

			//Add tasks that are in fogbugz, but not in Outlook
			foreach (var fogbugzCase in cases)
			{
				if (availableTasks.Where(x => x.GetFogbugzCaseId() == fogbugzCase.CaseId).Count() == 0)
				{
					var newTask = _outlook.CreateTask();
					_taskSync.SyncTask(fogbugzCase, newTask);

					_outlook.SaveTask(newTask);
					_fogbugz.SaveCase(fogbugzCase);
				}
			}

			foreach (var taskToDelete in tasksToDelete.Distinct())
			{
				_outlook.DeleteTask(taskToDelete);
			}
		}
	}
}