// <copyright file="GetResume.aspx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
// Copyright (c) 2004-2011
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
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    /// <summary>
    /// Displays the document with the given ID, assuming that the user is authenticated.  Otherwise redirects to login page.
    /// </summary>
    public partial class GetResume : PageBase
    {
        /// <summary>
        /// Gets the current user's ID.
        /// </summary>
        private static int UserId
        {
            get { return UserInfo.UserID; }
        }

        /// <summary>
        /// Gets the current user's info.
        /// </summary>
        private static UserInfo UserInfo
        {
            get { return UserController.GetCurrentUserInfo(); }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            int resumeId;
            if (int.TryParse(this.Request.QueryString["rid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out resumeId))
            {
                using (IDataReader documentReader = JobApplication.GetDocument(resumeId))
                {
                    if (documentReader.Read())
                    {
                        if (this.UserHasPermissionToViewDocument((int)documentReader["DocumentId"], documentReader["UserId"] as int?))
                        {
                            this.WriteDocumentContent(documentReader);
                            return;
                        }
                        
                        if (UserId == Null.NullInteger)
                        {
                            this.Response.Redirect(Dnn.Utility.GetLoginUrl(this.PortalSettings, this.Request));
                            return;
                        }
                    }
                }
            }

            this.Response.Redirect(Globals.NavigateURL("Access Denied"));
        }

        /// <summary>
        /// Gets a list of IDs of the job groups which the current user has permission to edit (and therefore to view documents for).
        /// </summary>
        /// <returns>A list of IDs for the job groups that the current user can view documents for.</returns>
        private IList<int?> GetPermissibleJobGroups()
        {
            var permissibleJobGroups = new List<int?>();
            if (UserInfo.IsSuperUser)
            {
                permissibleJobGroups.Add(null);
            }
            else
            {
// GetModulesByDefinition's obsolete status was rescinded after DNN 5.0
#pragma warning disable 618

                var moduleController = new ModuleController();
                foreach (ModuleInfo module in moduleController.GetModulesByDefinition(this.PortalSettings.PortalId, ModuleDefinition.JobListing.ToString()))
                {
#pragma warning restore 618
                    int? jobGroupId = ModuleSettings.JobGroupId.GetValueAsInt32For(EmploymentController.DesktopModuleName, module, ModuleSettings.JobGroupId.DefaultValue);
                    if (!permissibleJobGroups.Contains(jobGroupId))
                    {
                        if (PermissionController.CanManageJobs(module))
                        {
                            permissibleJobGroups.Add(jobGroupId);

                            if (!jobGroupId.HasValue)
                            {
                                // if they have access to the "null" job group, they can see it all, no need to keep looking
                                break;
                            }
                        }
                    }
                }
            }

            return permissibleJobGroups;
        }

        /// <summary>
        /// Whether the current user has permission to view the requested document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <param name="documentUserId">The ID of the user who submitted the requested document.</param>
        /// <returns>
        /// <c>true</c> if the current user has permission to view the requested document; otherwise <c>false</c>.
        /// </returns>
        private bool UserHasPermissionToViewDocument(int documentId, int? documentUserId)
        {
            if (documentUserId.HasValue && documentUserId.Value == UserId)
            {
                return true;
            }

            var permissibleJobGroups = this.GetPermissibleJobGroups();
            if (permissibleJobGroups.Contains(null))
            {
                return true;
            }

            if (permissibleJobGroups.Count > 0)
            {
                return Document.GetDocumentJobGroups(documentId).Any(jobGroup => permissibleJobGroups.Contains(jobGroup));
            }

            return false;
        }

        /// <summary>
        /// Writes the content of the document out to the <see cref="PageBase.Response"/> stream.
        /// </summary>
        /// <param name="documentRecord">The information about the requested document.</param>
        private void WriteDocumentContent(IDataRecord documentRecord)
        {
            this.Response.ContentType = (string)documentRecord["ContentType"];
            this.Response.AddHeader("content-disposition", "filename=" + (string)documentRecord["filename"] + ";");
            this.Response.BinaryWrite((byte[])documentRecord["ResumeData"]);
            this.Response.Flush();
        }
    }
}