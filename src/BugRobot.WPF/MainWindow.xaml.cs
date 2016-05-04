using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        //Creates new WorkItemRobots for each function needed
        WorkItemRobot bugRobot, assignToMeRobot;

        //Create the log list
        private static ObservableCollection<WorkItemLog> logList;
        private bool isBugRobotRunning = false;
        private bool isGetWorkItemsRobotRunning = false;
        private CancellationTokenSource runBugsToken, getWorkItemsToken;

        public MainWindow()
        {
            logList = new ObservableCollection<WorkItemLog>();
            InitializeComponent();
        }

        private void RunBugRobotButton(object sender, RoutedEventArgs e)
        {
            //If bug robot is not running    
            if (!isBugRobotRunning)
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

            bugRobotStartButton.Content = "Stop Bot";
            bugRobotState.Text = "Running";
            isBugRobotRunning = true;
            try
            {
                //Start a async task to loop each (interval) seconds
                runBugsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;
                if (bugRobotRunOnce.IsChecked.Value)
                {
                    runOnceBot(bugRobot, runBugsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.Equals(string.Empty)
                        );
                    stopBot();
                }
                else
                {
                    var taskGetBugs = Task.Factory.StartNew(async a =>
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            bugRobotStateGetBugs.Text = "Connecting.";
                        }));
                        runBot(intervalMs, bugRobot, runBugsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.Equals(string.Empty)
                        );
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            bugRobotStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                        }));
                    }, runBugsToken.Token, TaskCreationOptions.LongRunning);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
                bugRobotState.Text = "Failed";
            }
        }

        private void stopBot()
        {
            runBugsToken.Cancel();
            bugRobotState.Text = "Not Running";
            bugRobotStateGetBugs.Text = "Stopped";

            bugRobotStartButton.Content = "Run Bot";
            isBugRobotRunning = false;
        }


        private void RunWIBotButton(object sender, RoutedEventArgs e)
        {
            if (!username.Text.Equals(""))
            {
                //If bug robot is not running    
                if (!isGetWorkItemsRobotRunning)
                {
                    startWiBot();
                }
                else
                {
                    stopWiBot();
                }
            }
            else
            {
                MessageBox.Show("Username cannot be empty");
            }
        }

        private void startWiBot()
        {
            assignToMeStartButton.Content = "Stop Bot";
            assignToMeState.Text = "Running";
            assignToMeStateGetBugs.Text = "Starting...";
            isGetWorkItemsRobotRunning = true;
            try
            {
                getWorkItemsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;
                var user = username.Text;

                if (assignToMeRunOnce.IsChecked.Value)
                {
                    runOnceBot(assignToMeRobot, getWorkItemsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.ToString().ToLower().Equals(user.ToLower())
                        );
                    stopWiBot();
                }
                else
                {
                    //Start a async task to loop each (interval) seconds 
                    var taskGetWorkItems = Task.Factory.StartNew(async a =>
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            assignToMeStateGetBugs.Text = "Connecting.";
                        }));
                        runBot(intervalMs, assignToMeRobot, getWorkItemsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.ToString().ToLower().Equals(user.ToLower())
                        );
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            assignToMeStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                        }));
                    }, getWorkItemsToken.Token, TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
                assignToMeState.Text = "Failed";
            }
        }

        private void stopWiBot()
        {
            getWorkItemsToken.Cancel();
            assignToMeState.Text = "Not Running";
            assignToMeStateGetBugs.Text = "Stopped";
            assignToMeStartButton.Content = "Run Bot";
            isGetWorkItemsRobotRunning = false;
        }

        private static void addToGrid(WorkItemLog workItemLog)
        {
            if (!logList.Any(l => l.Id == workItemLog.Id))
            {
                logList.Add(workItemLog);
            }
        }

        private static async void runBot(int interval, WorkItemRobot robot, CancellationTokenSource cancelToken, Func<WorkItem, bool> filter)
        {
            try
            {
                //While didn't cancel
                while (cancelToken.IsCancellationRequested == false)
                {
                    //Run the bot and gets its list
                    var buglist = robot.Run(filter);

                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                     {
                         //Foreach item on that list, add it to the table
                         foreach (var bug in buglist)
                         {
                             addToGrid(bug);
                         }
                     }));
                    await Task.Delay(interval, cancelToken.Token);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }
        }

        private static void runOnceBot(WorkItemRobot robot, CancellationTokenSource cancelToken, Func<WorkItem, bool> filter)
        {
            try
            {
                //Run the bot and gets its list
                var buglist = robot.Run(filter);


                //Foreach item on that list, add it to the table
                foreach (var bug in buglist)
                {
                    addToGrid(bug);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vers = Assembly.GetExecutingAssembly().GetName().Version;
            versionBox.Content = string.Format("Version: {0}.{1}.{2}.{3}", vers.Major, vers.Minor, vers.Build, vers.Revision);
            this.DataContext = this;

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";
            //Set a default time interval in seconds
            interval.Text = "60";

            //Start a robot only for getting bugs
            bugRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Set the username if want to autoAssign
                username.Text,
                //Context string
                "em aberto",
                //Set if want to autoAssign value
                bugRobotAutoAssign.IsChecked.Value,
                //Gives it the notification system
                new Notification(Properties.Resources.BugIcon)
            );

            assignToMeRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Set the username if want to autoAssign
                username.Text,
                "adicionado(s) ao meu nome",
                //Set if want to autoAssign value
                false,
                //Gives it the notification system
                new Notification(Properties.Resources.TFSIcon)
            );

            //Set the WPF table list to the log list
            bugLogsTable.ItemsSource = logList;
        }
    }
}