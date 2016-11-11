using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace ARUP.IssueTracker.Revit.Entry
{

	/// <summary>
	/// Obfuscation Ignore for External Interface
	/// </summary>
	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	[Transaction(TransactionMode.Manual)]
	public class AppMain : IExternalApplication
	{
		private string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		#region Revit IExternalApplciation Implementation

		/// <summary>
		/// Startup
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public Result OnStartup(UIControlledApplication application)
		{

			try
			{

				// Version
				if (!application.ControlledApplication.VersionName.Contains("2014") && !application.ControlledApplication.VersionName.Contains("2015") && !application.ControlledApplication.VersionName.Contains("2016"))
				{
					using (TaskDialog td = new TaskDialog("Cannot Continue"))
					{
						td.TitleAutoPrefix = false;
						td.MainInstruction = "Incompatible Revit Version";
						td.MainContent = "This Add-In was built for Revit 2014, 2015 and 2016, please contact CASE for assistance.";
						td.Show();
					}
					return Result.Cancelled;
				}

				// Master Tab 
				const string c_tabName = "CASE";

				try
				{
					// Create the Tab
					application.CreateRibbonTab(c_tabName);
				}
				catch { }

                /*
				// Assembly Paths
				string m_issuetracker = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", "ARUP.IssueTracker.dll");

				// Check that File Exists
				if (!File.Exists(m_issuetracker))
				{
					using (TaskDialog td = new TaskDialog("Cannot Continue"))
					{
						td.TitleAutoPrefix = false;
						td.MainInstruction = "Required Issue Tracker Library Not Found";
						td.MainContent = m_issuetracker;
						td.Show();
					}
					return Result.Cancelled;
				}

				// Load Assemblies
				Assembly.LoadFrom(m_issuetracker);
                 */

                string m_issuetracker = "ARUP.IssueTracker.dll";

				// Tab
				RibbonPanel m_panel = application.CreateRibbonPanel(c_tabName, "Arup Issue Tracker");

				// Button Data
				PushButton m_pushButton = m_panel.AddItem(new PushButtonData("Issue Tracker",
																								 "Issue Tracker",
																								 Path.Combine(_path, "ARUP.IssueTracker.Revit.dll"),
																								 "ARUP.IssueTracker.Revit.Entry.CmdMain")) as PushButton;

				// Images and Tooltip
				if (m_pushButton != null)
				{
					m_pushButton.Image = LoadPngImgSource("ARUP.IssueTracker.Assets.ARUPIssueTrackerIcon16x16.png", m_issuetracker);
					m_pushButton.LargeImage = LoadPngImgSource("ARUP.IssueTracker.Assets.ARUPIssueTrackerIcon32x32.png", m_issuetracker);
					m_pushButton.ToolTip = "Arup Issue Manager";
				}

                // Initiate RestSharp first to avoid the conflict with Dynamo
                // string restSharpDllPath = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", "RestSharp.dll");
                // Assembly.LoadFrom(restSharpDllPath);
            }
			catch (Exception ex1)
			{
				MessageBox.Show("exception: " + ex1);
				return Result.Failed;
			}

			return Result.Succeeded;
		}

		/// <summary>
		/// Shut Down
		/// </summary>
		/// <param name="application"></param>
		/// <returns></returns>
		public Result OnShutdown(UIControlledApplication application)
		{
			return Result.Succeeded;
		}

		#endregion

		#region Private Members

		/// <summary>
		/// Get System Architecture
		/// </summary>
		/// <returns></returns>
		static string ProgramFilesx86()
		{
			if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
				return Environment.GetEnvironmentVariable("ProgramFiles(x86)");

			return Environment.GetEnvironmentVariable("ProgramFiles");
		}


		/// <summary>
		/// Load an Image Source from File
		/// </summary>
		/// <param name="sourceName"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private ImageSource LoadPngImgSource(string sourceName, string path)
		{

			try
			{
				// Assembly & Stream
                string currentAssembly = System.Reflection.Assembly.GetAssembly(this.GetType()).Location;
                string currentDirectory = Path.GetDirectoryName(currentAssembly);
                string combinedPath = Path.Combine(currentDirectory, path);
                Assembly m_assembly = Assembly.LoadFrom(combinedPath);
                Stream m_icon = m_assembly.GetManifestResourceStream(sourceName);

				// Decoder
				PngBitmapDecoder m_decoder = new PngBitmapDecoder(m_icon, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

				// Source
				ImageSource m_source = m_decoder.Frames[0];
				return (m_source);

			}
			catch(Exception ex) 
            {
                MessageBox.Show("Failed to load image source.\nSource Name:" + sourceName + "\nPath:" + path + "\n" + ex.Message);
            }

			// Fail
			return null;

		}

		#endregion

	}
}