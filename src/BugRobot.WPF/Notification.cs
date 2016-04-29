using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TFSRobot;

namespace BugRobot.WPF
{
    public class Notification : INotification
    {
        private string currentUrl { get; set; }
        private NotifyIcon notificationSystem;

        public bool isShowingNotification { get; set; }
        public bool isNotificationConfigurated { get; set; }

        public Notification()
        {
            this.configureNotification();
        }

        public void callNotification(NotificationContent notificationContent, int timeout = 0)
        {
            if (isNotificationConfigurated)
            {
                //Set notification's title
                this.notificationSystem.BalloonTipTitle = notificationContent.Title;

                //Set notification's body message
                if (!string.IsNullOrEmpty(notificationContent.Content))
                {
                    this.notificationSystem.BalloonTipText = notificationContent.Content;
                } else
                {
                    this.notificationSystem.BalloonTipText = "a";
                }

                //Set notification's link
                if (notificationContent.Url != null && notificationContent.Url.Count() > 8)
                {
                    this.currentUrl = notificationContent.Url;
                    this.notificationSystem.BalloonTipClicked += new EventHandler(redirectToLink);
                }

                //Set notification's timeout
                this.notificationSystem.ShowBalloonTip(timeout);
            }
        }

        private void configureNotification()
        {
            this.notificationSystem = new NotifyIcon();
            this.notificationSystem.Icon = System.Drawing.SystemIcons.Warning;
            this.notificationSystem.Visible = true;
            //Tests if there is really a image path
            //since this class is allways called
            //Passing an icon
            //if (img.Length > 4)
            //{
            //    var bitmap = new Bitmap(img);
            //    var iconHandle = bitmap.GetHicon();
            //    var icon = System.Drawing.Icon.FromHandle(iconHandle);
            //    this.notificationSystem.Icon = icon;
            //}
            this.isNotificationConfigurated = true;
        }

        //Handle link in baloon
        private void redirectToLink(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.currentUrl);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                if (ex.ErrorCode == -2147467259)
                    MessageBox.Show(ex.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        void INotification.configureNotification()
        {
            throw new NotImplementedException();
        }
    }
}
