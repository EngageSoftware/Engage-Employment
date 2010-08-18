// <copyright file="GetResume.aspx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
// Copyright (c) 2004-2010
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
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;

    /// <summary>
    /// Displays the document with the given ID, assuming that the user is authenticated.  Otherwise redirects to login page.
    /// </summary>
    public partial class GetResume : PageBase
    {
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
                        UserInfo requestingUser = UserController.GetCurrentUserInfo();
                        if (this.UserHasPermissionToViewDocument((int)documentReader["DocumentId"], documentReader["UserId"] as int?, requestingUser))
                        {
                            this.WriteDocumentContent(documentReader);
                            return;
                        }
                        else
                        {
                            if (requestingUser.UserID == Null.NullInteger)
                            {
                                this.Response.Redirect(Dnn.Utility.GetLoginUrl(this.PortalSettings, this.Request));
                                return;
                            }
                        }
                    }
                }
            }

            this.Response.Redirect(Globals.NavigateURL("Access Denied"));
        }

        /// <summary>
        /// Gets a list of IDs of the job groups which the <paramref name="requestingUser"/> has permission to edit (and therefore to view documents for).
        /// </summary>
        /// <param name="requestingUser">The user making this request.</param>
        /// <returns>A list of IDs for the job groups that the <paramref name="requestingUser"/> can view documents for.</returns>
        private IList<int?> GetPermissibleJobGroups(UserInfo requestingUser)
        {
            var permissibleJobGroups = new List<int?>();
            if (requestingUser.IsSuperUser)
            {
                permissibleJobGroups.Add(null);
            }
            else
            {
                // TODO: if user is in multiple portals, this might need to account for that
                var moduleController = new ModuleController();
                foreach (ModuleInfo module in moduleController.GetModulesByDefinition(requestingUser.PortalID, ModuleDefinition.JobListing.ToString()))
                {
                    int? jobGroupId = ModuleSettings.JobGroupId.GetValueAsInt32For(EmploymentController.DesktopModuleName, module, ModuleSettings.JobGroupId.DefaultValue);
                    if (!permissibleJobGroups.Contains(jobGroupId))
                    {
                        if (PortalSecurity.HasNecessaryPermission(SecurityAccessLevel.Edit, this.PortalSettings, module, requestingUser))
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
        /// <param name="requestingUser">The user making this request.</param>
        /// <returns>
        /// <c>true</c> if the current user has permission to view the requested document; otherwise <c>false</c>.
        /// </returns>
        private bool UserHasPermissionToViewDocument(int documentId, int? documentUserId, UserInfo requestingUser)
        {
            if (documentUserId.HasValue && documentUserId.Value == requestingUser.UserID)
            {
                return true;
            }

            IList<int?> permissibleJobGroups = this.GetPermissibleJobGroups(requestingUser);
            if (permissibleJobGroups.Contains(null))
            {
                return true;
            }

            if (permissibleJobGroups.Count > 0)
            {
                foreach (int jobGroup in Document.GetDocumentJobGroups(documentId))
                {
                    if (permissibleJobGroups.Contains(jobGroup))
                    {
                        return true;
                    }
                }
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