namespace gdi_framework
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.IO;

    internal class FileIORealtime
    {
        public bool ChangeDirectory(string Directory)
        {
            Environment.CurrentDirectory = Directory;
            return true;
        }

        public string Copy(string Target, string TargetPath)
        {
            if (!Directory.Exists(this.DecodeWildcards(TargetPath)))
            {
                return $"The target path '{TargetPath}' was not found";
            }
            if (File.Exists(Target))
            {
                File.Copy(Target, $"{TargetPath}\{Path.GetFileName(Target)}", true);
                return $"Copiled file '{Target}' to '{TargetPath}'";
            }
            return $"File not found '{Target}'";
        }

        public string DecodeWildcards(string Path) => 
            Path.Replace("%cwd%", Environment.CurrentDirectory).Replace("%appdata%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).Replace("%desktop%", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)).Replace("%documents%", Environment.GetFolderPath(Environment.SpecialFolder.Personal)).Replace("%music%", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)).Replace("%pictures%", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)).Replace("%videos%", Environment.GetFolderPath(Environment.SpecialFolder.Recent | Environment.SpecialFolder.Favorites)).Replace("%programfiles%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).Replace("%programfilesx86%", Environment.GetFolderPath(Environment.SpecialFolder.History | Environment.SpecialFolder.Recent)).Replace("%startmenu%", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)).Replace("%startup%", Environment.GetFolderPath(Environment.SpecialFolder.Startup)).Replace("%system%", Environment.GetFolderPath(Environment.SpecialFolder.System)).Replace("%systemx86%", Environment.GetFolderPath(Environment.SpecialFolder.Cookies | Environment.SpecialFolder.Recent)).Replace("%userprofile%", Environment.GetFolderPath(Environment.SpecialFolder.InternetCache | Environment.SpecialFolder.Recent)).Replace("%windows%", Environment.GetFolderPath((Environment.SpecialFolder) 0x24)).Replace("%c_appdata%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)).Replace("%c_desktop%", Environment.GetFolderPath(Environment.SpecialFolder.MyComputer | Environment.SpecialFolder.Recent)).Replace("%c_documents%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles | Environment.SpecialFolder.Recent)).Replace("%c_music%", Environment.GetFolderPath(Environment.SpecialFolder.System | Environment.SpecialFolder.DesktopDirectory)).Replace("%c_startmenu%", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures | Environment.SpecialFolder.DesktopDirectory)).Replace("%c_pictures%", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles | Environment.SpecialFolder.DesktopDirectory)).Replace("%c_programfiles%", Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles)).Replace("%c_programfilesx86%", Environment.GetFolderPath((Environment.SpecialFolder) 0x2c)).Replace("%c_startmenu%", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory | Environment.SpecialFolder.Favorites)).Replace("%c_startup%", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory | Environment.SpecialFolder.Recent));

        public string Delete(string Target)
        {
            if (Target == "*")
            {
                string newLine = Environment.NewLine;
                foreach (string str3 in Directory.GetFiles(Environment.CurrentDirectory))
                {
                    foreach (string str4 in Directory.GetDirectories(Environment.CurrentDirectory))
                    {
                        try
                        {
                            Directory.Delete(str4, true);
                            newLine = newLine + $"Deleted Directory> {Path.GetFileName(str4)}{Environment.NewLine}";
                        }
                        catch (Exception exception1)
                        {
                            ProjectData.SetProjectError(exception1);
                            Exception exception = exception1;
                            newLine = newLine + $"Error Deleting Directory> {Path.GetFileName(str4)} because {exception.Message}{Environment.NewLine}";
                            ProjectData.ClearProjectError();
                        }
                    }
                    try
                    {
                        File.Delete(str3);
                        newLine = newLine + $"Deleted File> {Path.GetFileName(str3)}{Environment.NewLine}";
                    }
                    catch (Exception exception3)
                    {
                        ProjectData.SetProjectError(exception3);
                        Exception exception2 = exception3;
                        newLine = newLine + $"Error Deleting file> {Path.GetFileName(str3)} because {exception2.Message}{Environment.NewLine}";
                        ProjectData.ClearProjectError();
                    }
                }
                return newLine;
            }
            if (File.Exists(Target))
            {
                File.Delete(Target);
                return $"Deleted file '{Target}'";
            }
            if (Directory.Exists(Target))
            {
                Directory.Delete(Target, true);
                return $"Deleted directory '{Target}'";
            }
            return $"Directory/File not found '{Target}'";
        }

        public string List()
        {
            string newLine = Environment.NewLine;
            foreach (string str2 in Directory.GetDirectories(Environment.CurrentDirectory))
            {
                newLine = newLine + $"{Path.GetFileName(str2)} <DIR>{Environment.NewLine}";
            }
            foreach (string str3 in Directory.GetFiles(Environment.CurrentDirectory))
            {
                newLine = newLine + $"{Path.GetFileName(str3)} <File>{Environment.NewLine}";
            }
            return newLine;
        }

        public string ListDirectories()
        {
            string newLine = Environment.NewLine;
            foreach (string str2 in Directory.GetDirectories(Environment.CurrentDirectory))
            {
                newLine = newLine + $"{Path.GetFileName(str2)} <DIR>{Environment.NewLine}";
            }
            return newLine;
        }

        public string ListFiles()
        {
            string newLine = Environment.NewLine;
            foreach (string str2 in Directory.GetFiles(Environment.CurrentDirectory))
            {
                newLine = newLine + $"{Path.GetFileName(str2)} <File>{Environment.NewLine}";
            }
            return newLine;
        }

        public string MakeDirectory(string Name) => 
            Directory.CreateDirectory($"{Name}").ToString();

        public string Move(string Target, string TargetPath)
        {
            if (File.Exists(Target))
            {
                File.Move(Target, $"{TargetPath}\{Path.GetFileName(Target)}");
                return $"Moved file '{Target}' to '{TargetPath}'";
            }
            if (Directory.Exists(Target))
            {
                Directory.Move(Target, $"{TargetPath}\{Path.GetFileName(Target)}");
                return $"Moved directory '{Target}' to '{TargetPath}'";
            }
            return $"Directory/File not found '{Target}'";
        }

        public string Rename(string Target, string NewName)
        {
            if (File.Exists(Target))
            {
                File.Move(Target, Target.Replace(Path.GetFileName(Target), NewName));
                return $"Renamed file '{Target}' to '{Target.Replace(Path.GetFileName(Target), NewName)}'";
            }
            if (Directory.Exists(Target))
            {
                Directory.Move(Target, Target.Replace(Path.GetFileName(Target), NewName));
                return $"Renamed directory '{Target}' to '{Target.Replace(Path.GetFileName(Target), NewName)}'";
            }
            return $"Directory/File not found '{Target}'";
        }
    }
}

