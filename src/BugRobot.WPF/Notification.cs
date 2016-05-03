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

        private NotificationContent lastNotification = new NotificationContent()
        {
            Title = ""
        };

        public Notification(string imgPath = "")
        {
            this.configureNotification(imgPath);
        }

        public Notification(Bitmap imageFile)
        {
            this.configureNotification(imageFile);
        }

        public void callNotification(NotificationContent notificationContent, int timeout = 0)
        {
            if (isNotificationConfigurated && (lastNotification.Title != notificationContent.Title))
            {
                //Set notification's title
                this.notificationSystem.BalloonTipTitle = notificationContent.Title;

                //Set notification's body message
                if (!string.IsNullOrEmpty(notificationContent.Content))
                {
                    this.notificationSystem.BalloonTipText = notificationContent.Content;
                } else
                {
                    //It must be a space, or the baloon won't work
                    this.notificationSystem.BalloonTipText = " ";
                }

                //Set notification's link
                if (notificationContent.Url != null && notificationContent.Url.Count() > 8)
                {
                    this.currentUrl = notificationContent.Url;
                    this.notificationSystem.BalloonTipClicked += new EventHandler(redirectToLink);
                }

                //Set notification's timeout
                this.notificationSystem.ShowBalloonTip(timeout);
                lastNotification = notificationContent;
            }
        }

        private void configureNotification(string imgPath = "")
        {
            Bitmap imageFile = null;
            if (imgPath.Length > 4)
            {
                imageFile = new Bitmap(imgPath);
            }
            this.configureNotification(imageFile);
        }

        private void configureNotification(Bitmap imageFile)
        {
            this.notificationSystem = new NotifyIcon();
            this.notificationSystem.Icon = System.Drawing.SystemIcons.Warning;
            this.notificationSystem.Visible = true;
            //Tests if there is really a image path
            //since this class is allways called
            //Passing an icon
            if (imageFile!=null)
            {   
                var iconHandle = imageFile.GetHicon();
                var icon = System.Drawing.Icon.FromHandle(iconHandle);
                this.notificationSystem.Icon = icon;
            }
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
