using System.Collections.Generic;

namespace YTech.Fogbugz
{
    public interface IFogbugzConnector
    {
        FogbugzUser GetCurrentFogbugzUser();
        IEnumerable<Case> LoadCases(string assigneeUserName);
        void SaveCase(Case fogbugzCase);
    }
}
