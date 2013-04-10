// <copyright file="PermissionController.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2013
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.UI.Modules;

    /// <summary>
    /// Answers whether the current user can do certain things
    /// </summary>
    public class PermissionController
    {
        /// <summary>
        /// The permission code for Engage: Employment
        /// </summary>
        private const string PermissionCode = "ENGAGE_EMPLOYMENT";

        /// <summary>
        /// The permission key for accessing the job detail options
        /// </summary>
        private const string ManageJobDetailOptionsPermissionKey = "MANAGE-JOB-DETAIL-OPTIONS";

        /// <summary>
        /// The permission key for accessing the applications
        /// </summary>
        private const string ManageApplicationsPermissionKey = "MANAGE-APPLICATIONS";

        /// <summary>
        /// The permission key for creating and editing jobs and job components
        /// </summary>
        private const string ManageJobsPermissionKey = "MANAGE-JOBS";

        /// <summary>
        /// The permission key for accessing the job listing options
        /// </summary>
        private const string ManageJobListingOptionsPermissionKey = "MANAGE-JOB-LISTING-OPTIONS";

        /// <summary>
        /// Provides the ability to create new permissions
        /// </summary>
        private readonly DotNetNuke.Security.Permissions.PermissionController permissionController;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionController"/> class.
        /// </summary>
        public PermissionController() 
        {
            this.permissionController = new DotNetNuke.Security.Permissions.PermissionController();
        }

        /// <summary>
        /// Determines whether the current user can manage applications for the specified module.
        /// </summary>
        /// <param name="moduleControl">The module control.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="moduleControl"/> is <c>null</c></exception>
        /// <returns>
        /// <c>true</c> if the current user can manage applications for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageApplications(IModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException("moduleControl");
            }

            return CanManageApplications(moduleControl.ModuleContext.Configuration);
        }

        /// <summary>
        /// Determines whether the current user can manage applications for the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module.</param>
        /// <returns>
        /// <c>true</c> if the current user can manage applications for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageApplications(ModuleInfo moduleInfo)
        {
            return HasModulePermission(moduleInfo, ManageApplicationsPermissionKey);
        }

        /// <summary>
        /// Determines whether the current user can manage jobs for the specified module.
        /// </summary>
        /// <param name="moduleControl">The module control.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="moduleControl"/> is <c>null</c></exception>
        /// <returns>
        /// <c>true</c> if the current user can manage jobs for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobs(IModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException("moduleControl");
            }

            return CanManageJobs(moduleControl.ModuleContext.Configuration);
        }

        /// <summary>
        /// Determines whether the current user can manage jobs for the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module.</param>
        /// <returns>
        /// <c>true</c> if the current user can manage jobs for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobs(ModuleInfo moduleInfo)
        {
            return HasModulePermission(moduleInfo, ManageJobsPermissionKey);
        }

        /// <summary>
        /// Determines whether the current user can manage job listing options for the specified module.
        /// </summary>
        /// <param name="moduleControl">The module control.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="moduleControl"/> is <c>null</c></exception>
        /// <returns>
        /// <c>true</c> if the current user can manage job listing options for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobListingOptions(IModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException("moduleControl");
            }

            return CanManageJobListingOptions(moduleControl.ModuleContext.Configuration);
        }

        /// <summary>
        /// Determines whether the current user can manage job listing options for the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module.</param>
        /// <returns>
        /// <c>true</c> if the current user can manage job listing options for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobListingOptions(ModuleInfo moduleInfo)
        {
            return HasModulePermission(moduleInfo, ManageJobListingOptionsPermissionKey);
        }

        /// <summary>
        /// Determines whether the current user can manage job detail options for the specified module.
        /// </summary>
        /// <param name="moduleControl">The module control.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="moduleControl"/> is <c>null</c></exception>
        /// <returns>
        /// <c>true</c> if the current user can manage job detail options for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobDetailOptions(IModuleControl moduleControl)
        {
            if (moduleControl == null)
            {
                throw new ArgumentNullException("moduleControl");
            }

            return CanManageJobDetailOptions(moduleControl.ModuleContext.Configuration);
        }

        /// <summary>
        /// Determines whether the current user can manage job detail options for the specified module.
        /// </summary>
        /// <param name="moduleInfo">The module.</param>
        /// <returns>
        /// <c>true</c> if the current user can manage job detail options for the specified module; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanManageJobDetailOptions(ModuleInfo moduleInfo)
        {
            return HasModulePermission(moduleInfo, ManageJobDetailOptionsPermissionKey);
        }

        /// <summary>
        /// Creates the custom permissions for Engage: Employment.
        /// </summary>
        public void CreateCustomPermissions()
        {
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(EmploymentController.DesktopModuleName, Null.NullInteger);
            var jobListingModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(ModuleDefinition.JobListing.ToString(), desktopModule.DesktopModuleID);
            var jobDetailModuleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(ModuleDefinition.JobDetail.ToString(), desktopModule.DesktopModuleID);

            this.permissionController.AddPermission(new PermissionInfo
            {
                ModuleDefID = jobListingModuleDefinition.ModuleDefID,
                PermissionCode = PermissionCode,
                PermissionKey = ManageApplicationsPermissionKey,
                PermissionName = "Manage Applications"
            });

            this.permissionController.AddPermission(new PermissionInfo
            {
                ModuleDefID = jobListingModuleDefinition.ModuleDefID,
                PermissionCode = PermissionCode,
                PermissionKey = ManageJobsPermissionKey,
                PermissionName = "Manage Jobs"
            });

            this.permissionController.AddPermission(new PermissionInfo
            {
                ModuleDefID = jobListingModuleDefinition.ModuleDefID,
                PermissionCode = PermissionCode,
                PermissionKey = ManageJobListingOptionsPermissionKey,
                PermissionName = "Manage Job Listing Options"
            });

            this.permissionController.AddPermission(new PermissionInfo
            {
                ModuleDefID = jobDetailModuleDefinition.ModuleDefID,
                PermissionCode = PermissionCode,
                PermissionKey = ManageJobDetailOptionsPermissionKey,
                PermissionName = "Manage Job Detail Options"
            });
        }

        /// <summary>
        /// Determines whether the user has the permission to the module.
        /// </summary>
        /// <param name="moduleInfo">The module.</param>
        /// <param name="permissionKey">The permission's key</param>
        /// <returns>
        ///   <c>true</c> if the user has the permission to the module; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasModulePermission(ModuleInfo moduleInfo, string permissionKey)
        {
            return ModulePermissionController.CanAdminModule(moduleInfo) || ModulePermissionController.HasModulePermission(ModulePermissionController.GetModulePermissions(moduleInfo.ModuleID, moduleInfo.TabID), permissionKey);
        }
    }
}