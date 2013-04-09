using System;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YTech.FogbugzOutlook;

namespace UnitTests
{
	[TestClass]
	public class OutlookTaskExtensionTests
	{
		[TestMethod]
		public void TestProperties()
		{
			var app = new Application();
			var t = (TaskItem) app.CreateItem(OlItemType.olTaskItem);

			t.SetFogbugzCaseId(123);
			Assert.AreEqual(123, t.GetFogbugzCaseId());
		}

		[TestMethod]
		public void SetLastUpdateTime()
		{
			var app = new Application();
			var t = (TaskItem) app.CreateItem(OlItemType.olTaskItem);

			t.SetLastModificationDateForTesting(DateTime.Parse("2013-1-1 10:00am"));
			Assert.AreEqual(DateTime.Parse("2013-1-1 10:00am"), t.GetLastModifiedDate());
		}

		[TestMethod]
		public void LastUpdateTimeNullWhenNotSet()
		{
				var app = new Application();
				var t = (TaskItem)app.CreateItem(OlItemType.olTaskItem);

				Assert.AreEqual(null, t.GetLastModifiedDate());
		}

		[TestMethod]
		public void LastUpdateTimeWhenSet()
		{
				var app = new Application();
				var t = (TaskItem)app.CreateItem(OlItemType.olTaskItem);
			t.Save();

				Assert.AreEqual(t.LastModificationTime, t.GetLastModifiedDate());
		}

		[TestMethod]
		public void SetLastSync()
		{
				var app = new Application();
				var t = (TaskItem)app.CreateItem(OlItemType.olTaskItem);

				t.SetLastSync(DateTime.Parse("2013-1-1 10:00am"));
				Assert.AreEqual(DateTime.Parse("2013-1-1 10:00am"), t.GetLastSync());
		}
	}
}
