using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using YTech.Fogbugz;

namespace UnitTests
{
	[TestClass]
	public class CaseTests
	{
		[TestMethod]
		public void ParseCaseProperties()
		{
			var casesXml = TestUtilities.GetEmbeddedSampleXml("Cases.xml");
			var cases = Case.ParseCasesXml(casesXml).ToList();

			Assert.AreEqual(80464, cases[0].CaseId);
			Assert.AreEqual("Fake Case for Testing 2", cases[0].Subject);
			Assert.AreEqual(null, cases[0].Due);
			Assert.AreEqual(3, cases[0].Priority);
			Assert.AreEqual(DateTime.Parse("2012-07-30 19:18:16").ToLocalTime(), cases[0].Opened);
			Assert.AreEqual(DateTime.Parse("2013-01-07 03:19:53").ToLocalTime(), cases[0].LastUpdated);
		}

		[TestMethod]
		public void ParseDueDateWhenSuppliedProperties()
		{
			var casesXml = TestUtilities.GetEmbeddedSampleXml("CasesWithDueDates.xml");
			var cases = Case.ParseCasesXml(casesXml).ToList();

			Assert.AreEqual(DateTime.Parse("2012-07-30 20:18:16").ToLocalTime(), cases[0].Due);
		}

		[TestMethod]
		public void PercentComplete()
		{
			var casesXml = TestUtilities.GetEmbeddedSampleXml("Cases.xml");
			var cases = Case.ParseCasesXml(casesXml).ToList();

			Assert.AreEqual(30, cases[0].PercentComplete);
		}

		[TestMethod]
		public void InvalidPercentComplete()
		{
			var c = new Case();
			c.AddTag("xx%_Complete");

			Assert.AreEqual(0, c.PercentComplete);
		}

		[TestMethod]
		public void LowerCasePercentCompleteTag()
		{
			var c = new Case();
			c.AddTag("10%_complete");

			Assert.AreEqual(10, c.PercentComplete);
		}

		[TestMethod]
		public void NegativeCasePercentCompleteTag()
		{
			var c = new Case();
			c.AddTag("-1%_complete");

			Assert.AreEqual(0, c.PercentComplete);
		}

		[TestMethod]
		public void TagsUpdatedNewCase()
		{
			var c = new Case();
			Assert.IsFalse(c.TagsUpdated);
		}

		[TestMethod]
		public void TagsUpdatedWhenChanged()
		{
			var c = new Case();
			c.AddTag("boom");
			Assert.IsTrue(c.TagsUpdated);
		}

		[TestMethod]
		public void TagsUpdatedFalseWhenUnchanged()
		{
			var c = new Case();
			c.AddTag("boom");
			c.ResetUpdateFlags();
			c.AddTag("boom");
			Assert.IsFalse(c.TagsUpdated);
		}

		[TestMethod]
		public void GetTags()
		{
			var c = new Case();
			c.AddTag("boom");
			c.AddTag("boom2");

			var tags = c.GetTags().ToList();

			Assert.AreEqual(2, tags.Count());
			Assert.AreEqual("boom", tags[0]);
			Assert.AreEqual("boom2", tags[1]);
		}

		[TestMethod]
		public void SetPercentComplete()
		{
			var c = new Case();
			c.PercentComplete = 11;
			Assert.AreEqual(11, c.PercentComplete);
		}

		[TestMethod]
		public void SetPercentCompleteMultiple()
		{
			var c = new Case();
			c.PercentComplete = 11;
			c.PercentComplete = 12;
			Assert.AreEqual(12, c.PercentComplete);
		}

		[TestMethod]
		public void SetPercentCompleteUpdateFlag()
		{
			var c = new Case();
			c.PercentComplete = 11;
			Assert.IsTrue(c.TagsUpdated);
		}

		[TestMethod]
		public void SetPercentComplete100()
		{
			var c = new Case();
			c.PercentComplete = 100;
			Assert.AreEqual(100, c.PercentComplete);
		}

		[TestMethod]
		public void SetPercentComplete0()
		{
			var c = new Case();
			c.PercentComplete = 0;
			Assert.AreEqual(0, c.PercentComplete);
		}

