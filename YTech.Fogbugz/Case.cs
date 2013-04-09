using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace YTech.Fogbugz
{
	public class Case
	{
		private string _subject;
		private DateTime? _due;
		private bool _resolved;

		public int CaseId { get; set; }

		public string Subject
		{
			get { return _subject; }
			set
			{
				if (value == null)
				{
					_subject = "";
					return;
				}

				var subjectWithoutCaseToken = Case.RemoveSubjectCaseToken(value);
				if (_subject != subjectWithoutCaseToken)
				{
					_subject = subjectWithoutCaseToken;
					SubjectUpdated = true;
				}
			}
		}

		public DateTime LastUpdated { get; set; }

		public DateTime? Due
		{
			get { return _due; }
			set
			{
				if (_due != value)
				{
					_due = value;
					DueUpdated = true;
				}
			}
		}

		public int Priority { get; set; }
		public DateTime Opened { get; set; }

		private readonly List<string> _tags;

		//Update indicators
		public bool TagsUpdated { get; private set; }
		public bool SubjectUpdated { get; private set; }
		public bool DueUpdated { get; private set; }
		public bool ResolvedUpdated { get; private set; }

		public Case()
		{
			_tags = new List<string>();
		}

		public bool Resolved
		{
				get { return _resolved; }
				set
				{
					if (_resolved != value)
					{
						_resolved = value;
						ResolvedUpdated = true;
					}
				}
		}

		public int PercentComplete
		{
			set
			{
				if (value < 0 || value > 100)
					throw new ArgumentOutOfRangeException("% complete for a case must be between 0 and 100");

				//This is the easiest way to confirm the percent hasn't changed
				if (PercentComplete == value)
					return;

				var tagIndex = _tags.FindIndex(x => x.EndsWith("%_Complete", StringComparison.CurrentCultureIgnoreCase));

				var tagValue = string.Format("{0}%_Complete", value);

				if (tagIndex == -1)
					_tags.Add(tagValue);
				else
					_tags[tagIndex] = tagValue;

				TagsUpdated = true;
			}
			get
			{
				foreach (var tag in _tags)
				{
					if (tag.EndsWith("%_Complete", StringComparison.CurrentCultureIgnoreCase))
					{
						int percent;
						var success = int.TryParse(tag.Substring(0, tag.IndexOf('%')), out percent);
						if (percent < 0 || percent > 100)
							percent = 0;
						return success ? percent : 0;
					}
				}

				return 0;
			}
		}

		public string GetTitleWithCaseToken()
		{
			if (string.IsNullOrWhiteSpace(Subject))
				return string.Format("(Case {0})", CaseId);

			return string.Format("{0} (Case {1})", Subject, CaseId);
		}

		public void AddTag(string tag)
		{
			if (_tags.Contains(tag))
				return;

			_tags.Add(tag);
			TagsUpdated = true;
		}

		public IEnumerable<string> GetTags()
		{
			//I don't want to return the original list, so we'll obfuscate it
			return _tags.Select(x => x);
		}

		public void ResetUpdateFlags()
		{
			TagsUpdated = false;
			SubjectUpdated = false;
			DueUpdated = false;
			ResolvedUpdated = false;
		}

		public static string RemoveSubjectCaseToken(string subject)
		{
			return Regex.Replace(subject, @" \(Case \d{1,10}\)", "");
		}

		public override string ToString()
		{
			return string.Format("CaseId: {0}", CaseId);
		}

		public static IEnumerable<Case> ParseCasesXml(string xml)
		{
			using (var sr = new StringReader(xml))
			{
				var x = XmlDynamo.Load(sr);

				var cases = x.cases.Elements("case");
				var caseList = new List<Case>();

				foreach (var currCase in cases)
				{
					var c = new Case();
					c.CaseId = int.Parse(currCase.ixBug);
					c._subject = currCase.sTitle;
					c.LastUpdated = DateTime.Parse(currCase.dtLastUpdated);
					if (currCase.dtDue != "")
						c._due = DateTime.Parse(currCase.dtDue);
					c.Priority = int.Parse(currCase.ixPriority);
					c.Opened = DateTime.Parse(currCase.dtOpened);

					if ((string) currCase.tags != null)
					{
						var tags = currCase.tags.Elements("tag");
						c._tags.Clear();
						foreach (string tag in tags)
						{
							c._tags.Add(tag);
						}
					}

					c.ResetUpdateFlags();

					caseList.Add(c);
				}

				return caseList;
			}
		}
	}
}