namespace DotNetNuke.Services.Url.FriendlyUrl
{
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.HttpModules;
    using DotNetNuke.HttpModules.Config;
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Text.RegularExpressions;
    using System.Web;

    public class forDNNFriendlyUrlProvider : FriendlyUrlProvider
    {
        private bool _addNothing = false;
        private string _fileExtension;
        private bool _includePageName;
        private ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration("friendlyUrl");
        private string _regexMatch;
        private UrlFormatType _urlFormat = UrlFormatType.SearchFriendly;
        private const string ProviderType = "friendlyUrl";
        private const string RegexMatchExpression = "[^a-zA-Z0-9 ]";

        public forDNNFriendlyUrlProvider()
        {
            Provider provider = (Provider) this._providerConfiguration.Providers[this._providerConfiguration.DefaultProvider];
            if (Convert.ToString(provider.Attributes["includePageName"]) != "")
            {
                this._includePageName = bool.Parse(provider.Attributes["includePageName"]);
            }
            else
            {
                this._includePageName = true;
            }
            if (Convert.ToString(provider.Attributes["regexMatch"]) != "")
            {
                this._regexMatch = provider.Attributes["regexMatch"];
            }
            else
            {
                this._regexMatch = "[^a-zA-Z0-9 ]";
            }
            if (Convert.ToString(provider.Attributes["fileExtension"]) != "")
            {
                this._fileExtension = provider.Attributes["fileExtension"];
            }
            else
            {
                this._fileExtension = ".aspx";
            }
            if (Convert.ToString(provider.Attributes["urlFormat"]) != "")
            {
                string str = provider.Attributes["urlFormat"].ToLower();
                if (str == "searchfriendly")
                {
                    this._urlFormat = UrlFormatType.SearchFriendly;
                }
                else if (str == "humanfriendly")
                {
                    this._urlFormat = UrlFormatType.HumanFriendly;
                }
                else
                {
                    this._urlFormat = UrlFormatType.SearchFriendly;
                }
            }
        }

        private string AddPage(string path, string pageName)
        {
            string str = path;
            if (str.EndsWith("/"))
            {
                return (str + pageName);
            }
            return (str + "/" + pageName);
        }

        private string CheckPathLength(string friendlyPath, string originalpath)
        {
            if (friendlyPath.Length >= 260)
            {
                return Globals.ResolveUrl(originalpath);
            }
            return friendlyPath;
        }

        public override string FriendlyUrl(TabInfo tab, string path)
        {
            PortalSettings currentPortalSettings = PortalController.GetCurrentPortalSettings();
            return this.FriendlyUrl(tab, path, "Default.aspx", currentPortalSettings);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName)
        {
            PortalSettings currentPortalSettings = PortalController.GetCurrentPortalSettings();
            return this.FriendlyUrl(tab, path, pageName, currentPortalSettings);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
        {
            return this.FriendlyUrl(tab, path, pageName, settings.PortalAlias.HTTPAlias);
        }

        public override string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias)
        {
            Match match;
            string str3;
            string str = path;
            bool isPagePath = tab != null;
            this._addNothing = false;
            string url = this.GetUrl(tab, path);
            if (url != "")
            {
                pageName = url;
                this._addNothing = true;
            }
            str = this.GetFriendlyAlias(path, portalAlias, isPagePath);
            str = this.GetFriendlyQueryString(tab, str, pageName);
            if (this.UrlFormat == UrlFormatType.HumanFriendly)
            {
                if (Regex.IsMatch(str, @"[^?]*/Tab[Ii]d/(\d+)/default.aspx$", RegexOptions.IgnoreCase))
                {
                    if (tab != null)
                    {
                        str = this.GetFriendlyAlias("~/" + tab.TabPath.Replace("//", "/").TrimStart(new char[] { Convert.ToChar("/") }) + ".aspx", portalAlias, isPagePath);
                    }
                }
                else
                {
                    Regex regex = new Regex(@"[^?]*/Tab[Ii]d/(\d+)/ctl/([A-Z][a-z]+)/default.aspx(\?returnurl=([^>]+))?$", RegexOptions.IgnoreCase);
                    if (!regex.IsMatch(str))
                    {
                        return str;
                    }
                    match = regex.Match(str);
                    if (match.Groups.Count <= 2)
                    {
                        return str;
                    }
                    str3 = match.Groups[2].Value.ToLower();
                    string str5 = str3;
                    if (str5 == null)
                    {
                        goto Label_0273;
                    }
                    if (!(str5 == "terms"))
                    {
                        if (str5 == "privacy")
                        {
                            str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx", portalAlias, isPagePath);
                            goto Label_032B;
                        }
                        if (str5 == "login")
                        {
                            if (match.Groups[4].Value.ToLower() != "")
                            {
                                str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx?ReturnUrl=" + match.Groups[4].Value, portalAlias, isPagePath);
                            }
                            else
                            {
                                str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx", portalAlias, isPagePath);
                            }
                            goto Label_032B;
                        }
                        goto Label_0273;
                    }
                    str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx", portalAlias, isPagePath);
                }
            }
            goto Label_032B;
        Label_0273:
            if (str3 != "register")
            {
                return str;
            }
            if (match.Groups[4].Value.ToLower() != "")
            {
                str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx?ReturnUrl=" + match.Groups[4].Value, portalAlias, isPagePath);
            }
            else
            {
                str = this.GetFriendlyAlias("~/" + match.Groups[2].Value + ".aspx", portalAlias, isPagePath);
            }
        Label_032B:
            return this.CheckPathLength(str, path);
        }

