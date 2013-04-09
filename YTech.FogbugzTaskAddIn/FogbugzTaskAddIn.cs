using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using YTech.Fogbugz;
using YTech.FogbugzOutlook;

namespace YTech.FogbugzTaskAddIn
{
	public partial class FogbugzTaskAddIn
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		//These need to be set here until I add a GUI
		private const string FogbugzUrl = "http://example.fogbugz.com";
		private const string EmailDomain = "@example.com";
		private const string AdminUser = "admin@example.com";
		private const string AdminPass = "password";

		private IOutlook _outlook;

		private static DateTime _lastConflictMessage = new DateTime();

		private void ThisAddIn_Startup(object sender, System.EventArgs e)
		{
#if(DEBUG)
			log4net.Util.LogLog.InternalDebugging = true;
#endif

			var dir = GetExecutionPath();
			var logConfigPath = Path.Combine(dir, "Logging.config");
			//MessageBox.Show("v5");
			//MessageBox.Show("Logs stored in: " + logConfigPath);
			var logConfig = new FileInfo(logConfigPath);
			log4net.Config.XmlConfigurator.ConfigureAndWatch(logConfig);
			Log.Info("Add-in Starting");

			var fi = new FileInfo("Logging.config");

			try
			{
				_outlook = new OutlookConnector(Application);

#if(DEBUG)
				RunFullSync();
#endif

				var ts = new ThreadStart(() => SyncLoop(RunFullSync));
				var t = new Thread(ts)
					{
						Priority = ThreadPriority.BelowNormal,
						IsBackground = true,
						Name = "OutlookFogbugzTaskAddingBackground"
					};
				t.Start();
			}
			catch (Exception ex)
			{
					Log.Error("Error initializing", ex);
			}
		}

		private static string GetExecutionPath()
		{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;
				var uri = new UriBuilder(codeBase);
				var path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
		}

		private static void SyncLoop(System.Action syncAction)
		{
			while (true)
			{
				try
				{
					Log.Debug("Starting sync loop");
					syncAction();
				}
				catch (TaskConflictException tcex)
				{
					Log.Error("Task conflict, the user needs to resolve this before a sync can occur");

					//Show the user a message if they haven't resolved the conflict recently
					if (DateTime.Now.Subtract(_lastConflictMessage).TotalHours >= 4)
					{
						MessageBox.Show(
							"You have Outlook tasks that are in conflict with the server. Please go to your task list to resolve.");
						_lastConflictMessage = DateTime.Now;
					}
				}
				catch (System.Exception ex)
				{
					Log.Error("Error running sync loop. It will be attempted again in the next cycle", ex);
					//What should I do with this?
#if(DEBUG)
					//MessageBox.Show(ex.ToString());
#endif
				}

				Log.Debug("Sleeping thread until next sync loop");
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}
		}

		private void RunFullSync()
		{
			Log.Debug("Running a full Fogbugz <-> Outlook sync");
			

			if (!_outlook.IsOnline)
			{
				Log.Debug("Skipping Outlook sync due to Outlook being disconnected");
				return;
			}

			//Note: This domain needs to be set to your user domain
			var assigneeEmail = string.Format("{0}{1}", Environment.UserName, EmailDomain);
			Log.DebugFormat("User email address identified as {0}", assigneeEmail);
			var fogbugz = new FogbugzConnector(FogbugzUrl, AdminUser, AdminPass, assigneeEmail);

			var fogbugzUser = fogbugz.GetCurrentFogbugzUser();

			if (fogbugzUser == null)
			{
				var msg = string.Format("Could not find a user in Fogbugz associated with the email address '{0}'",
				                        assigneeEmail);
				Log.Error(msg);
				MessageBox.Show(msg);
				return;
			}

			var cases = fogbugz.LoadCases(fogbugzUser.FullName);
			var tasks = _outlook.GetTasksFromDefaultList();

			var taskListSync = new TaskListSync(new OutlookConnector(Application), fogbugz, new TaskSync());
			taskListSync.SyncTasks(cases, tasks);
		}

		#region VSTO generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InternalStartup()
		{
			this.Startup += ThisAddIn_Startup;
		}

		#endregion
	}
}