using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace ARUP.IssueTracker.Revit.Entry
{

	/// <summary>
	/// Obfuscation Ignore for External Interface
	/// </summary>
	[Obfuscation(Exclude = true, ApplyToMembers = false)]
	[Transaction(TransactionMode.Manual)]
	[Regeneration(RegenerationOption.Manual)]
	public class CmdMain : IExternalCommand
	{
		internal static CmdMain ThisCmd = null;
		private static bool _isRunning;
		private static AppIssueTracker _appIssueTracker;

		/// <summary>
		/// Main Command Entry Point
		/// </summary>
		/// <param name="commandData"></param>
		/// <param name="message"></param>
		/// <param name="elements"></param>
		/// <returns></returns>
		public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
		{
			try
			{

				// Version
				if (!commandData.Application.Application.VersionName.Contains("2016"))
				{
					using (TaskDialog td = new TaskDialog("Cannot Continue"))
					{
						td.TitleAutoPrefix = false;
						td.MainInstruction = "Incompatible Revit Version";
						td.MainContent = "This Add-In was built for Revit 2016, please contact info@case-inc for assistance...";
						td.Show();
					}
					return Result.Cancelled;
				}

				// Form Running?
				if (_isRunning && _appIssueTracker != null && _appIssueTracker.RvtWindow.IsLoaded)
				{
					_appIssueTracker.Focus();
					return Result.Succeeded;
				}

				_isRunning = true;

				ThisCmd = this;
				_appIssueTracker = new AppIssueTracker();
				_appIssueTracker.ShowForm(commandData.Application);
				return Result.Succeeded;

			}
			catch (Exception e)
			{
				message = e.Message;
				return Result.Failed;
			}

		}

	}

}