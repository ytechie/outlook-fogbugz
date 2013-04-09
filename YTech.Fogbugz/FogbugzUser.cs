using System.Collections.Generic;
using System.IO;

namespace YTech.Fogbugz
{
    public class FogbugzUser
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        public static IEnumerable<FogbugzUser> ParseUsersXml(string xml)
        {
            using (var sr = new StringReader(xml))
            {
                var x = XmlDynamo.Load(sr);

                var persons = x.people.Elements("person");
                var fogbugzUsers = new List<FogbugzUser>();
                foreach (var person in persons)
                {
                    var newUser = new FogbugzUser();
                    newUser.Email = person.sEmail;
                    newUser.FullName = person.sFullName;
                    newUser.UserId = int.Parse(person.ixPerson);

                    fogbugzUsers.Add(newUser);
                }

                return fogbugzUsers;
            }
        }
    }
}
