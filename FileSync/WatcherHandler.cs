using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Configuration;
using System.Reflection;

namespace FileSync
{
    class WatcherHandler
    {
        private string fileNameXML { get; set; }

        private List<CustomFolderSettings> folderList { get; set; }

        private List<FileSystemWatcher> fileSystemWatchers { get; set; }

        private EventLog EventLog { get; set; }

        public bool IsPaused { get; set; }

        /// <summary>
        /// Standard constructor for class
        /// </summary>
        public WatcherHandler()
        {
            //Creates EventLog Instance for logging to Windows Application Log
            this.EventLog = new EventLog("Application");
            CustomLogEvent("FileSync sucessfuly started!", EventLogEntryType.Information);
        }

        /// <summary>
        /// Reads in a XML File and populates a list of <CustomFolderSettings>
        /// </summary>
        /// <returns>List Size</returns>
        public int PopulateListFileSystemWatchers()
        {
            //Get the XML file name from App.config file
            this.fileNameXML = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.fileNameXML = Path.Combine(this.fileNameXML, ConfigurationManager.AppSettings["XmlConfigName"]);
            CustomLogEvent(fileNameXML, EventLogEntryType.Warning);
            //Create an instance of XMLSerializer
            XmlSerializer desiralizer = new XmlSerializer(typeof(List<CustomFolderSettings>));
            TextReader reader = new StreamReader(fileNameXML);
            object obj = desiralizer.Deserialize(reader);
            //Close the Text Reader
            reader.Close();

            this.folderList = obj as List<CustomFolderSettings>;

            return folderList.Count;
        }

        /// <summary>
        /// Start the file system watcher for each of the elements
        /// </summary>
        public void StartFileSystemWatcher()
        {
            //Bool for checking if vent was added
            bool eventAdded = false;
            //Creates new instancen of of the list
            this.fileSystemWatchers = new List<FileSystemWatcher>();
            //Loop the list to process each of the folder specifications found
            foreach (CustomFolderSettings customFolder in folderList)
            {
                DirectoryInfo dir = new DirectoryInfo(customFolder.FolderPath);
                //Checks wheter the folder is enabled and
                //also the directory is a valid location
                if (customFolder.FolderEnabled && dir.Exists)
                {
                    //Create new Instance of FileSystemWatcher
                    FileSystemWatcher fileSwatcher = new FileSystemWatcher();
                    //Sets the filter
                    fileSwatcher.Filter = customFolder.FolderFilter;
                    //Sets the folder location
                    fileSwatcher.Path = customFolder.FolderPath;
                    //Sets if Subdirectories should be included
                    fileSwatcher.IncludeSubdirectories = customFolder.FolderIncludeSub;
                    //Sets the action to be executed
                    StringBuilder actionToExec = new StringBuilder(customFolder.Action);
                    //List of Arguments
                    StringBuilder actionArguments = new StringBuilder(customFolder.ActionArguments);
                    //Subscribe to notify filters
                    fileSwatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    //Associoate the event that is specified in arguments that will be triggered when a new file
                    //is added to the monitored folder, using a lamda expression
                    if (actionArguments.ToString().Contains("-create"))
                    {
                        fileSwatcher.Created += (senderObj, fileSysArgs) =>
                            fileSwatcher_Created(senderObj, fileSysArgs, actionToExec.ToString(),
                            actionArguments.ToString());

                        eventAdded = true;
                    }

                    if (actionArguments.ToString().Contains("-change"))
                    {
                        fileSwatcher.Changed += (senderObj, fileSysArgs) =>
                            fileSwatcher_Changed(senderObj, fileSysArgs, actionToExec.ToString(),
                            actionArguments.ToString());

                        eventAdded = true;
                    }

                    if (actionArguments.ToString().Contains("-rename"))
                    {
                        fileSwatcher.Renamed += (senderObj, fileSysArgs) =>
                            fileSwatcher_Renamed(senderObj, fileSysArgs, actionToExec.ToString(),
                            actionArguments.ToString());

                        eventAdded = true;
                    }

                    if (actionArguments.ToString().Contains("-delete"))
                    {
                        fileSwatcher.Deleted += (senderObj, fileSysArgs) =>
                            fileSwatcher_Deleted(senderObj, fileSysArgs, actionToExec.ToString(),
                            actionArguments.ToString());

                        eventAdded = true;
                    }

                    if (!eventAdded)
                    {
                        CustomLogEvent(String.Format("No Event specified for Directory {0}!", customFolder.FolderPath), EventLogEntryType.Warning);
                    }

                    //Begin watching
                    fileSwatcher.EnableRaisingEvents = true;
                    //add The System Watcher to the List
                    this.fileSystemWatchers.Add(fileSwatcher);
                    //Record a log entry into Windows event log
                    CustomLogEvent(String.Format(
                        "Starting to monitor files with extension ({0}) in the folder ({1})",
                        fileSwatcher.Filter, fileSwatcher.Path), EventLogEntryType.Information);
                }
            }
        }

