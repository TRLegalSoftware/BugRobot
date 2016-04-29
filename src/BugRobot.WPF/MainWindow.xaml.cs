using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TFSRobot;
using static TFSRobot.WorkItemRobot;

namespace BugRobot.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<WorkItemLog> bugLogs;
        bool isGetBugsRunning = false;
        WorkItemLog lastResult = new WorkItemLog()
        {
            Title = ""
        };

        WorkItemRobot bugRobot, assignToMeRobot;

        public MainWindow()
        {
            InitializeComponent();
            //Create a log list
            this.bugLogs = new List<WorkItemLog>();

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";
            //Set a default time interval in seconds
            interval.Text = "5";

            //Start a robot only for getting bugs
            bugRobot = new WorkItemRobot(query.Text, username.Text, autoAssign.IsChecked.Value, new Notification());

            bugLogsTable.ItemsSource = bugLogs;
        }

        private void RunBotButton(object sender, RoutedEventArgs e)
        {
            //Only starts if its not running
            if (state.Text.Equals("Not running"))
            {
                startBot();
            }
            else
            {
                stopBot();
            }
        }

        private void startBot()
        {
            try
            {
                //Change the state to starting
                state.Text = "Starting...";
                stateGetBugsFromTFS.Text = "Waiting...";
                startButton.Content = "Stop bot";

                Action threadAction = () =>
                {
                    bugRobot.Run(bug =>
                        bug.Type.Name.ToLower() == "bug" &&
                        bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                        bug.Fields["Assigned To"].Equals(string.Empty)
                    );
                };

                //CancellationTokenSource _cts = new CancellationTokenSource((int.Parse(interval.Text))*1000);

                var CurrentCulture = Thread.CurrentThread.CurrentCulture;
                var CurrentUICulture = Thread.CurrentThread.CurrentUICulture;

                var newtask = Task.Factory.StartNew(() =>
                {
                    var OldCulture = Thread.CurrentThread.CurrentCulture;
                    var OldUICulture = Thread.CurrentThread.CurrentUICulture;
                    Thread.CurrentThread.CurrentCulture = CurrentCulture;
                    Thread.CurrentThread.CurrentUICulture = CurrentUICulture;

                    try
                    {
                        threadAction();
                    }
                    finally
                    {
                        Thread.CurrentThread.CurrentCulture = OldCulture;
                        Thread.CurrentThread.CurrentUICulture = OldUICulture;
                    }
                });


                state.Text = "Running";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }

        }

        private void stopBot()
        {
            //Stops the timer and then changes the state text
            //if (timer != null)
            //{
            //    timer.Stop();
            //    timer.Enabled = false;
            //}
            //state.Text = "Not running";
            //stateGetBugsFromTFS.Text = "Stopped";
            //startButton.Content = "Run bot";
        }

        #region deprecated

        //private void GetWorkItems()
        //{
        //    Dispatcher.Invoke(new Action(() => {
        //        this.isGetBugsRunning = true;
        //        stateGetBugsFromTFS.Text = "Connecting...";

        //        WorkItemRobot bugRobot;

        //        //Get the robot
        //        bugRobot = new WorkItemRobot(query.Text, username.Text, autoAssign.IsChecked.Value, onlyUnassignedBugs.IsChecked.Value);                        

        //        stateGetBugsFromTFS.Text = "Checking";

        //        //Run it
        //        WorkItemLog result;

        //        try
        //        {
        //            result = bugRobot.Run();

        //            //If there's something
        //            if (result.Success && !(lastResult.Title.Equals(result.Title)))
        //            {
        //                stateGetBugsFromTFS.Text = "Success!";
        //                lastResult = result;

        //                new Notification(result.Icon).callNotification("Bug " + result.Ids, result.Title, result.Url);                    

        //                bugLogs.Add(new WorkItemLog() {
        //                    Hour = DateTime.Now.ToString("HH:mm:ss"),
        //                    Id = result.Ids,
        //                    Title = result.Title
        //                });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            stateGetBugsFromTFS.Text = "Failed.";
        //            MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
        //        }


        //        this.isGetBugsRunning = false;
        //        stateGetBugsFromTFS.Text = "Waiting...";
        //        if (runOnce.IsChecked.Value)
        //        {
        //            stopBot();
        //        }
        //    }), DispatcherPriority.ContextIdle);
        //}

        #endregion
    }
}
