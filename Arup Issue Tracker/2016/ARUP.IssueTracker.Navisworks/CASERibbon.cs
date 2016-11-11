using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Navisworks.Api.Plugins;

namespace ARUP.IssueTracker.Navisworks
{
	/// <summary>
	/// Obfuscation Ignore for External Interface
	/// </summary>
	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	[Plugin("ARUPIssueTrackerRibbon", "CASE", DisplayName = "CASE Design, Inc.")]
	[RibbonLayout("RibbonDefinition.xaml")]
    [RibbonTab("ID_case")]
    [Command("ID_arupissuetracker", DisplayName = "Arup Issue Tracker", Icon = "ARUP.IssueTracker.Icon16x16.png", LargeIcon = "ARUP.IssueTracker.Icon32x32.png", ToolTip = "Arup Issue Tracker", ExtendedToolTip = "Arup Issue Tracker by CASE")]
	public class CASERibbon : CommandHandlerPlugin
	{
		/// <summary>
		/// Constructor, just initialises variables.
		/// </summary>
		public CASERibbon()
		{

		}

		public override int ExecuteCommand(string commandId, params string[] parameters)
		{
			switch (commandId)
			{
				case "ID_arupissuetracker":
					{

						LaunchPlugin();
						break;
					}

				default:
					{
						MessageBox.Show("You have clicked on the command with ID = '" + commandId + "'");
						break;
					}
			}

			return 0;
		}

		public override bool TryShowCommandHelp(String commandId)
		{
			MessageBox.Show("Showing Help for command with the Id " + commandId);
			return true;
		}

		/// <summary>
		/// Launch
		/// </summary>
		public void LaunchPlugin()
		{

			// Running Navis
			if (Autodesk.Navisworks.Api.Application.IsAutomated)
			{
				throw new InvalidOperationException("Invalid when running using Automation");
			}

			// Version
			if (!Autodesk.Navisworks.Api.Application.Version.RuntimeProductName.Contains("2016"))
			{
				MessageBox.Show("Incompatible Navisworks Version" +
									 "\nThis Add-In was built for Navisworks 2016, please contact info@case-inc for assistance...",
									 "Cannot Continue!",
									 MessageBoxButtons.OK,
									 MessageBoxIcon.Error);
				return;
			}

			//Find the plugin
			PluginRecord pr = Autodesk.Navisworks.Api.Application.Plugins.FindPlugin("ARUP.IssueTracker.Navisworks.Plugin.CASE");

			if (pr != null && pr is DockPanePluginRecord && pr.IsEnabled)
			{
                /*
				string m_issuetracker = Path.Combine(ProgramFilesx86(), "CASE", "ARUP Issue Tracker", "ARUP.IssueTracker.dll");
				if (!File.Exists(m_issuetracker))
				{
					MessageBox.Show("Required Issue Tracker Library Not Found");
					return;
				}
				Assembly.LoadFrom(m_issuetracker);
                 */

				//check if it needs loading
				if (pr.LoadedPlugin == null)
				{

					string exeConfigPath = GetType().Assembly.Location;
					pr.LoadPlugin();
				}

				DockPanePlugin dpp = pr.LoadedPlugin as DockPanePlugin;
				if (dpp != null)
				{
					//switch the Visible flag
					dpp.Visible = !dpp.Visible;
				}
			}

		}
		static string ProgramFilesx86()
		{
			if (8 == IntPtr.Size
				 || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
			{
				return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			}

			return Environment.GetEnvironmentVariable("ProgramFiles");
		}

	}
}
