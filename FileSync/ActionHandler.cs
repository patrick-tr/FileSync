using System;
using System.IO;
using System.IO.Compression;

namespace FileSync
{
    class ActionHandler
    {
        /// <summary>
        /// Default constructor for class
        /// </summary>
        public ActionHandler()
        {

        }

        /// <summary>
        /// Creates new folder in destination directory and adds timestamp to name
        /// </summary>
        /// <param name="destDir">Destination directory where the files should be backed up</param>
        /// <param name="sourcDir">Source directory. Specifies which directory to back up</param>
        /// <param name="archive">Specifies if backup shuld be packt to .zip file</param>
        /// <returns>true if backup was sucessfull</returns>
        public bool Backup(string destDir, string sourcDir, bool archive)
        {
            try
            {
                if (archive)
                {

                    //Pack directory to zip
                    if (!Archive(sourcDir, destDir))
                    {
                        return false;
                    }
                }
                else
                {
                    //Copy Directory to new location
                    if (!DirectoryCopy(sourcDir, destDir))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Replace folder in directory if it exists
        /// </summary>
        /// <param name="destDir"></param>
        /// <param name="sourceDir"></param>
        /// <returns>True if sucessful</returns>
        public bool Replace(string destDir, string sourceDir)
        {
            DirectoryInfo dDir = new DirectoryInfo(destDir);
            DirectoryInfo sDir = new DirectoryInfo(sourceDir);

            if (!sDir.Exists)
            {
                return false;
            }
            if (!dDir.Exists)
            {
                Directory.CreateDirectory(dDir.FullName);  
            }

            if (dDir.Name.Equals(sDir.Name))
            {
                //Delete old dir recursive
                Directory.Delete(Path.Combine(dDir.FullName, sDir.Name), true);
            }
            else
            {
                DirectoryInfo[] dirs = dDir.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    if (dir.Name.Equals(sDir.Name))
                    {
                        Directory.Delete(Path.Combine(dDir.FullName, sDir.Name), true);
                    }
                }
            }

            DirectoryCopy(sDir.FullName, dDir.FullName, false);

            return true;
        }

        /// <summary>
        /// Archives the specified dir
        /// </summary>
        /// <param name="dirToArchive">path to dir</param>
        /// <returns>True if successful</returns>
        private bool Archive(string sourcDir, string destDir)
        {
            DirectoryInfo sDir = new DirectoryInfo(sourcDir);
            DirectoryInfo dDir = new DirectoryInfo(destDir);

            //Check if source exists
            if (!sDir.Exists)
            {
                return false;
            }
            //check if destination exists
            if (!dDir.Exists)
            {
                Directory.CreateDirectory(dDir.FullName);
            }

            //Create new dastination name for zip file
            string destName = dDir.FullName + "\\" + sDir.Name + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".zip";
            try
            {
                //Create new zip file
                ZipFile.CreateFromDirectory(sourcDir, destName);
            }
            catch(IOException ex)
            {
                throw ex;
            }

            return true;
        }

        /// <summary>
        /// Copys the sourc directory to destination directory
        /// </summary>
        /// <param name="sourcDir">Source directory</param>
        /// <param name="destDir">Destination directory</param>
        /// <returns>True if sucessful</returns>
        private bool DirectoryCopy(string sourcDir, string destDir, bool addDate = true)
        {
            DirectoryInfo dDir = new DirectoryInfo(destDir);
            DirectoryInfo sDir = new DirectoryInfo(sourcDir);

            //Check if destination dir exists
            if (!dDir.Exists)
            {
                Directory.CreateDirectory(dDir.FullName);
            }

            //set new destination path
            if (addDate)
            {
                dDir = new DirectoryInfo(Path.Combine(dDir.FullName, sDir.Name) + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss"));
            }
            else
            {
                dDir = new DirectoryInfo(Path.Combine(dDir.FullName, sDir.Name));
            }

            if (!dDir.Exists)
            {
                Directory.CreateDirectory(dDir.FullName);
            }

            //Get files and copie them
            FileInfo[] files = sDir.GetFiles();
            foreach(FileInfo file in files)
            {
                file.CopyTo(Path.Combine(dDir.FullName, file.Name));
            }

            //Get subdirs to copie them
            DirectoryInfo[] subDirs = sDir.GetDirectories();
            foreach(DirectoryInfo subDir in subDirs)
            {
                CopySubDirs(subDir, dDir);
            }

            return true;
        }

        /// <summary>
        /// Recursivly Copies Files in a subdirectory and Creats further subdirectorys
        /// </summary>
        /// <param name="sourceDir">Source of files and dirs</param>
        /// <param name="destDir">New location for files</param>
        private void CopySubDirs(DirectoryInfo sourceDir, DirectoryInfo destDir)
        {
            destDir = new DirectoryInfo(Path.Combine(destDir.FullName, sourceDir.Name));

            //Creates subdir if not exists
            if (!destDir.Exists)
            {
                Directory.CreateDirectory(destDir.FullName);
            }

            //Copie Files
            FileInfo[] files = sourceDir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(destDir.FullName, file.Name));
            }

            //Do action for each subdir
            DirectoryInfo[] subDirs = sourceDir.GetDirectories();
            foreach(DirectoryInfo subDir in subDirs)
            {
                CopySubDirs(subDir, destDir);
            }
        }
    }
}
