using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Microsoft.Office.Interop.Outlook;
using YTech.Fogbugz;

namespace YTech.FogbugzOutlook
{
    [TestClass]
    public class TaskListSyncTests
    {
        [TestMethod]
        public void TasksInFogbugzNotOutlook()
        {
            var mockOutlook = MockRepository.GenerateStrictMock<IOutlook>();
            var mockFogbugz = MockRepository.GenerateMock<IFogbugzConnector>();
            var mockTaskSync = MockRepository.GenerateStrictMock<ITaskSync>();

            var mockTask1 = CreateOutlookTask();
            var mockTask2 = CreateOutlookTask();

            var cases = new List<Case>()
                {
                    new Case() { CaseId = 123 },
                    new Case() { CaseId = 456 },
                };

            mockOutlook.Expect(x => x.CreateTask()).Return(mockTask1);
            mockOutlook.Expect(x => x.CreateTask()).Return(mockTask2);

            mockTaskSync.Expect(x => x.SyncTask(cases[0], mockTask1));
            mockTaskSync.Expect(x => x.SyncTask(cases[1], mockTask2));

            mockOutlook.Expect(x => x.SaveTask(mockTask1));
            mockOutlook.Expect(x => x.SaveTask(mockTask2));

            var tls = new TaskListSync(mockOutlook, mockFogbugz, mockTaskSync);
            tls.SyncTasks(cases, new List<TaskItem>());

            mockOutlook.VerifyAllExpectations();
        }

        [TestMethod]
        public void UpdateMatchingTasks()
        {
            var mockOutlook = MockRepository.GenerateStrictMock<IOutlook>();
            var mockFogbugz = MockRepository.GenerateMock<IFogbugzConnector>();
            var mockTaskSync = MockRepository.GenerateStrictMock<ITaskSync>();

            var mockTask1 = CreateOutlookTask();
            mockTask1.SetFogbugzCaseId(123);
            var mockTask2 = CreateOutlookTask();

            var cases = new List<Case>()
                {
                    new Case() { CaseId = 123 },
                };
            var tasks = new List<TaskItem>() { mockTask1, mockTask2 };

            mockTaskSync.Expect(x => x.SyncTask(cases[0], mockTask1));

            mockOutlook.Expect(x => x.SaveTask(mockTask1));

            var tls = new TaskListSync(mockOutlook, mockFogbugz, mockTaskSync);
            tls.SyncTasks(cases, tasks);

            mockOutlook.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteDuplicateTasks()
        {
            var mockOutlook = MockRepository.GenerateStrictMock<IOutlook>();
            var mockFogbugz = MockRepository.GenerateMock<IFogbugzConnector>();
            var mockTaskSync = MockRepository.GenerateMock<ITaskSync>();

            var cases = new List<Case>()
                {
                    new Case() { CaseId = 123 },
                    new Case() { CaseId = 456 }
                };

            var mockTask1 = CreateOutlookTask();
            mockTask1.SetFogbugzCaseId(123);
            var mockTask2 = CreateOutlookTask();
            mockTask2.SetFogbugzCaseId(123);
            var mockTask3 = CreateOutlookTask();
            mockTask3.SetFogbugzCaseId(456);
            var tasks = new List<TaskItem>() { mockTask1, mockTask2, mockTask3 };

            mockOutlook.Expect(x => x.SaveTask(mockTask2));
            mockOutlook.Expect(x => x.SaveTask(mockTask3));
            mockOutlook.Expect(x => x.DeleteTask(mockTask1));

            var tls = new TaskListSync(mockOutlook, mockFogbugz, mockTaskSync);
            tls.SyncTasks(cases, tasks);

            mockOutlook.VerifyAllExpectations();
        }

        [TestMethod]
        public void DeleteUnassignedCases()
        {
            var mockOutlook = MockRepository.GenerateStrictMock<IOutlook>();
            var mockFogbugz = MockRepository.GenerateMock<IFogbugzConnector>();
            var mockTaskSync = MockRepository.GenerateMock<ITaskSync>();

            var cases = new List<Case>()
                {
                    new Case() { CaseId = 123 },
                };

            var mockTask1 = CreateOutlookTask(); //Existing task
            mockTask1.SetFogbugzCaseId(123);
            var mockTask2 = CreateOutlookTask(); //Unassigned task
            mockTask2.SetFogbugzCaseId(456);
            var mockTask3 = CreateOutlookTask(); //Other user task

            var tasks = new List<TaskItem>() { mockTask1, mockTask2, mockTask3 };

            mockOutlook.Expect(x => x.SaveTask(mockTask1));
            mockOutlook.Expect(x => x.DeleteTask(mockTask2));

            var tls = new TaskListSync(mockOutlook, mockFogbugz, mockTaskSync);
            tls.SyncTasks(cases, tasks);

            mockOutlook.VerifyAllExpectations();
        }

        public static TaskItem CreateOutlookTask()
        {
            var app = new Microsoft.Office.Interop.Outlook.Application();
            return (TaskItem)app.CreateItem(OlItemType.olTaskItem);
        }
    }
}
