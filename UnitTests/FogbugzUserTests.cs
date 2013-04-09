using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YTech.Fogbugz;

namespace UnitTests
{
    [TestClass]
    public class FogbugzUserTests
    {
        [TestMethod]
        public void TestParsePeopleXml()
        {
            var peopleXml = GetEmbeddedSampleXml("People.xml");
            var users = FogbugzUser.ParseUsersXml(peopleXml).ToList();
            Assert.AreEqual(1, users.Count);
            Assert.AreEqual("Old MacDonald", users[0].FullName);
            Assert.AreEqual(11, users[0].UserId);
        }

        public static string GetEmbeddedSampleXml(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileStream = assembly.GetManifestResourceStream(typeof(FogbugzUserTests).Namespace + ".SampleFogbugzXml." + fileName);
            var sr = new StreamReader(fileStream);
            return sr.ReadToEnd();
        }
    }
}
