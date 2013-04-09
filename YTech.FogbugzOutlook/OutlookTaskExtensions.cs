using System;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace YTech.FogbugzOutlook
{
	public static class OutlookTaskExtensions
	{
			private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
		System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static int? GetFogbugzCaseId(this TaskItem task)
		{
			return GetProperty<int>(task, TaskSync.CaseIdUserPropertyName);
		}

		public static void SetFogbugzCaseId(this TaskItem task, int caseId)
		{
			SetProperty<int>(task, TaskSync.CaseIdUserPropertyName, caseId);
		}

		public static DateTime? GetLastSync(this TaskItem task)
		{
			return GetProperty<DateTime>(task, TaskSync.LastSyncPropertyName);
		}

		public static void SetLastSync(this TaskItem task, DateTime? syncTime)
		{
			SetProperty(task, TaskSync.LastSyncPropertyName, syncTime);
		}

		public static void SetLastModificationDateForTesting(this TaskItem task, DateTime lastModified)
		{
			SetProperty<DateTime>(task, "LastModified", lastModified);
		}

		public static DateTime? GetLastModifiedDate(this TaskItem task)
		{
			var readValue = GetProperty<DateTime>(task, "LastModified");
			if (readValue != null)
				return readValue.Value;

			return task.LastModificationTime == OutlookConnector.EmptyDate ? (DateTime?) null : task.LastModificationTime;
		}

		public static T? GetProperty<T>(TaskItem task, string propertyName) where T : struct
		{
			try
			{
				var idProp = task.UserProperties.Find(propertyName);

				if (idProp == null)
					return null;

				return idProp.Value;
			}
			catch (Exception ex)
			{
				if (propertyName != null)
					Log.Error("Error reading property '" + propertyName + "'", ex);
				throw;
			}
		}

		public static void SetProperty<T>(TaskItem task, string propertyName, T? value) where T : struct
		{
			var propertyType = GetUserPropertyType(typeof (T));

			var idProp = task.UserProperties.Find(propertyName);

			//Check if we're supposed to remove the property
			if (idProp != null && value == null)
				idProp = null; //does this remove the property?

			if (idProp == null)
				idProp = task.UserProperties.Add(propertyName, propertyType);

			idProp.Value = value;
		}

		public static OlUserPropertyType GetUserPropertyType(Type t)
		{
			if (t == typeof (int))
				return OlUserPropertyType.olInteger;
			if (t == typeof (string))
				return OlUserPropertyType.olText;
			if (t == typeof (DateTime))
				return OlUserPropertyType.olDateTime;

			throw new ArgumentException("Unknown property type of outlook task");
		}
	}
}