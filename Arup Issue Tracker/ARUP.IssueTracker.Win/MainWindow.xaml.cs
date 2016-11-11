using System;
using System.Windows;
using System.ComponentModel;
using System.Deployment.Application;

namespace ARUP.IssueTracker.Win
{

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      // Buttons, etc.
      mainPan.setButtonVisib(false, false, false);
      string[] args = Environment.GetCommandLineArgs();
      if (args.Length > 1)
      {
        mainPan.OpenBCF(args[1].ToString());
      }
    }

    /// <summary>
    /// passing event to the user control
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = mainPan.onClosing(e);
    }

  }
}