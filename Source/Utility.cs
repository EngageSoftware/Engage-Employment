//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Xml.XPath;
    using Data;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;

    internal static class Utility
    {
        public const string ApplicationStatusListName = "EngageEmployment:ApplicationStatus";
        public const string DesktopModuleRelativePath = "~/DesktopModules/EngageEmployment/";
        public const string EnableDnnSearchSetting = "EnableDnnSearch";
        public const string JobGroupIdSetting = "JobGroupId";
        public const string LeadListName = "EngageEmployment:Lead";
        public const string UserStatusPropertyName = "EngageEmployment_UserStatus";

        /// <summary>
        /// Splits <paramref name="text"/> at each space, unless the space is wrapped in quotes
        /// </summary>
        /// <remarks>based on http://www.devx.com/vb2themax/Tip/19420 </remarks>
        /// <param name="text">The text value to be split</param>
        /// <returns>A list of the words in <paramref cref="text"/></returns>
        public static List<string> SplitQuoted(string text)
        {
            List<string> res = new List<string>();
            // notice that the quoted element is defined by group #2 
            // and the unquoted element is defined by group #3
            foreach (Match m in Regex.Matches(text, @"\s*(""([^""]*)""|([^\s]+))\s*"))
            {
                // get a reference to the unquoted element, if it's there
                string g3 = m.Groups[3].Value;
                if (!string.IsNullOrEmpty(g3))
                {
                    // if the 3rd group is not null, then the element wasn't quoted
                    res.Add(g3);
                }
                else
                {
                    // get the quoted string, but without the quotes
                    res.Add(m.Groups[2].Value);
                }
            }
            return res;
        }

        public static List<string> RemoveCommonWords(List<string> words)
        {
            List<string> list = new List<string>(words);
            List<string> commonWords = GetCommonWords();

            list.RemoveAll(delegate(string word) { return commonWords.Contains(word); });

            return list;
        }

        private static List<string> GetCommonWords()
        {
            List<string> commonWords = new List<string>();

            using (IDataReader dr = DataProvider.Instance().GetCommonWords())
            {
                while (dr.Read())
                {
                    commonWords.Add((string)dr[0]);
                }
            }

            return commonWords;
        }

        /// <summary>
        /// Gets an instance of the Employment module with the given <paramref name="moduleDefinition"/> and <paramref name="jobGroupId"/> which can be viewed by the current user.
        /// </summary>
        /// <param name="portalSettings">The portal id.</param>
        /// <param name="moduleDefinition">The module definition.</param>
        /// <param name="jobGroupId">ID of the Job Group displayed on the module.</param>
        /// <returns>A non-deleted <see cref="ModuleInfo"/> with the given <paramref name="moduleDefinition"/>, or null if none exists.</returns>
        public static ModuleInfo GetCurrentModuleByDefinition(PortalSettings portalSettings, ModuleDefinition moduleDefinition, int? jobGroupId)
        {
            ModuleInfo bestModule = null;
            ModuleController modules = new ModuleController();

            foreach (ModuleInfo module in modules.GetModulesByDefinition(portalSettings.PortalId, moduleDefinition.ToString()))
            {
                if (!module.IsDeleted)
                {
                    TabInfo tab = (new TabController()).GetTab(module.TabID, portalSettings.PortalId, false);
                    if (tab != null && !tab.IsDeleted && PortalSecurity.HasNecessaryPermission(SecurityAccessLevel.View, portalSettings, module))
                    {
                        bestModule = bestModule ?? module; //set the value if it is null, otherwise leave it the same

                        int? moduleJobGroupSetting = Dnn.Utility.GetIntSetting(modules.GetTabModuleSettings(module.TabModuleID), JobGroupIdSetting);
                        if (jobGroupId.Equals(moduleJobGroupSetting))
                        {
                            //if it's the right Job Group, return this one now; otherwise, keep it around, but see if we can find a better match.  BD
                            bestModule = module;
                            break;
                        }
                        else if (!moduleJobGroupSetting.HasValue)
                        {
                            bestModule = module; //a module without a jobGroupId beats out one with the wrong jobGroupId.  BD
                        }
                    }
                }
            }

            return bestModule;
        }

        /// <summary>
        /// Gets the URL to retrieve a particular document for an application.
        /// </summary>
        /// <param name="request">The current web request.</param>
        /// <param name="documentId">The resumé id.</param>
        /// <returns>The URL used to retrieve the document.</returns>
        public static string GetDocumentUrl(HttpRequest request, int documentId)
        {
            return new Uri(request.Url, Globals.ResolveUrl(DesktopModuleRelativePath + "GetResume.aspx?rid=" + documentId.ToString(CultureInfo.InvariantCulture))).AbsoluteUri;
        }

        public static int GetSearchResultsTabId(int? jobGroupId, PortalSettings portalSettings)
        {
            ModuleInfo mi = GetCurrentModuleByDefinition(portalSettings, ModuleDefinition.JobSearch, jobGroupId);
            return mi == null ? -1 : mi.TabID;
        }

        public static int GetJobDetailTabId(int? jobGroupId, PortalSettings portalSettings)
        {
            ModuleInfo mi = GetCurrentModuleByDefinition(portalSettings, ModuleDefinition.JobDetail, jobGroupId);
            return mi == null ? -1 : mi.TabID;
        }

        public static int? GetJobListingTabId(int? jobGroupId, PortalSettings portalSettings)
        {
            ModuleInfo mi = GetCurrentModuleByDefinition(portalSettings, ModuleDefinition.JobListing, jobGroupId);
            return mi == null ? (int?)null : mi.TabID;
        }

        public static string GetJobDetailUrl(object jobId, int? jobGroupId, PortalSettings portalSettings)
        {
            if (jobId is int)
            {
                return Globals.NavigateURL(
                    GetJobDetailTabId(jobGroupId, portalSettings), string.Empty, "jobid=" + ((int)jobId).ToString(CultureInfo.InvariantCulture));
            }

            return Globals.NavigateURL();
        }

        public static void GetRandomSelection(IList collection, int maximumNumber)
        {
            if (collection.Count <= maximumNumber)
            {
                return;
            }

            Random random = Engage.Utility.GetRandomNumberGenerator();

            while (collection.Count > maximumNumber)
            {
                int i = random.Next(0, collection.Count);
                collection.RemoveAt(i);
            }
        }

        public static string GetMaxLengthValidationExpression(int length)
        {
            // validate that status length is no longer than StatusMaxLength
            return ".{1," + length.ToString(CultureInfo.InvariantCulture) + "}";
        }

        #region Localization

        public static string GetString(string name, string resourceFileRoot, int portalId)
        {
            return GetString(name, resourceFileRoot, portalId, null, false);
        }

        //public static string GetString(string name, string resourceFileRoot, int portalId, bool disableShowMissingKeys)
        //{
        //    return GetString(name, resourceFileRoot, portalId, null, disableShowMissingKeys);
        //}

        //public static string GetString(string name, string resourceFileRoot, int portalId, string requestedResourceLanguage)
        //{
        //    return GetString(name, resourceFileRoot, portalId, requestedResourceLanguage, false);
        //}

        public static string GetString(string name, string resourceFileRoot, int portalId, string requestedResourceLanguage, bool disableShowMissingKeys)
        {
            if (!Engage.Utility.HasValue(name))
            {
                return string.Empty;
            }
            string portalDefaultLangauge = null;
            if (!Null.IsNull(portalId))
            {
                portalDefaultLangauge = (new PortalController()).GetPortal(portalId).DefaultLanguage;
            }
            IDictionary<string, string> resources = GetResource(resourceFileRoot, portalDefaultLangauge, portalId, requestedResourceLanguage);

            // make the default translation property ".Text"
            if (name.IndexOf(".", StringComparison.Ordinal) < 1)
            {
                name += ".Text";
            }

            // If the key can't be found try the Local Shared Resource File resources
            if (resourceFileRoot != null && (resources == null || !resources.ContainsKey(name)))
            {
                // try to use a module specific shared resource file
                string localSharedFile = resourceFileRoot.Substring(0, resourceFileRoot.LastIndexOf("/", StringComparison.Ordinal) + 1)
                                         + Localization.LocalSharedResourceFile;
                resources = GetResource(localSharedFile, portalDefaultLangauge, portalId, null);
            }

            // If the key can't be found try the Shared Resource Files resources
            if (resources == null || !resources.ContainsKey(name))
            {
                resources = GetResource(Localization.SharedResourceFile, portalDefaultLangauge, portalId, null);
            }

            // If the key still can't be found then it doesn't exist in the Localization Resources
            if (Localization.ShowMissingKeys && !disableShowMissingKeys)
            {
                if (resources == null || !resources.ContainsKey(name))
                {
                    return "RESX:" + name;
                }
                else
                {
                    return "[L]" + resources[name];
                }
            }
            return resources[name];
        }

        private static IDictionary<string, string> GetResource(string resourceFileRoot, string defaultLanguage, int portalId, string requestedResourceLanguage)
        {
            defaultLanguage = defaultLanguage.ToUpperInvariant();
            string fallbackLanguage = Localization.SystemLocale.ToUpperInvariant();
            string userLanguage = requestedResourceLanguage ?? Thread.CurrentThread.CurrentCulture.ToString().ToUpperInvariant();

            //  Ensure the user has a language set
            if (!Engage.Utility.HasValue(userLanguage))
            {
                userLanguage = defaultLanguage;
            }

            Locale userLocale = Localization.GetSupportedLocales()[userLanguage];
            if (userLocale != null && Engage.Utility.HasValue(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback.ToUpperInvariant();
            }

            // Get the filename for the userLanguage version of the resource file
            string userFile = GetResourceFileName(resourceFileRoot, userLanguage);

            // Set the cachekey as the userFile + the PortalId
            string cacheKey = userFile.Replace("~/", "/").ToUpperInvariant() + portalId.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(Globals.ApplicationPath))
            {
                cacheKey = cacheKey.Replace(Globals.ApplicationPath, string.Empty);
            }

            // Attempt to get the resources from the cache
            IDictionary<string, string> resources = DataCache.GetCache(cacheKey) as IDictionary<string, string>;
            if (resources == null)
            {
                // resources not in Cache so load from Files
                // Create resources dictionary
                resources = new Dictionary<string, string>();
                // First Load the Fallback Language ensuring that the keys are loaded
                string fallbackFile = GetResourceFileName(resourceFileRoot, fallbackLanguage);
                resources = LoadResource(resources, cacheKey, fallbackFile, CustomizedLocale.None, portalId);
                //  Add any host level customizations
                resources = LoadResource(resources, cacheKey, fallbackFile, CustomizedLocale.Host, portalId);
                //  Add any portal level customizations
                resources = LoadResource(resources, cacheKey, fallbackFile, CustomizedLocale.Portal, portalId);

                // if the defaultLanguage is different, load it
                if (!string.IsNullOrEmpty(defaultLanguage) && defaultLanguage != fallbackLanguage && userLanguage != fallbackLanguage)
                {
                    string defaultFile = GetResourceFileName(resourceFileRoot, defaultLanguage);
                    resources = LoadResource(resources, cacheKey, defaultFile, CustomizedLocale.None, portalId);
                    //  Add any host level customizations
                    resources = LoadResource(resources, cacheKey, defaultFile, CustomizedLocale.Host, portalId);
                    //  Add any portal level customizations
                    resources = LoadResource(resources, cacheKey, defaultFile, CustomizedLocale.Portal, portalId);
                }
                //  If the user language is different load it
                if (userLanguage != defaultLanguage && userLanguage != fallbackLanguage)
                {
                    resources = LoadResource(resources, cacheKey, userFile, CustomizedLocale.None, portalId);
                    //  Add any host level customizations
                    resources = LoadResource(resources, cacheKey, userFile, CustomizedLocale.Host, portalId);
                    //  Add any portal level customizations
                    resources = LoadResource(resources, cacheKey, userFile, CustomizedLocale.Portal, portalId);
                }
            }
            return resources;
        }

        private static string GetResourceFileName(string resourceFileRoot, string language)
        {
            string resourceFileName;
            if (resourceFileRoot != null)
            {
                if (language == Localization.SystemLocale.ToUpperInvariant() || string.IsNullOrEmpty(language))
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5).ToUpperInvariant())
                    {
                        case ".RESX":
                            resourceFileName = resourceFileRoot;
                            break;
                        case ".ASCX":
                        case ".ASPX":
                            resourceFileName = resourceFileRoot + ".resx";
                            break;
                        default:
                            resourceFileName = resourceFileRoot + ".ascx.resx";
                            break;
                    }
                }
                else
                {
                    switch (resourceFileRoot.Substring(resourceFileRoot.Length - 5).ToUpperInvariant())
                    {
                        case ".RESX":
                            resourceFileName = resourceFileRoot.Replace(".RESX", "." + language + ".resx");
                            break;
                        case ".ASCX":
                        case ".ASPX":
                            resourceFileName = resourceFileRoot + "." + language + ".resx";
                            break;
                        default:
                            resourceFileName = resourceFileRoot + ".ascx." + language + ".resx";
                            break;
                    }
                }
            }
            else if (language == Localization.SystemLocale.ToUpperInvariant() || string.IsNullOrEmpty(language))
            {
                resourceFileName = Localization.SharedResourceFile;
            }
            else
            {
                resourceFileName = Localization.SharedResourceFile.Replace(".RESX", "." + language + ".resx");
            }
            return resourceFileName;
        }

        // ReSharper disable SuggestBaseTypeForParameter
        private static IDictionary<string, string> LoadResource(IDictionary<string, string> resources, string cacheKey, string ResourceFile,
                                                                CustomizedLocale CheckCustomCulture, int portalId)
            // ReSharper restore SuggestBaseTypeForParameter
        {
            string f = null;
            // Are we looking for customised resources
            switch (CheckCustomCulture)
            {
                case CustomizedLocale.None:
                    f = ResourceFile;
                    break;
                case CustomizedLocale.Portal:
                    f = ResourceFile.Replace(".RESX", ".Portal-" + portalId.ToString(CultureInfo.InvariantCulture) + ".resx");
                    break;
                case CustomizedLocale.Host:
                    f = ResourceFile.Replace(".RESX", ".Host.resx");
                    break;
            }
            // If the filename is empty or the file does not exist return the dictionary
            string filePath = HostingEnvironment.MapPath(f);
            if (f == null || !File.Exists(filePath))
            {
                return resources;
            }

            bool xmlLoaded;
            XPathDocument doc = null;
            CacheDependency dp = new CacheDependency(filePath);
            try
            {
                // ReSharper disable AssignNullToNotNullAttribute
                doc = new XPathDocument(filePath);
                // ReSharper restore AssignNullToNotNullAttribute
                xmlLoaded = true;
            }
            catch
            {
                xmlLoaded = false;
            }

            if (xmlLoaded)
            {
                foreach (XPathNavigator nav in doc.CreateNavigator().Select("root/data"))
                {
                    if (nav.NodeType != XPathNodeType.Comment)
                    {
                        resources[nav.GetAttribute("name", string.Empty)] = nav.SelectSingleNode("value").Value;
                    }
                }
                try
                {
                    int cacheMinutes = 3 * Convert.ToInt32(Globals.PerformanceSetting, CultureInfo.InvariantCulture);
                    if (cacheMinutes > 0)
                    {
                        DataCache.SetCache(cacheKey, resources, dp, DateTime.MaxValue, new TimeSpan(0, cacheMinutes, 0));
                    }
                }
#pragma warning disable 1692
#pragma warning disable EmptyGeneralCatchClause
                catch
                {
                }
#pragma warning restore EmptyGeneralCatchClause
#pragma warning restore 1692
            }
            return resources;
        }

        private enum CustomizedLocale
        {
            None = 0,
            Portal = 1,
            Host = 2
        }

        #endregion
    }

    public enum ControlKey
    {
        Edit = 0,
        EditJob,
        [Obsolete("User statuses are now edited inline on the ManageApplications page, this control does not exist", true)]
        EditUserStatus,
        ManageApplications,
        ManageCategories,
        ManageLocations,
        ManagePositions,
        ManageStates,
        ManageStatuses,
        Options,
        ManageApplicationStatuses
    }

    public enum ModuleDefinition
    {
        JobListing = 0,
        JobSearch,
        JobDetail
    }

    public enum Visibility
    {
        Hidden = 0,
        Optional,
        Required
    }

    public enum LocationType
    {
        City = 0,
        State
    }
}