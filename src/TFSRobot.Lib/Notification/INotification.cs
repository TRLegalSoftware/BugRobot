using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSRobot
{
    public interface INotification
    {
        //If there's a notification showing right now
        bool isShowingNotification { get; set; }
        bool isNotificationConfigurated { get; set; }

        /// <summary>
        /// Configure the notification properties
        /// </summary>
        void configureNotification();

        /// <summary>
        /// Call the notification.
        /// Only if it had been configured before(use configureNotification for that)
        /// </summary>
        /// <param name="content">The content to display on the notification</param>
        void callNotification(NotificationContent notificationContent, int timeout = 0);
    }
}
