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
    using System.Linq;
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
            this.AddButton.Click += this.AddButton_Click;
            this.BackButton.Click += this.BackButton_Click;
            this.CancelNewButton.Click += this.CancelNewButton_Click;
            this.SaveNewButton.Click += this.SaveNewButton_Click;
            this.StatusesGridView.RowCancelingEdit += this.StatusesGridView_RowCancelingEdit;
            this.StatusesGridView.RowCommand += this.StatusesGridView_RowCommand;
            this.StatusesGridView.RowDataBound += this.StatusesGridView_RowDataBound;
            this.StatusesGridView.RowDeleting += this.StatusesGridView_RowDeleting;
            this.StatusesGridView.RowEditing += this.StatusesGridView_RowEditing;

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
            var statuses = UserStatus.LoadStatuses(this.PortalId);
            this.StatusesGridView.DataSource = statuses;
            this.StatusesGridView.DataBind();

            this.NewPanel.CssClass = statuses.Count() % 2 == 0
                                         ? this.StatusesGridView.RowStyle.CssClass
                                         : this.StatusesGridView.AlternatingRowStyle.CssClass;

            this.rowNewHeader.Visible = !statuses.Any();
        }

        private int? GetStatusId(int rowIndex)
        {
            if (this.StatusesGridView != null && this.StatusesGridView.Rows.Count > rowIndex)
            {
                return GetStatusId(this.StatusesGridView.Rows[rowIndex]);
            }

            return null;
        }

        private string GetStatusName(int rowIndex)
        {
            if (this.StatusesGridView != null && this.StatusesGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.StatusesGridView.Rows[rowIndex];
                var txtStatus = (TextBox)row.FindControl("txtStatus");
                return txtStatus.Text;
            }

            return null;
        }

        private void HideAndClearNewStatusPanel()
        {
            this.NewPanel.Visible = false;
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
        /// Handles the <see cref="Button.Click"/> event of the <see cref="AddButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPanel.Visible = true;
            this.txtNewStatus.Focus();
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="BackButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.ManageApplications.ToString()));
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="CancelNewButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelNewButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewStatusPanel();
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="SaveNewButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveNewButton_Click(object sender, EventArgs e)
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
        /// Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="StatusesGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void StatusesGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.StatusesGridView.EditIndex = -1;
            this.BindData();
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowCommand"/> event of the <see cref="StatusesGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        private void StatusesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
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
                                this.StatusesGridView.EditIndex = -1;
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
        /// Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="StatusesGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void StatusesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    var btnDelete = (Button)row.FindControl("btnDelete");
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
        /// Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="StatusesGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void StatusesGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? statusId = GetStatusId(e.RowIndex);
            if (statusId.HasValue)
            {
                UserStatus.DeleteStatus(statusId.Value);
                this.BindData();
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowEditing"/> event of the <see cref="StatusesGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void StatusesGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.StatusesGridView.EditIndex = e.NewEditIndex;
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
                    Localization.LocalizeGridView(ref this.StatusesGridView, this.LocalResourceFile);
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