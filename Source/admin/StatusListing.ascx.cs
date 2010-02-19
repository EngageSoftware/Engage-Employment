// <copyright file="StatusListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

namespace Engage.Dnn.Employment.Admin
{
    partial class StatusListing : ModuleBase
    {
        private const int StatusMaxLength = 255;

        protected string MaxLengthValidationText
        {
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("StatusMaxLength", LocalResourceFile), StatusMaxLength); }
        }

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(StatusMaxLength); }
        }

        protected override void OnInit(EventArgs e)
        {
            this.Load += Page_Load;
            btnAdd.Click += btnAdd_Click;
            btnBack.Click += btnBack_Click;
            btnCancelNew.Click += btnCancelNew_Click;
            btnSaveNew.Click += btnSaveNew_Click;
            gvStatuses.RowCancelingEdit += gvStatuses_RowCancelingEdit;
            gvStatuses.RowCommand += gvStatuses_RowCommand;
            gvStatuses.RowDataBound += gvStatuses_RowDataBound;
            gvStatuses.RowDeleting += gvStatuses_RowDeleting;
            gvStatuses.RowEditing += gvStatuses_RowEditing;

            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Page_Load(Object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Engage.Dnn.Utility.LocalizeGridView(ref gvStatuses, LocalResourceFile);
                    SetupStatusLengthValidation();
                    BindData();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl(ControlKey.ManageApplications.ToString()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnAdd_Click(object sender, EventArgs e)
        {
            pnlNew.Visible = true;
            txtNewStatus.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnSaveNew_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (IsStatusNameUnique(null, txtNewStatus.Text))
                {
                    UserStatus.InsertStatus(txtNewStatus.Text, PortalId);
                    HideAndClearNewStatusPanel();
                    BindData();
                }
                else
                {
                    cvDuplicateUserStatus.IsValid = false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnCancelNew_Click(object sender, EventArgs e)
        {
            HideAndClearNewStatusPanel();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvStatuses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvStatuses.EditIndex = -1;
            BindData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvStatuses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    Button btnDelete = (Button)row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        int? statusId = GetStatusId(row);
                        if (statusId.HasValue && UserStatus.IsStatusUsed(statusId.Value))
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(CultureInfo.CurrentCulture, "return confirm('{0}');", ClientAPI.GetSafeJSString(Localization.GetString("DeleteConfirm", LocalResourceFile)));
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvStatuses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? statusId = GetStatusId(e.RowIndex);
            if (statusId.HasValue)
            {
                UserStatus.DeleteStatus(statusId.Value);
                BindData();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvStatuses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvStatuses.EditIndex = e.NewEditIndex;
            HideAndClearNewStatusPanel();
            BindData();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvStatuses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? statusId = GetStatusId(rowIndex);
                        if (statusId.HasValue)
                        {
                            string newStatusName = GetStatusName(rowIndex);
                            if (IsStatusNameUnique(statusId, newStatusName))
                            {
                                UserStatus.UpdateStatus(statusId.Value, newStatusName);
                                gvStatuses.EditIndex = -1;
                                BindData();
                            }
                            else
                            {
                                cvDuplicateUserStatus.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        private void BindData()
        {
            List<UserStatus> statuses = UserStatus.LoadStatuses(PortalId);
            this.gvStatuses.DataSource = statuses;
            this.gvStatuses.DataBind();

            if (statuses == null || statuses.Count % 2 == 0)
            {
                pnlNew.CssClass = gvStatuses.RowStyle.CssClass;
            }
            else
            {
                pnlNew.CssClass = gvStatuses.AlternatingRowStyle.CssClass;
            }

            rowNewHeader.Visible = (statuses == null || statuses.Count < 1);
        }

        private bool IsStatusNameUnique(int? statusId, string newStatusName)
        {
            int? newStatusId = UserStatus.GetStatusId(newStatusName, PortalId);
            return !newStatusId.HasValue || (statusId.HasValue && newStatusId.Value == statusId.Value);
        }

        private void SetupStatusLengthValidation()
        {
            this.regexNewUserStatus.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewUserStatus.ErrorMessage = MaxLengthValidationText;
        }

        private void HideAndClearNewStatusPanel()
        {
            this.pnlNew.Visible = false;
            this.txtNewStatus.Text = string.Empty;
        }

        private string GetStatusName(int rowIndex)
        {
            if (gvStatuses != null && gvStatuses.Rows.Count > rowIndex)
            {
                GridViewRow row = gvStatuses.Rows[rowIndex];
                TextBox txtStatus = row.FindControl("txtStatus") as TextBox;
                Debug.Assert(txtStatus != null);
                return txtStatus.Text;
            }
            return null;
        }

        private static int? GetStatusId(Control row)
        {
            HiddenField hdnStatusId = (HiddenField)row.FindControl("hdnStatusId");

            int statusId;
            if (hdnStatusId != null && int.TryParse(hdnStatusId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                return statusId;
            }
            return null;
        }

        private int? GetStatusId(int rowIndex)
        {
            if (gvStatuses != null && gvStatuses.Rows.Count > rowIndex)
            {
                return GetStatusId(gvStatuses.Rows[rowIndex]);
            }
            return null;
        }
    }
 }