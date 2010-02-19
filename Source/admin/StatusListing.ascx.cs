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

namespace Engage.Dnn.Employment.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    public partial class StatusListing : ModuleBase
    {
        private const int StatusMaxLength = 255;

        protected static string MaxLengthValidationExpression
        {
            get
            {
                return Utility.GetMaxLengthValidationExpression(StatusMaxLength);
            }
        }

        protected string MaxLengthValidationText
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture, Localization.GetString("StatusMaxLength", this.LocalResourceFile), StatusMaxLength);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.Load += this.Page_Load;
            this.btnAdd.Click += this.btnAdd_Click;
            this.btnBack.Click += this.btnBack_Click;
            this.btnCancelNew.Click += this.btnCancelNew_Click;
            this.btnSaveNew.Click += this.btnSaveNew_Click;
            this.gvStatuses.RowCancelingEdit += this.gvStatuses_RowCancelingEdit;
            this.gvStatuses.RowCommand += this.gvStatuses_RowCommand;
            this.gvStatuses.RowDataBound += this.gvStatuses_RowDataBound;
            this.gvStatuses.RowDeleting += this.gvStatuses_RowDeleting;
            this.gvStatuses.RowEditing += this.gvStatuses_RowEditing;

            base.OnInit(e);
        }

        private static int? GetStatusId(Control row)
        {
            var hdnStatusId = (HiddenField)row.FindControl("hdnStatusId");

            int statusId;
            if (hdnStatusId != null && int.TryParse(hdnStatusId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                return statusId;
            }

            return null;
        }

        private void BindData()
        {
            List<UserStatus> statuses = UserStatus.LoadStatuses(this.PortalId);
            this.gvStatuses.DataSource = statuses;
            this.gvStatuses.DataBind();

            if (statuses == null || statuses.Count % 2 == 0)
            {
                this.pnlNew.CssClass = this.gvStatuses.RowStyle.CssClass;
            }
            else
            {
                this.pnlNew.CssClass = this.gvStatuses.AlternatingRowStyle.CssClass;
            }

            this.rowNewHeader.Visible = statuses == null || statuses.Count < 1;
        }

        private int? GetStatusId(int rowIndex)
        {
            if (this.gvStatuses != null && this.gvStatuses.Rows.Count > rowIndex)
            {
                return GetStatusId(this.gvStatuses.Rows[rowIndex]);
            }

            return null;
        }

        private string GetStatusName(int rowIndex)
        {
            if (this.gvStatuses != null && this.gvStatuses.Rows.Count > rowIndex)
            {
                GridViewRow row = this.gvStatuses.Rows[rowIndex];
                var txtStatus = (TextBox)row.FindControl("txtStatus");
                return txtStatus.Text;
            }

            return null;
        }

        private void HideAndClearNewStatusPanel()
        {
            this.pnlNew.Visible = false;
            this.txtNewStatus.Text = string.Empty;
        }

        private bool IsStatusNameUnique(int? statusId, string newStatusName)
        {
            int? newStatusId = UserStatus.GetStatusId(newStatusName, this.PortalId);
            return !newStatusId.HasValue || (statusId.HasValue && newStatusId.Value == statusId.Value);
        }

        private void SetupStatusLengthValidation()
        {
            this.regexNewUserStatus.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewUserStatus.ErrorMessage = this.MaxLengthValidationText;
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="btnAdd"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.pnlNew.Visible = true;
            this.txtNewStatus.Focus();
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="btnBack"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.ManageApplications.ToString()));
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="btnCancelNew"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnCancelNew_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewStatusPanel();
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="btnSaveNew"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSaveNew_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                if (this.IsStatusNameUnique(null, this.txtNewStatus.Text))
                {
                    UserStatus.InsertStatus(this.txtNewStatus.Text, this.PortalId);
                    this.HideAndClearNewStatusPanel();
                    this.BindData();
                }
                else
                {
                    this.cvDuplicateUserStatus.IsValid = false;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="gvStatuses"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void gvStatuses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.gvStatuses.EditIndex = -1;
            this.BindData();
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowCommand"/> event of the <see cref="gvStatuses"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        private void gvStatuses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (this.Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? statusId = GetStatusId(rowIndex);
                        if (statusId.HasValue)
                        {
                            string newStatusName = this.GetStatusName(rowIndex);
                            if (this.IsStatusNameUnique(statusId, newStatusName))
                            {
                                UserStatus.UpdateStatus(statusId.Value, newStatusName);
                                this.gvStatuses.EditIndex = -1;
                                this.BindData();
                            }
                            else
                            {
                                this.cvDuplicateUserStatus.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="gvStatuses"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
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
                            btnDelete.OnClientClick = string.Format(
                                    CultureInfo.CurrentCulture, 
                                    "return confirm('{0}');", 
                                    ClientAPI.GetSafeJSString(Localization.GetString("DeleteConfirm", this.LocalResourceFile)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="gvStatuses"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void gvStatuses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? statusId = GetStatusId(e.RowIndex);
            if (statusId.HasValue)
            {
                UserStatus.DeleteStatus(statusId.Value);
                this.BindData();
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowEditing"/> event of the <see cref="gvStatuses"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void gvStatuses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.gvStatuses.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatusPanel();
            this.BindData();
        }

        /// <summary>
        /// Handles the <see cref="Control.Load"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    Dnn.Utility.LocalizeGridView(ref this.gvStatuses, this.LocalResourceFile);
                    this.SetupStatusLengthValidation();
                    this.BindData();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}