        /// <summary>
        /// Stop the Listening on the elements
        /// </summary>
        public void StopFileSystemWatchers()
        {
            if (this.fileSystemWatchers != null)
            {
                foreach (FileSystemWatcher fsw in this.fileSystemWatchers)
                {
                    //Stop listening
                    fsw.EnableRaisingEvents = false;
                    //Dispose the object
                    fsw.Dispose();
                }

                //Clear the List
                this.fileSystemWatchers.Clear();
            }
        }


        /// <summary>
        /// Writes a Entry to Application log of Windows
        /// </summary>
        /// <param name="msg">Message to write to Application log</param>
        /// <param name="type">Type of Event - EventLogEntryType</param>
        public void CustomLogEvent(string msg, EventLogEntryType type)
        {
            string source = "FileSync";

            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, "Application");
            }

            EventLog.WriteEntry(source, msg);
        }

        /// <summary>
        /// This event is triggered when a file with the specified
        /// extension is created on the monitired folder
        /// </summary>
        /// <param name="sender">Object raising the event</param>
        /// <param name="e">List of Arguments - FileSystemEventArgs</param>
        /// <param name="action_Exec">The action to be executed upon detection a change in the File system</param>
        /// <param name="action_Args">Arguments to be passed to the executable (action)</param>
        void fileSwatcher_Created(object sender, FileSystemEventArgs e,
            string action_Exec, string action_Args)
        {
            if (!this.IsPaused)
            {
                CheckIfActionSet(e.FullPath, action_Exec);
                ExecAction(e.FullPath, action_Exec, action_Args);
            }
        }

        void fileSwatcher_Changed(object sender, FileSystemEventArgs e,
            string action_Exec, string action_Args)
        {
            if (!this.IsPaused)
            {
                CheckIfActionSet(e.FullPath, action_Exec);
                ExecAction(e.FullPath, action_Exec, action_Args);
            }
        }

        void fileSwatcher_Renamed(object sender, FileSystemEventArgs e,
            string action_Exec, string action_Args)
        {
            if (!this.IsPaused)
            {
                CheckIfActionSet(e.FullPath, action_Exec);
                ExecAction(e.FullPath, action_Exec, action_Args);
            }
        }

        void fileSwatcher_Deleted(object sender, FileSystemEventArgs e,
            string action_Exec, string action_Args)
        {
            if (!this.IsPaused)
            {
                CheckIfActionSet(e.FullPath, action_Exec);
                ExecAction(e.FullPath, action_Exec, action_Args);
            }
        }

        /// <summary>
        /// Checks if for the specific event a action is set
        /// </summary>
        /// <param name="dir">Dir on which is listened</param>
        /// <param name="args">arguments from config file</param>
        private void CheckIfActionSet(string dir, string action)
        {
            if (!action.Contains("-backup") && !action.Contains("-replace"))
            {
                CustomLogEvent(String.Format("No action for directory {0} in config file specified! Valid actions are: -backup, -replace", dir), EventLogEntryType.Warning);
            }
        }

        /// <summary>
        /// Function to execute the setet action
        /// </summary>
        /// <param name="dir">Dir on which the event occured</param>
        /// <param name="action">String with action name</param>
        /// <param name="actionArgs">String with action args</param>
        private void ExecAction(string dir, string action, string actionArgs)
        {
            var actionHandler = new ActionHandler();
            FileInfo fi = new FileInfo(dir);

            dir = fi.DirectoryName;

            if (action.Contains("-backup"))
            {
                string destDir = FindParamInArgs(actionArgs ,"dest=");
                bool archive = FindParamInArgs(actionArgs, "archive=") == "true" ? true : false;
                try
                {
                    if (!String.IsNullOrEmpty(destDir))
                    {
                        actionHandler.Backup(destDir, dir, archive);
                    }
                    else
                    {
                        CustomLogEvent(String.Format("No destination dir set for action Backup on Directory {0}", dir), EventLogEntryType.Warning);
                    }
                }
                catch(Exception ex)
                {
                    CustomLogEvent(String.Format("Cannot copy directory. Maybe there is a File open for Reading/Writing, {0}", ex.Message), EventLogEntryType.Warning);
                }
            }
            else if (action.Contains("-replace"))
            {
                string destDir = FindParamInArgs(actionArgs , "dest=");

                if (!String.IsNullOrEmpty(destDir))
                {
                    actionHandler.Replace(destDir, dir);
                }
                else
                {
                    CustomLogEvent(String.Format("No destination dir set for action Replace on Directory {0}", dir), EventLogEntryType.Warning);
                }
            }
        }

        /// <summary>
        /// Finds the Value of the given argument if it exits in argument string
        /// </summary>
        /// <param name="args">String of arguments to search in</param>
        /// <param name="param">Parameter name to search for</param>
        /// <returns>If parameter fount the value of it otherwise an empty string</returns>
        private string FindParamInArgs(string args, string param)
        {
            if (args.Contains(param))
            {
                string[] subStrings = args.Split(' ');
                foreach(string sub in subStrings)
                {
                    if (sub.Contains(param))
                    {
                        return sub.Replace(param, "");
                    }
                }
            }

            CustomLogEvent("Parameter for action not found!", EventLogEntryType.Warning);
            return String.Empty;
        }
    }
}
