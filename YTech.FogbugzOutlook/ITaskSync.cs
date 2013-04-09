using Microsoft.Office.Interop.Outlook;
using YTech.Fogbugz;

namespace YTech.FogbugzOutlook
{
    public interface ITaskSync
    {
        void SyncTask(Case fogbugzCase, TaskItem outlookTask);
    }
}
