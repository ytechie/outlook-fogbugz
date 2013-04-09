using System;
using System.Collections.Generic;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YTech.Fogbugz;

namespace YTech.FogbugzOutlook
{
	[TestClass]
	public class TaskSyncTests
	{
		[TestMethod]
		public void LoadEmbeddedTaskTemplate()
		{
			var template = TaskSync.GetEmbeddedTaskTemplate();
			Assert.IsTrue(template.StartsWith(@"{\rtf1"));
		}

		[TestMethod]
		public void GenerateTaskBody()
		{
			var template = TaskSync.GenerateTaskBody(new List<KeyValuePair<string, string>>());
			Assert.IsTrue(template.StartsWith(@"{\rtf1"));
		}

		[TestMethod]
		public void TaskUpdatesFogbugz()
		{
			var ts = new TaskSync();

			var c = new Case {LastUpdated = DateTime.Parse("2013-1-1 9:00am")};
			c.PercentComplete = 20;
			c.Subject = "case subject";
			c.Priority = 1;
			c.Due = DateTime.Parse("2013-2-1 8:00am");
			c.ResetUpdateFlags();

			var t = TaskListSyncTests.CreateOutlookTask();
			t.SetLastModificationDateForTesting(DateTime.Parse("2013-1-1 10:00am"));
			t.PercentComplete = 30;
			t.Subject = "task subject";
			t.Importance = OlImportance.olImportanceLow;
			t.DueDate = DateTime.Parse("2013-3-1");

			ts.SyncTask(c, t);

			Assert.AreEqual(30, c.PercentComplete);
			Assert.AreEqual("task subject", c.Subject);
			Assert.AreEqual(1, c.Priority); //doesn't change
			Assert.AreEqual(DateTime.Parse("2013-3-1"), c.Due);
		}

		[TestMethod]
		public void FogbugzUpdatesTask()
		{
			var ts = new TaskSync();

			var c = new Case {LastUpdated = DateTime.Parse("2013-1-1 9:00am")};
			c.CaseId = 123;
			c.PercentComplete = 20;
			c.Subject = "case subject";
			c.Priority = 1;
			c.Due = DateTime.Parse("2013-2-1 8:00am");
			c.ResetUpdateFlags();

			var t = TaskListSyncTests.CreateOutlookTask();
			t.SetLastModificationDateForTesting(DateTime.Parse("2013-1-1 9:00am"));
			t.PercentComplete = 30;
			t.Subject = "task subject";
			t.Importance = OlImportance.olImportanceLow;
			t.DueDate = DateTime.Parse("2013-3-1");

			ts.SyncTask(c, t);

			Assert.AreEqual(20, t.PercentComplete);
			Assert.AreEqual("case subject (Case 123)", t.Subject);
			Assert.AreEqual(OlImportance.olImportanceHigh, t.Importance); //this doesn't change
			Assert.AreEqual(DateTime.Parse("2013-2-1"), t.DueDate);
			Assert.AreEqual(123, t.GetFogbugzCaseId());
		}

		[TestMethod]
		public void SetDueDate()
		{
			var c = new Case();
			var t = TaskListSyncTests.CreateOutlookTask();
			t.DueDate = DateTime.Parse("2013-01-01");
				t.SetLastModificationDateForTesting(DateTime.Now);

			var ts = new TaskSync();
			ts.SyncTask(c, t);

			Assert.AreEqual(DateTime.Parse("2013-01-01"), c.Due);
		}

		[TestMethod]
		public void SetDueDateDifferentDay()
		{
				var c = new Case();
			c.Due = DateTime.Parse("2013-01-01 6:00am");

				var t = TaskListSyncTests.CreateOutlookTask();
				t.DueDate = DateTime.Parse("2013-01-02");
				t.SetLastModificationDateForTesting(DateTime.Now);

				var ts = new TaskSync();
				ts.SyncTask(c, t);

				Assert.AreEqual(DateTime.Parse("2013-01-02"), c.Due);
		}

		[TestMethod]
		public void SetDueDateDontMessUpFogbugzTime()
		{
				var c = new Case();
			c.Due = DateTime.Parse("2013-01-01 6:00am");

				var t = TaskListSyncTests.CreateOutlookTask();
				t.DueDate = DateTime.Parse("2013-01-01");
				t.SetLastModificationDateForTesting(DateTime.Now);

				var ts = new TaskSync();
				ts.SyncTask(c, t);

				Assert.AreEqual(DateTime.Parse("2013-01-01 6:00am"), c.Due);
		}

		//[TestMethod]
		//public void SetDueDateDontMessUpFogbugzTime()
		//{
		//		var app = new Application();
		//		var t = (TaskItem)app.CreateItem(OlItemType.olTaskItem);
		//		t.DueDate = DateTime.Parse("2013-01-01");

		//		Assert.AreEqual(DateTime.Parse("2013-01-01"), t.DueDate);
		//}
	}
}