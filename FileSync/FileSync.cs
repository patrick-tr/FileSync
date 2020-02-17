using System.Diagnostics;
using System.ServiceProcess;

namespace FileSync
{
    public partial class FileSync : ServiceBase
    {
        //Object of handler class
        private WatcherHandler WatcherHandler { get; set; }

        public FileSync()
        {
            InitializeComponent();
            this.WatcherHandler = new WatcherHandler();
            this.WatcherHandler.IsPaused = false;
        }

        protected override void OnStart(string[] args)
        {
            //Initialize the list of FileSystemWatchers based on xml configuration file
            if(this.WatcherHandler.PopulateListFileSystemWatchers() > 0)
            {
                this.WatcherHandler.StartFileSystemWatcher();
                this.WatcherHandler.CustomLogEvent("File watching sucessfuly started...", EventLogEntryType.Information);
            }
        }

        protected override void OnPause()
        {
            this.WatcherHandler.IsPaused = true;
            this.WatcherHandler.CustomLogEvent("FileSync paused", EventLogEntryType.Information);
        }

        protected override void OnContinue()
        {
            this.WatcherHandler.IsPaused = false;
            this.WatcherHandler.CustomLogEvent("FileSync continued", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            this.WatcherHandler.StopFileSystemWatchers();
            this.WatcherHandler.CustomLogEvent("FileSync service stopped!", EventLogEntryType.Information);
        }
    }
}
