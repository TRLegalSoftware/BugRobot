using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BugRobot.WPF.Control
{
    public class NotificationControl
    {
        private NotifyIcon notificationSystem;
        public NotificationControl()
        {
            this.notificationSystem = new NotifyIcon();
            this.notificationSystem.Icon = System.Drawing.SystemIcons.Warning;
            this.notificationSystem.Visible = true;
        }

        private void setupNotification(string title, string text)
        {
            this.notificationSystem.BalloonTipTitle = title;
            this.notificationSystem.BalloonTipText = text;
        }

        public void callNotification(string title, string text) {
            this.setupNotification(title, text);
            this.notificationSystem.ShowBalloonTip(0);
        }

        public void callNotification(string title, string text, int timeout) {
            this.setupNotification(title, text);
            this.notificationSystem.ShowBalloonTip(timeout);
        }
    }
}
