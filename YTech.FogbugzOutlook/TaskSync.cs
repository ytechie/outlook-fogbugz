using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Office.Interop.Outlook;
using YTech.Fogbugz;
using Exception = System.Exception;

namespace YTech.FogbugzOutlook
{
	public class TaskSync : ITaskSync
	{
		public const string CaseIdUserPropertyName = "FogbugzCaseId";
		public const string LastSyncPropertyName = "FogbugzLastSync";

		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
		System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void SyncTask(Case fogbugzCase, TaskItem outlookTask)
		{
			try
			{
				//Associate the case and task
				outlookTask.SetFogbugzCaseId(fogbugzCase.CaseId);
				Log.DebugFormat("Setting task metadata case id to {0}", fogbugzCase.CaseId);

				var subjectWithCaseId = fogbugzCase.GetTitleWithCaseToken();

				var taskNewer = false;

				//Determine which is newer
				var taskLastModified = outlookTask.GetLastModifiedDate();
				if (taskLastModified != null && taskLastModified > fogbugzCase.LastUpdated)
					taskNewer = true;

				Log.DebugFormat("Fogbugz case last update: {0}", fogbugzCase.LastUpdated);
				Log.DebugFormat("Outlook task last update: {0}", taskLastModified);

				if (taskNewer)
				{
					Log.Debug("Task is newer than the fogbugz case");

					fogbugzCase.PercentComplete = outlookTask.PercentComplete;
					fogbugzCase.Subject = outlookTask.Subject;
					if (outlookTask.DueDate != OutlookConnector.EmptyDate)
					{
						if (fogbugzCase.Due != null && fogbugzCase.Due.Value.Date == outlookTask.DueDate.Date)
						{
							//Don't update fogbugz in this case, because we'll only be dropping the time component.
							//In other words, the fogbugz due date will be reset to midnight since Outlook doesn't store the time.
						}
						else
						{
							fogbugzCase.Due = outlookTask.DueDate;
						}
					}

					//Not sure how to translate priorities back to fogbugz
				}
				else
				{
					Log.Debug("Fogbugz case is newer than the task");

					outlookTask.PercentComplete = fogbugzCase.PercentComplete;
					outlookTask.Subject = subjectWithCaseId;

					if (fogbugzCase.Due != null)
					{
						outlookTask.DueDate = fogbugzCase.Due.Value;

						//Only populate the start date if there is a due date, otherwise
						//the task will have a due date the same as the start (stupid)
						outlookTask.StartDate = fogbugzCase.Opened;
					}

					if (fogbugzCase.Priority == 1 || fogbugzCase.Priority == 2)
						outlookTask.Importance = OlImportance.olImportanceHigh;
					else if (fogbugzCase.Priority == 4 || fogbugzCase.Priority == 5 || fogbugzCase.Priority == 6)
						outlookTask.Importance = OlImportance.olImportanceLow;
					else
						outlookTask.Importance = OlImportance.olImportanceNormal;
				}

				var tokens = new List<KeyValuePair<string, string>>
					{
						//Note; tokens must be lower case
						new KeyValuePair<string, string>("caseid", fogbugzCase.CaseId.ToString()),
						new KeyValuePair<string, string>("emailsubject", Uri.EscapeUriString(subjectWithCaseId))
					};

				outlookTask.RTFBody = Encoding.ASCII.GetBytes(GenerateTaskBody(tokens));
			}
			catch (Exception ex)
			{
				if(fogbugzCase != null)
						Log.Info(fogbugzCase.ToString());
				if(outlookTask != null)
						Log.Info(outlookTask.ToString());

				Log.Error("Error synchronizing tasks", ex);
				throw;
			}
		}

		public static string GenerateTaskBody(List<KeyValuePair<string, string>> tokenReplacments)
		{
			var rtfTemplate = GetEmbeddedTaskTemplate();
			foreach (var token in tokenReplacments)
			{
				var key = "{" + token.Key + "}";

				//This will encode plain text tokens
				rtfTemplate = rtfTemplate.Replace(key, token.Value);
				//This will replace tokens in URL's
				rtfTemplate = rtfTemplate.Replace(Uri.EscapeUriString(key).ToLower(), token.Value);
			}

			return rtfTemplate;
		}

		public static string GetEmbeddedTaskTemplate()
		{
			return GetEmbeddedFile("TaskBodyTemplate.rtf");
		}

		private static string GetEmbeddedFile(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = typeof (TaskSync).Namespace + "." + fileName;
			var fileStream = assembly.GetManifestResourceStream(resourceName);
			if (fileStream == null)
				throw new System.Exception("Could not find resource with name '" + resourceName + "'");
			var sr = new StreamReader(fileStream);
			return sr.ReadToEnd();
		}
	}
}