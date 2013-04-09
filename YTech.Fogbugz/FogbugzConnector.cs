using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Xml;
using System.Globalization;

namespace YTech.Fogbugz
{
	public class FogbugzConnector : IFogbugzConnector
	{
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly string _url;
		private readonly string _user;
		private readonly string _pass;
		private readonly string _currentUserEmail;

		private string _apiKey;
		private FogbugzUser _fogbugzUser;

		public FogbugzConnector(string url, string apiUser, string apiPassword, string currentUserEmail)
		{
			_url = url;
			_user = apiUser;
			_pass = apiPassword;
			_currentUserEmail = currentUserEmail;
		}

		private string MakeFogbugzRequest(string cmd, Dictionary<string, string> parameters)
		{
			if (_apiKey != null)
				parameters.Add("token", _apiKey);

			var reqPath = string.Format("{0}/api.asp?cmd={1}", _url, cmd);
			foreach (var parameter in parameters)
			{
				reqPath += string.Format("&{0}={1}", parameter.Key, Uri.EscapeDataString(parameter.Value));
			}

			string responseString = null;

			var req = (HttpWebRequest) WebRequest.Create(reqPath);
			using (var res = req.GetResponse())
			{
				using (var responseStream = res.GetResponseStream())
				{
					if (responseStream != null)
					{
						using (var sr = new StreamReader(responseStream))
						{
							responseString = sr.ReadToEnd();
						}
					}
				}
			}

			return responseString;
		}

		private void LoadApiKey()
		{
			if (_apiKey != null)
				return;

			var logonParameters = new Dictionary<string, string>();
			logonParameters.Add("email", _user);
			logonParameters.Add("password", _pass);

			var xmlString = MakeFogbugzRequest("logon", logonParameters);
			_apiKey = ParseFogbugzToken(xmlString);
		}

		public IEnumerable<Case> LoadCases(string assigneeUserName)
		{
			LoadApiKey();

			var parameters = new Dictionary<string, string>();
			parameters.Add("q", string.Format("assignedto:\"{0}\"", assigneeUserName));
			parameters.Add("cols", "sTitle,dtLastUpdated,dtDue,ixPriority,dtOpened,tags");

			var xmlString = MakeFogbugzRequest("search", parameters);
			var cases = Case.ParseCasesXml(xmlString);

			return cases;
		}

		private void UpdateFogbugzUser()
		{
			if (_fogbugzUser != null)
				return;

			LoadApiKey();

			var parameters = new Dictionary<string, string>();

			var xmlString = MakeFogbugzRequest("listPeople", parameters);
			var users = FogbugzUser.ParseUsersXml(xmlString);

			_fogbugzUser = users.SingleOrDefault(x => x.Email == _currentUserEmail);

			if (_fogbugzUser == null)
				throw new Exception("Could not find a user with the email " + _currentUserEmail);
		}

		public FogbugzUser GetCurrentFogbugzUser()
		{
			UpdateFogbugzUser();

			return _fogbugzUser;
		}

		public static string ParseFogbugzToken(string xml)
		{
			//<?xml version="1.0" encoding="UTF-8"?><response><token><![CDATA[g3pjra83gjk3ptggb76972b05dsldf]]></token></response>
			var document = new XmlDocument();
			document.LoadXml(xml);

			var nav = document.CreateNavigator();
			var tokenNode = nav.SelectSingleNode("/response/token");
			if (tokenNode == null)
				throw new Exception("Could not find /response/token node in Fogbugz XML response");

			var token = tokenNode.InnerXml;

			return token;
		}

		public void SaveCase(Case fogbugzCase)
		{
			var parameters = new Dictionary<string, string>();

			if (fogbugzCase.TagsUpdated)
				parameters.Add("sTags", string.Join(",", fogbugzCase.GetTags().ToArray()));
			if (fogbugzCase.SubjectUpdated)
				parameters.Add("sTitle", fogbugzCase.Subject);
			if (fogbugzCase.DueUpdated && fogbugzCase.Due != null)
				parameters.Add("dtDue", fogbugzCase.Due.Value.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
			
			if (parameters.Count > 0)
			{
				UpdateFogbugzUser();

				parameters.Add("ixBug", fogbugzCase.CaseId.ToString());
				parameters.Add("ixPersonEditedBy", _fogbugzUser.UserId.ToString());

				var fogbugzResponse = MakeFogbugzRequest("edit", parameters);

				Log.DebugFormat("Response received from Fogbugz: {0}", fogbugzResponse);
			}

			if (fogbugzCase.ResolvedUpdated && fogbugzCase.Resolved)
			{
					parameters.Add("ixBug", fogbugzCase.CaseId.ToString());
					parameters.Add("ixPersonEditedBy", _fogbugzUser.UserId.ToString());

					var fogbugzResponse = MakeFogbugzRequest("resolve", parameters);

					Log.DebugFormat("Response received from Fogbugz: {0}", fogbugzResponse);
			}
		}
	}
}