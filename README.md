# TFSRobot - TFS Bug tracking application
> A complete rewrite of the BugRobot to work with the TFS

This document describes how our bug notifier application works and how to use it. Feel free to update this document if you find anything that should be improved.
 
Bug robot is an application we use in the maintenance team to track if new bugs has arrived. It shows a notification in your system tray so you don't need to check TFS page every time if there's a new bug, you'll know it just by seeing the little balloon notification.
 
It is a WPF based application **still on its basic version**, be aware that there are more improvements to come over time. There was a web version to run besides with IIS(or anything that could run the build) but I had to rewrite the application and didn't worked in a web version based on its rewrite because a desktop version doesn't need any server to run in a way that anyone can run it w/out any problems *if you have access to our query in TFS*.
 
It looks for updated versions of itself every 2 hours.
 
The query is still readonly because it must be that query. And the field is enabled in case you want to copy it and open it on your browser.

------------
# Functionalities

## Bug Tracking
BugRobot checks if there's a new unassigned bug every given seconds, if the list has changed, it shows you the previous unassigned bugs(that are still unassigned to remember that there are still open bugs) and the new opened bug.

Options:
- Auto Assign - Every time a new bug arrive it'll be automatically assigned to the given user on the username field
- Run Once - Tries to get new bugs only one time. It **doesn't try again** after any given seconds if its checked.

**Bug Tracking rules:**
- If auto assign is checked. The username field MUST have your TFS name

## Work Item Tracking
With this tool, you'll be notified every time that a Work Item on the given query had been assigned to the given TFS profile name on the **Username** field

Options:
- Run Once - Tries to get new bugs only one time. It doesn't try again after any given seconds if its checked.

**Work Item Tracking rules:**
- The username field MUST have your TFS name.

## Recent Grid
This grid shows every Work Item that BugRobot has notified you, if there's a URL you can click on its row and the desired bug will be opened on your browser

Grid columns:

| Column | Functionality |
| --- | --- |
| Bot | Shows which bot notified you |
| ID | Work Item number on TFS |
| Title | Title field from that Work Item |
| URL | The link to be redirected to that work item(It is used to know if you can click and be redirected) |

------------

**General Rules:**
- **The username MUST be the same as it's on your TFS name(in the Assigned To field), and not your Username/Login. (Case is ignored, spaces and symbols are allowed)**

------------

# How to use.
## Bug Tracking
>To use the bug tracking tool you must set the interval in seconds and just click "Run Bot"
 
## Work Item Tracking
>First, you need to put your name as it is on TFS on the username field(It must be identical! Even with spaces), then you set the interval in seconds and click "Run Bot"

------------

# Known Bugs:
- If you're in VPN and it tries to update or install anything, you'll get an error message saying that the update couldn't be downloaded. You have to leave VPN for this.
- If the icon is clicked when a balloon is showing, instead of opening a single tab on your browser, it'll open multiple tabs with the same link

------------

# WIP Improvements:
- Better looking, better visual spacing.
- "US Robot". A bot that'll check if your US has changed, helpful when you ask databases and want to be notified if it's ready

------------

###### Current Version: 0.0.0.26 (2016-06-01 11:15)
 
Version Log:
```
- 0.0.0.26 - The recent grid now have a horizontal slider.
```

------------
If there's some random errors when accessing TFS try to put these files in the bin folder:

Microsoft.WITDataStore32.dll,
Microsoft.WITDataStore64.dll.