        private string GetFriendlyAlias(string path, string portalAlias, bool IsPagePath)
        {
            string url = path;
            string newValue = "";
            if ((portalAlias != Null.NullString) && (HttpContext.Current.Items["UrlRewrite:OriginalUrl"] != null))
            {
                string input = HttpContext.Current.Items["UrlRewrite:OriginalUrl"].ToString();
                if (Regex.Match(input, "^" + Globals.AddHTTP(portalAlias), RegexOptions.IgnoreCase) != Match.Empty)
                {
                    newValue = Globals.AddHTTP(portalAlias);
                }
                if ((newValue == "") && (Regex.Match(input, "^?alias=" + portalAlias, RegexOptions.IgnoreCase) != Match.Empty))
                {
                    newValue = Globals.AddHTTP(portalAlias);
                }
                if (!(!(newValue == "") || (HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(url)).Contains(portalAlias)))
                {
                    newValue = Globals.AddHTTP(portalAlias);
                }
                if ((newValue == "") && (Regex.Match(input, "^" + Globals.AddHTTP("www." + portalAlias), RegexOptions.IgnoreCase) != Match.Empty))
                {
                    newValue = Globals.AddHTTP("www." + portalAlias);
                }
            }
            if (newValue != "")
            {
                if (path.IndexOf("~") != -1)
                {
                    url = url.Replace("~", newValue);
                }
                else
                {
                    url = newValue + url;
                }
            }
            else
            {
                url = Globals.ResolveUrl(url);
            }
            if (url.StartsWith("//") & IsPagePath)
            {
                url = url.Substring(1);
            }
            return url;
        }

        private string GetFriendlyQueryString(TabInfo tab, string path, string pageName)
        {
            string input = path;
            Match match = Regex.Match(input, @"(.[^\\?]*)\\?(.*)", RegexOptions.IgnoreCase);
            string str2 = "";
            if (match != Match.Empty)
            {
                input = Regex.Replace(match.Groups[1].Value, "Default.aspx", "", RegexOptions.IgnoreCase);
                string str3 = match.Groups[2].Value.Replace("&amp;", "&");
                if (str3.StartsWith("?"))
                {
                    str3 = str3.TrimStart(new char[] { Convert.ToChar("?") });
                }
                string[] strArray = str3.Split(new char[] { Convert.ToChar("&") });
                int num = strArray.Length - 1;
                for (int i = 0; i <= num; i++)
                {
                    string str4 = "";
                    string[] strArray2 = strArray[i].Split(new char[] { Convert.ToChar("=") });
                    if (!this._addNothing)
                    {
                        if (input.EndsWith("/"))
                        {
                            str4 = str4 + strArray2[0];
                        }
                        else
                        {
                            str4 = str4 + "/" + strArray2[0];
                        }
                    }
                    if ((strArray2.Length > 1) && !this._addNothing)
                    {
                        if (strArray2[1].Length > 0)
                        {
                            if (!Regex.IsMatch(strArray2[1], this._regexMatch))
                            {
                                if (((strArray2[0].ToLower() == "tabid") && Versioned.IsNumeric(strArray2[1])) && (tab != null))
                                {
                                    int num3 = Convert.ToInt32(strArray2[1]);
                                    if ((tab.TabID == num3) && ((tab.TabPath != Null.NullString) & this.IncludePageName))
                                    {
                                        str4 = tab.TabPath.Replace("//", "/").TrimStart(new char[] { '/' }) + "/" + str4;
                                    }
                                }
                                str4 = str4 + "/" + HttpUtility.UrlPathEncode(strArray2[1]);
                            }
                            else
                            {
                                if (str2.Length == 0)
                                {
                                    str2 = strArray2[0] + "=" + strArray2[1];
                                }
                                else
                                {
                                    str2 = str2 + "&" + strArray2[0] + "=" + strArray2[1];
                                }
                                str4 = "";
                            }
                        }
                        else
                        {
                            char ch = ' ';
                            str4 = str4 + "/" + HttpUtility.UrlPathEncode(ch.ToString());
                        }
                    }
                    input = input + str4;
                }
            }
            if (str2.Length > 0)
            {
                return (this.AddPage(input, pageName) + "?" + str2);
            }
            return this.AddPage(input, pageName);
        }

        private string GetUrl(TabInfo tab, string Path)
        {
            if (tab != null)
            {
                if (Path.Split(new char[] { '&' }).Length > 1)
                {
                    return "";
                }
                RewriterConfiguration config = RewriterConfiguration.GetConfig();
                foreach (RewriterRule rule in config.Rules)
                {
                    if (rule.SendTo.ToLower().IndexOf(string.Format("tabid={0}", tab.TabID)) >= 0)
                    {
                        return rule.LookFor.Replace(".*/", "");
                    }
                }
            }
            return "";
        }

        public string FileExtension
        {
            get
            {
                return this._fileExtension;
            }
        }

        public bool IncludePageName
        {
            get
            {
                return this._includePageName;
            }
        }

        public string RegexMatch
        {
            get
            {
                return this._regexMatch;
            }
        }

        public UrlFormatType UrlFormat
        {
            get
            {
                return this._urlFormat;
            }
        }
    }
}

