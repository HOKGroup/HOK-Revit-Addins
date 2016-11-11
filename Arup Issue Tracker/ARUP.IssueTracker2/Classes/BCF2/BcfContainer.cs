using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ARUP.IssueTracker.Classes.BCF2
{
  /// <summary>
  /// Model View, binds to the tab control, contains the BCf files
  /// and the main methods as save, open...
  /// </summary>
  public class BcfContainer : INotifyPropertyChanged
  {
    private ObservableCollection<BcfFile> _bcfFiles { get; set; }
    private int selectedReport { get; set; }

    public BcfContainer()
    {
      BcfFiles = new ObservableCollection<BcfFile>();
    }


    public ObservableCollection<BcfFile> BcfFiles
    {
      get
      {
        return _bcfFiles;
      }

      set
      {
        _bcfFiles = value;
        NotifyPropertyChanged("BcfFiles");
      }
    }

    public int SelectedReportIndex
    {
      get
      {
        return selectedReport;
      }

      set
      {
        selectedReport = value;
        NotifyPropertyChanged("SelectedReportIndex");
      }
    }


    public void NewFile()
    {
      BcfFiles.Add(new BcfFile());
      SelectedReportIndex = BcfFiles.Count - 1;
    }
    public void SaveFile(BcfFile bcf)
    {
      SaveBcfFile(bcf);
    }
    public void MergeFiles(BcfFile bcf)
    {
      var bcffiles = OpenBcfDialog();
      if (bcffiles == null)
        return;
      bcf.MergeBcfFile(bcffiles);
    }

    public void OpenFile(string path)
    {
      var newbcf = OpenBcfFile(path);
      BcfOpened(newbcf);
    }
    public void OpenFile()
    {
      var bcffiles = OpenBcfDialog();
      if (bcffiles == null)
        return;
      foreach (var bcffile in bcffiles)
      {
        if (bcffile == null)
          continue;
        BcfOpened(bcffile);
      }
    }

    private void BcfOpened(BcfFile newbcf)
    {
      if (newbcf != null)
      {
        BcfFiles.Add(newbcf);
        SelectedReportIndex = BcfFiles.Count - 1;
        if (newbcf.Issues.Any())
          newbcf.SelectedIssue = newbcf.Issues.First();
      }
    }

    public void CloseFile(BcfFile bcf)
    {
      try
      {
        _bcfFiles.Remove(bcf);
        Utils.DeleteDirectory(bcf.TempPath);
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }

    }


    #region private methods
    /// <summary>
    /// Prompts a dialog to select one or more BCF files to open
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<BcfFile> OpenBcfDialog()
    {
      try
      {
        var openFileDialog1 = new Microsoft.Win32.OpenFileDialog();
        openFileDialog1.Filter = "BIM Collaboration Format (*.bcfzip)|*.bcfzip";
        openFileDialog1.DefaultExt = ".bcfzip";
        openFileDialog1.Multiselect = true;
        openFileDialog1.RestoreDirectory = true;
        openFileDialog1.CheckFileExists = true;
        openFileDialog1.CheckPathExists = true;
        var result = openFileDialog1.ShowDialog(); // Show the dialog.

        if (result == true) // Test result.
        {
          return openFileDialog1.FileNames.Select(OpenBcfFile).ToList();
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return null;
    }

    /// <summary>
    /// Logic that extracts files from a bcfzip and deserializes them
    /// </summary>
    /// <param name="bcfzipfile">Path to the .bcfzip file</param>
    /// <returns></returns>
    private static BcfFile OpenBcfFile(string bcfzipfile)
    {
      var bcffile = new BcfFile();
      try
      {
        if (!File.Exists(bcfzipfile) || !String.Equals(Path.GetExtension(bcfzipfile), ".bcfzip", StringComparison.InvariantCultureIgnoreCase))
          return bcffile;


        bcffile.Filename = Path.GetFileNameWithoutExtension(bcfzipfile);
        bcffile.Fullname = bcfzipfile;

        using (ZipArchive archive = ZipFile.OpenRead(bcfzipfile))
        {
          archive.ExtractToDirectory(bcffile.TempPath);
        }

        var dir = new DirectoryInfo(bcffile.TempPath);

        var projectFile = Path.Combine(bcffile.TempPath, "project.bcfp");
        if (File.Exists(projectFile))
        {
          var project = DeserializeProject(projectFile);
          var g = Guid.NewGuid();
          Guid.TryParse(project.Project.ProjectId, out g);
          bcffile.ProjectId = g;
        }
         

        //ADD ISSUES FOR EACH SUBFOLDER

        foreach (var folder in dir.GetDirectories())
        {
          //An issue needs at least the markup file
          var markupFile = Path.Combine(folder.FullName, "markup.bcf");
          if (!File.Exists(markupFile))
            continue;

          var bcfissue = DeserializeMarkup(markupFile);


          if (bcfissue == null)
            continue;

          //Is a BCF 2 file, has multiple viewpoints
          if (bcfissue.Viewpoints != null && bcfissue.Viewpoints.Any())
          {
            foreach (var viewpoint in bcfissue.Viewpoints)
            {
              string viewpointpath = Path.Combine(folder.FullName, viewpoint.Viewpoint);
              if (File.Exists(viewpointpath))
              {
                //deserializing the viewpoint into the issue
                viewpoint.VisInfo = DeserializeViewpoint(viewpointpath);
                viewpoint.SnapshotPath = Path.Combine(folder.FullName, viewpoint.Snapshot);
              }
            }
          }
          //Is a BCF 1 file, only one viewpoint
          //there is no Viewpoints tag in the markup
          //update it to BCF 2
          else
          {
            bcfissue.Viewpoints = new ObservableCollection<ViewPoint>();
            string viewpointFile = Path.Combine(folder.FullName, "viewpoint.bcfv");
            if (File.Exists(viewpointFile))
            {
              bcfissue.Viewpoints.Add(new ViewPoint(true)
              {
                VisInfo = DeserializeViewpoint(viewpointFile),
                SnapshotPath = Path.Combine(folder.FullName, "snapshot.png"),
              });
              //update the comments
              foreach (var comment in bcfissue.Comment)
              {
                comment.Viewpoint = new CommentViewpoint();
                comment.Viewpoint.Guid = bcfissue.Viewpoints.First().Guid;
              }
            }
          }
          bcfissue.Comment = new ObservableCollection<Comment>(bcfissue.Comment.OrderBy(x => x.Date));
          //register the collectionchanged events,
          //it is needed since deserialization overwrites the ones set in the constructor
          bcfissue.RegisterEvents();
          //ViewComment stuff
          bcffile.Issues.Add(bcfissue);
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return bcffile;
    }

    /// <summary>
    /// Serializes to a bcfzip and saves it to disk
    /// </summary>
    /// <param name="bcffile"></param>
    /// <returns></returns>
    public static bool SaveBcfFile(BcfFile bcffile)
    {
      try
      {
        if (bcffile.Issues.Count == 0)
        {
          MessageBox.Show("The current BCF Report is empty.", "No Issue", MessageBoxButton.OK, MessageBoxImage.Error);
          return false;
        }
        if (!Directory.Exists(bcffile.TempPath))
          Directory.CreateDirectory(bcffile.TempPath);
        // Show save file dialog box
        string name = !string.IsNullOrEmpty(bcffile.Filename)
            ? bcffile.Filename
            : "New BCF Report";
        string filename = SaveBcfDialog(name);

        // Process save file dialog box results
        if (string.IsNullOrWhiteSpace(filename))
          return false;

        var bcfProject = new ProjectExtension
        {
          Project = new Project
          {
            Name =string.IsNullOrEmpty(bcffile.ProjectName) ? bcffile.Filename : bcffile.ProjectName,
            ProjectId = bcffile.ProjectId.Equals(Guid.Empty) ? Guid.NewGuid().ToString() : bcffile.ProjectId.ToString()
          },
          ExtensionSchema = "ExtensionSchema.xsd"
           
        };
        var bcfVersion = new Version { VersionId = "2.0", DetailedVersion = "2.0 RC" };

        var serializerP = new XmlSerializer(typeof(ProjectExtension));
        Stream writerP = new FileStream(Path.Combine(bcffile.TempPath, "project.bcfp"), FileMode.Create);
        serializerP.Serialize(writerP, bcfProject);
        writerP.Close();

        var serializerVers = new XmlSerializer(typeof(Version));
        Stream writerVers = new FileStream(Path.Combine(bcffile.TempPath, "bcf.version"), FileMode.Create);
        serializerVers.Serialize(writerVers, bcfVersion);
        writerVers.Close();

        var serializerV = new XmlSerializer(typeof(VisualizationInfo));
        var serializerM = new XmlSerializer(typeof(Markup));
        
        foreach (var issue in bcffile.Issues)
        {
          // Serialize the object, and close the TextWriter
          string issuePath = Path.Combine(bcffile.TempPath, issue.Topic.Guid);
          if (!Directory.Exists(issuePath))
            Directory.CreateDirectory(issuePath);

            //BCF 1 compatibility
            //there needs to be a view whose viewpoint and snapshot are named as follows and not with a guid
            //uniqueness is still guarenteed by the guid field

            if (issue.Viewpoints != null)
            {
                if (issue.Viewpoints.Any() && (issue.Viewpoints.Count == 1 || issue.Viewpoints.All(o => o.Viewpoint != "viewpoint.bcfv")))
                {
                    if (File.Exists(Path.Combine(issuePath, issue.Viewpoints[0].Viewpoint)))
                        File.Move(Path.Combine(issuePath, issue.Viewpoints[0].Viewpoint), Path.Combine(issuePath, "viewpoint.bcfv"));
                    issue.Viewpoints[0].Viewpoint = "viewpoint.bcfv";
                    if (File.Exists(Path.Combine(issuePath, issue.Viewpoints[0].Snapshot)))
                        File.Move(Path.Combine(issuePath, issue.Viewpoints[0].Snapshot), Path.Combine(issuePath, "snapshot.png"));
                    issue.Viewpoints[0].Snapshot = "snapshot.png";
                }
            }
          
          //serialize markup with updated content
          Stream writerM = new FileStream(Path.Combine(issuePath, "markup.bcf"), FileMode.Create);
          serializerM.Serialize(writerM, issue);
          writerM.Close();

            //serialize views
            if (issue.Viewpoints != null)
            {
                foreach (var bcfViewpoint in issue.Viewpoints)
                {
                    if (bcfViewpoint.Viewpoint != null)
                    {
                        string viewpointPath = Path.Combine(issuePath, bcfViewpoint.Viewpoint);
                        if (bcfViewpoint.VisInfo != null)
                        {
                            Stream writerV = new FileStream(viewpointPath, FileMode.Create);
                            serializerV.Serialize(writerV, bcfViewpoint.VisInfo);
                            writerV.Close();
                        }
                        else if (File.Exists(viewpointPath))    // this code block is for Solibri
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.PreserveWhitespace = true;
                            try
                            {
                                doc.Load(viewpointPath);
                                doc.DocumentElement.SetAttribute("noNamespaceSchemaLocation",
                                                  "http://www.w3.org/2001/XMLSchema-instance",
                                                  "visinfo.xsd"
                                                 );
                                doc.Save(viewpointPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to write schema info. Cannot be imported to Solibri.",
                                                   "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else // fallback logic for Solibri and BCFier
                        {
                            // add a blank viewpoint file
                            Stream writerV = new FileStream(viewpointPath, FileMode.Create);
                            serializerV.Serialize(writerV, new VisualizationInfo());
                            writerV.Close();
                        }
                    }                    
                }
            }  
        }

        //overwrite, without doubts
        if (File.Exists(filename))
          File.Delete(filename);

        using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
        {
            zip.AddDirectory(bcffile.TempPath);
            zip.Save(filename);
        }

        //this library cause conflicts with Solibri
        //ZipFile.CreateFromDirectory(bcffile.TempPath, filename, CompressionLevel.Fastest, false);

        //DeleteDirectory(bcffile.TempPath);

        //Open browser at location
        Uri uri2 = new Uri(filename);
        string reportname = Path.GetFileName(uri2.LocalPath);

        if (File.Exists(filename))
        {
          string argument = @"/select, " + filename;
          System.Diagnostics.Process.Start("explorer.exe", argument);
        }
        bcffile.HasBeenSaved = true;
        bcffile.Filename = reportname;
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return true;
    }
    /// <summary>
    /// Prompts a the user to select where to save the bcfzip
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private static string SaveBcfDialog(string filename)
    {
      var saveFileDialog = new Microsoft.Win32.SaveFileDialog
      {
        Title = "Save as BCF report file (.bcfzip)",
        FileName = filename,
        DefaultExt = ".bcfzip",
        Filter = "BIM Collaboration Format (*.bcfzip)|*.bcfzip"
      };

      //if it goes fine I return the filename, otherwise empty
      var result = saveFileDialog.ShowDialog();
      return result == true ? saveFileDialog.FileName : "";
    }

    public static VisualizationInfo DeserializeViewpoint(string path)
    {
      VisualizationInfo output = null;
      try
      {

        using (var viewpointFile = new FileStream(path, FileMode.Open))
        {
          var serializerS = new XmlSerializer(typeof(VisualizationInfo));
          output = serializerS.Deserialize(viewpointFile) as VisualizationInfo;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return output;
    }
    private static Markup DeserializeMarkup(string path)
    {
      Markup output = null;
      try
      {
        using (var markupFile = new FileStream(path, FileMode.Open))
        {
          var serializerM = new XmlSerializer(typeof(Markup));
          output = serializerM.Deserialize(markupFile) as Markup;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return output;
    }

    private static ProjectExtension DeserializeProject(string path)
    {
      ProjectExtension output = null;
      try
      {
        using (var markupFile = new FileStream(path, FileMode.Open))
        {
          var serializerM = new XmlSerializer(typeof(ProjectExtension));
          output = serializerM.Deserialize(markupFile) as ProjectExtension;
        }
      }
      catch (System.Exception ex1)
      {
        MessageBox.Show("exception: " + ex1);
      }
      return output;
    }

    public static void DeleteDirectory(string target_dir)
    {
        try
        {
            if (Directory.Exists(target_dir))
            {
                string[] files = Directory.GetFiles(target_dir);
                string[] dirs = Directory.GetDirectories(target_dir);
                foreach (string file in files)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }

                foreach (string dir in dirs)
                {
                    DeleteDirectory(dir);
                }
                Directory.Delete(target_dir, false);
            }
        }
        catch (System.Exception ex1)
        {
            MessageBox.Show("exception: " + ex1);
        }
    }


    #endregion

    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
  }
}
