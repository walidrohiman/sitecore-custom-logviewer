using System;
using System.IO;
using Sitecore;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.IO;
using Sitecore.Security.Accounts;
using Sitecore.Shell.Web;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;

namespace SitecoreExtension.CustomLogViewer.Utilities
{
    public class CustomFileListerForm : DialogForm
    {
        protected XmlControl Dialog;
        protected Listview FileLister;

        protected virtual void CheckSecurity()
        {
            ShellPage.IsLoggedIn();
            User user = Context.User;
            if (user.IsAdministrator)
                return;
            bool flag1 = user.IsInRole("sitecore\\Sitecore Client Developing");
            bool flag2 = user.IsInRole("sitecore\\Sitecore Client Maintaining");
            if (!flag1 && !flag2)
                throw new AccessDeniedException("Application access denied.");
        }

        protected void DoOK()
        {
            ListviewItem[] selectedItems = this.FileLister.SelectedItems;
            if (selectedItems.Length == 0)
                SheerResponse.Alert("Select a file.");
            else if (selectedItems.Length > 1)
            {
                SheerResponse.Alert("Select a single file.");
            }
            else
            {
                SheerResponse.SetDialogValue(selectedItems[0].ServerProperties["Path"] as string);
                SheerResponse.CloseWindow();
            }
        }

        protected void OnFileListerDblClick()
        {
            this.DoOK();
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            this.CheckSecurity();
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
                return;
            UrlHandle urlHandle = UrlHandle.Get();
            string str1 = urlHandle["ic"];
            if (!string.IsNullOrEmpty(str1))
                this.Dialog["Icon"] = (object)str1;
            string str2 = WebUtil.SafeEncode(urlHandle["he"]);
            if (str2.Length > 0)
                this.Dialog["Header"] = (object)str2;
            string str3 = WebUtil.SafeEncode(urlHandle["txt"]);
            if (str3.Length > 0)
                this.Dialog["Text"] = (object)str3;
            string str4 = WebUtil.SafeEncode(urlHandle["btn"]);
            if (str4.Length > 0)
                this.Dialog["OKButton"] = (object)str4;
            string path1 = urlHandle["fo"];
            string searchPattern = urlHandle["flt"];
            string path2 = FileUtil.MapPath(path1);
            if (urlHandle["di"] == "1")
            {
                foreach (string directory in Directory.GetDirectories(path2))
                {
                    ListviewItem listviewItem = new ListviewItem();
                    this.FileLister.Controls.Add((System.Web.UI.Control)listviewItem);
                    listviewItem.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("I");
                    listviewItem.Header = Path.GetFileName(directory);
                    listviewItem.Icon = "Applications/16x16/folder.png";
                    listviewItem.ServerProperties["Path"] = (object)FileUtil.UnmapPath(directory);
                }
            }

            string[] subFolders = Directory.GetDirectories(path2);
            foreach (var subFolder in subFolders)
            {
                ListviewItem listviewItem = new ListviewItem();
                this.FileLister.Controls.Add((System.Web.UI.Control)listviewItem);
                listviewItem.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("I");
                listviewItem.Header = Path.GetFileName(subFolder);
                listviewItem.Icon = "Applications/16x16/folder.png";
                listviewItem.ServerProperties["Path"] = (object)FileUtil.UnmapPath(subFolder);
            }

            foreach (string file in Directory.GetFiles(path2, searchPattern))
            {
                ListviewItem listviewItem = new ListviewItem();
                this.FileLister.Controls.Add(listviewItem);
                listviewItem.ID = Control.GetUniqueID("I");
                listviewItem.Header = Path.GetFileName(file);
                listviewItem.Icon = "Applications/16x16/document.png";
                listviewItem.ServerProperties["Path"] = (object)FileUtil.UnmapPath(file);
                FileInfo fileInfo = new FileInfo(FileUtil.MapPath(file));
                listviewItem.ColumnValues["size"] = (object)MainUtil.FormatSize(fileInfo.Length);
                listviewItem.ColumnValues["modified"] = (object)DateUtil.FormatShortDateTime(DateUtil.ToServerTime(fileInfo.LastWriteTimeUtc));
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull((object)args, nameof(args));
            this.DoOK();
        }
    }
}