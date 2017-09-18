using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using HOK.Core.Utilities;

namespace HOK.MoveBackup
{
    public class MoveBackupModel
    {
        public string RevitFilePath { get; set; }
        private const string BackupFolderName = "Backup";
        private const int NumberOfBackups = 3;

        public void MoveBackupFiles()
        {
            try
            {
                var saveDirectory = Path.GetDirectoryName(RevitFilePath);
                if (string.IsNullOrEmpty(saveDirectory)) return;

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(RevitFilePath);
                var extension = Path.GetExtension(RevitFilePath);
                if (string.IsNullOrEmpty(extension)) return;
                if (!AppCommand.extensions.Contains(extension)) return;

                var backupDirectory = Path.Combine(saveDirectory, BackupFolderName);
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                // (Konrad) First we need to count how many backup files with matching name were already saved
                var regex = new Regex("(\\d{4})\\" + extension + "$", RegexOptions.IgnoreCase);
                var suffix = 1;
                var searchPattern = ".????" + extension;
                var existingBackups = Directory.GetFiles(backupDirectory, fileNameWithoutExtension + searchPattern);
                Array.Sort(existingBackups);
                if (existingBackups.Any())
                {
                    var match = regex.Match(existingBackups.Last());
                    if (match.Success)
                    {
                        suffix = int.Parse(match.Groups[1].Captures[0].Value) + 1;
                    }
                }

                // (Konrad) Now we have to override the name of the new backup file created to account for existing
                var newBackups = Directory.GetFiles(saveDirectory, fileNameWithoutExtension + searchPattern);
                Array.Sort(newBackups);
                if (newBackups.Any())
                {
                    foreach (var file in newBackups)
                    {
                        var fileName = Path.GetFileName(file);
                        if (string.IsNullOrEmpty(fileName) || !regex.Match(fileName).Success) return;

                        var backupFileName = Path.Combine(backupDirectory, $"{fileNameWithoutExtension}.{suffix:D4}{extension}");
                        var flag = false;
                        for (var i = 0; i < 5; ++i)
                        {
                            try
                            {
                                if (File.Exists(backupFileName))
                                    File.Delete(backupFileName);
                                File.Move(fileName, backupFileName);
                                flag = true;
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                                Thread.Sleep((int)(5000.0 * Math.Pow(3.0, i)));
                            }
                            if (flag) break;
                        }
                        suffix++;
                    }
                }

                // (Konrad) It makes sense to store only 3-4 backups at most and just delete the rest
                var allBackups = Directory.GetFiles(backupDirectory, fileNameWithoutExtension + searchPattern);
                Array.Sort(allBackups);
                for (var j = 0; j < allBackups.Length - NumberOfBackups; j++)
                {
                    var flag = false;
                    for (var k = 0; k < 5; k++)
                    {
                        try
                        {
                            File.Delete(allBackups[j]);
                            flag = true;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                            Thread.Sleep((int)(5000.0 * Math.Pow(3.0, k)));
                        }
                        if (flag) break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, e.Message);
            }
        }
    }
}