		[TestMethod, ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SetPercentCompleteNegativeInvalid()
		{
			var c = new Case();
			c.PercentComplete = -1;
		}

		[TestMethod, ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void SetPercentCompleteAbove100Invalid()
		{
			var c = new Case();
			c.PercentComplete = 101;
		}

		[TestMethod]
		public void SubjectGetSet()
		{
			var c = new Case();
			c.Subject = "test";
			Assert.AreEqual("test", c.Subject);
		}

		[TestMethod]
		public void SubjectGetSetNull()
		{
			var c = new Case();
			c.Subject = null;
			Assert.AreEqual("", c.Subject);
		}

		[TestMethod]
		public void SubjectGetSetRemoveCaseNumber()
		{
			var c = new Case();
			c.Subject = "This is a test case (Case 80123)";
			Assert.AreEqual("This is a test case", c.Subject);
		}

		[TestMethod]
		public void SubjectUpdated()
		{
			var c = new Case();
			c.Subject = "test";
			c.ResetUpdateFlags();
			c.Subject = "test2";
			Assert.IsTrue(c.SubjectUpdated);
		}

		[TestMethod]
		public void GetTitleWithCaseToken()
		{
			var c = new Case();
			c.Subject = "title";
			c.CaseId = 123;
			Assert.AreEqual("title (Case 123)", c.GetTitleWithCaseToken());
		}

		[TestMethod]
		public void GetTitleWithCaseToken_NoSubject()
		{
				var c = new Case();
			c.CaseId = 123;
				Assert.AreEqual("(Case 123)", c.GetTitleWithCaseToken());
		}

		[TestMethod]
		public void DueGetSet()
		{
			var c = new Case();
			c.Due = DateTime.Parse("2013-1-1 10:00am");
			Assert.AreEqual(DateTime.Parse("2013-1-1 10:00am"), c.Due);
		}

		[TestMethod]
		public void DueUpdated()
		{
			var c = new Case();
			c.Due = DateTime.Parse("2013-1-1 10:00am");
			c.ResetUpdateFlags();
			c.Due = DateTime.Parse("2013-1-1 11:00am");
			Assert.IsTrue(c.DueUpdated);
		}

		[TestMethod]
		public void DueNotUpdatedWithSameValue()
		{
			var c = new Case();
			c.Due = DateTime.Parse("2013-1-1 10:00am");
			c.ResetUpdateFlags();
			c.Due = DateTime.Parse("2013-1-1 10:00am");
			Assert.IsFalse(c.DueUpdated);
		}

		[TestMethod]
		public void RemoveSubjectCaseToken()
		{
			Assert.AreEqual("This is a test", Case.RemoveSubjectCaseToken("This is a test (Case 1234)"));
		}

		[TestMethod]
		public void ResolvedGetSet()
		{
			var c = new Case();
			c.Resolved = true;
			Assert.AreEqual(true, c.Resolved);
		}

		[TestMethod]
		public void ResolvedGetSetUpdated()
		{
				var c = new Case();
				c.Resolved = true;
				Assert.AreEqual(true, c.ResolvedUpdated);
		}

		[TestMethod]
		public void ResolvedGetSetNotUpdated()
		{
				var c = new Case();
				c.Resolved = true;
				c.ResetUpdateFlags();
				Assert.AreEqual(false, c.ResolvedUpdated);
		}

			[TestMethod]
			public void ToStringValid()
			{
				var c = new Case();
				c.CaseId = 123;
					Assert.AreEqual("CaseId: 123", c.ToString());
			}

		//[TestMethod]
		//public void TestMethod1()
		//{
		//    var fogbugzTokenResponseXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><response><token><![CDATA[g3pjra83gjk3ptggb76972b05dsldf]]></token></response>";

		//    var token = Fogbugz.ParseFogbugzToken(fogbugzTokenResponseXml);
		//    Assert.AreEqual("g3pjra83gjk3ptggb76972b05dsldf", token);

		//}

		//[TestMethod]
		//public void ParseCasesXml()
		//{
		//    const string CasesXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><response><cases count=\"11\"><case ixBug=\"81918\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"81851\" operations=\"edit,assign,resolve,reply,forward,remind\"></case><case ixBug=\"82041\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"82002\" operations=\"edit,spam,assign,resolve,reply,forward,remind\"></case><case ixBug=\"82037\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"81993\" operations=\"edit,spam,assign,resolve,reply,forward,remind\"></case><case ixBug=\"81936\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"81869\" operations=\"edit,assign,resolve,reply,forward,remind\"></case><case ixBug=\"81694\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"81632\" operations=\"edit,assign,resolve,email,remind\"></case><case ixBug=\"81557\" operations=\"edit,assign,resolve,email,remind\"></case></cases></response>";

		//    var cases = Case.ParseCasesXml(CasesXml).ToList();
		//    Assert.AreEqual(11, cases.Count);
		//    Assert.AreEqual(81918, cases[0].CaseId);
		//}

		//[TestMethod]
		//public void ParseTags()
		//{
		//    var casesXml = TestUtilities.GetEmbeddedSampleXml("Cases.xml");

		//    var cases = Case.ParseCasesXml(casesXml).ToList();
		//    Assert.AreEqual(81918, cases[0].CaseId);
		//}
	}
}