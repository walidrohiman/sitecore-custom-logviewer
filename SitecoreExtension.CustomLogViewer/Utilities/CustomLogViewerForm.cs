using System;
using System.IO;
using System.Web;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Shell.Applications.Reports.LogViewer;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace SitecoreExtension.CustomLogViewer.Utilities
{
    public class CustomLogViewerForm : LogViewerForm
    {
        [HandleMessage("logviewer:Open", true)]
        public new void Open(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull((object)args, nameof(args));
            if (args.IsPostBack)
            {
                if (args.Result.Length <= 0 || args.Result == "undefined")
                    return;
                string file = GetFile(args.Result);
                if (string.IsNullOrEmpty(file))
                    SheerResponse.Alert("You can only open log files.");
                else if (!file.Contains(".txt")) //open sub folders found in logs
                {
                    ListFiles("Open Log File", " ", "Software/32x32/text_code_colored.png", nameof(Open), file, "*.txt", false);
                    args.WaitForPostBack();
                }
                else
                    this.SetFile(file);
            }
            else
            {
                ListFiles("Open Log File", " ", "Software/32x32/text_code_colored.png", nameof(Open), Settings.LogFolder, "*.txt", false);
                args.WaitForPostBack();
            }
        }
        private static string GetFile(string file)
        {
            if (string.IsNullOrEmpty(file) || !FileUtil.MapPath(file).StartsWith(FileUtil.MapPath(Settings.LogFolder), StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;
            return file;
        }

        private void SetFile(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                Context.ClientPage.ServerProperties["File"] = (object)null;
                this.Document.SetSource("control:LogViewerDetails", string.Empty);
                Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarTitle", Translate.Text("Log Files"));
                Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarDescription", Translate.Text("This tool displays the content of log files."));
                this.HasFile.Disabled = true;
            }
            else
            {
                Context.ClientPage.ServerProperties["File"] = (object)filename;
                this.Document.SetSource("control:LogViewerDetails", "file=" + HttpUtility.UrlEncode(filename));
                FileInfo fileInfo = new FileInfo(FileUtil.MapPath(filename));
                Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarTitle", Path.GetFileNameWithoutExtension(filename));
                Context.ClientPage.ClientResponse.SetInnerHtml("Commandbar_CommandbarDescription", Translate.Text(Translate.Text("Last access: {0}") + "<br/>", (object)DateUtil.FormatShortDateTime(DateUtil.ToServerTime(fileInfo.LastWriteTimeUtc))) + Translate.Text("Size: {0}", (object)MainUtil.FormatSize(fileInfo.Length)));
                this.HasFile.Disabled = false;
            }
        }

        public static void ListFiles(string header, string text, string icon, string button, string folder, string filter, bool directories)
        {
            Assert.ArgumentNotNull((object)header, nameof(header));
            Assert.ArgumentNotNull((object)text, nameof(text));
            Assert.ArgumentNotNull((object)icon, nameof(icon));
            Assert.ArgumentNotNull((object)button, nameof(button));
            Assert.ArgumentNotNull((object)folder, nameof(folder));
            Assert.ArgumentNotNull((object)filter, nameof(filter));
            Assert.ArgumentNotNull((object)directories, nameof(directories));

            UrlString urlString = new UrlString(UIUtil.GetUri("control:CustomFileLister"));
            new UrlHandle()
            {
                ["he"] = header,
                ["txt"] = text,
                ["ic"] = icon,
                ["btn"] = button,
                ["fo"] = folder,
                ["flt"] = filter,
                ["di"] = (directories ? "1" : "0")
            }.Add(urlString);
            Context.ClientPage.ClientResponse.ShowModalDialog(new ModalDialogOptions(urlString.ToString())
            {
                Response = true,
                MinWidth = "580px"
            });
        }
    }
}