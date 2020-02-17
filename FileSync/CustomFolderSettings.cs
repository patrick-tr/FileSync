using System.Xml.Serialization;

namespace FileSync
{
    public class CustomFolderSettings
    {
        /// <summary>
        /// Unique identifieres of the combination File type/folder.
        /// </summary>
        [XmlAttribute]
        public string FolderId { get; set; }

        /// <summary>
        /// If TRUE: the file type of files and folder will be monitored
        /// </summary>
        [XmlElement]
        public bool FolderEnabled { get; set; }

        /// <summary>
        /// Discription of the type of files and folders location -
        /// Just for documentation purpose
        /// </summary>
        [XmlElement]
        public string FolderDescription { get; set; }

        /// <summary>
        /// Filter to select the type of files to be monitored
        /// (Examples: *.shp, *.*, Project00*.zip)
        /// </summary>
        [XmlElement]
        public string FolderFilter { get; set; }

        /// <summary>
        /// Full Path to be monitored
        /// (i.e.: D:\files\projects\shapes)
        /// </summary>
        [XmlElement]
        public string FolderPath { get; set; }

        /// <summary>
        /// If TRUE: the folder and its subfolders will be monitored
        /// </summary>
        [XmlElement]
        public bool FolderIncludeSub { get; set; }

        /// <summary>
        /// Specifies the command or action to be executed
        /// after event has raised
        /// </summary>
        [XmlElement]
        public string Action { get; set; }

        /// <summary>
        /// List of arguments to be passed to the executable file
        /// </summary>
        [XmlElement]
        public string ActionArguments { get; set; }

        /// <summary>
        /// Default constructor of the class
        /// </summary>
        public CustomFolderSettings()
        {

        }
    }
}
