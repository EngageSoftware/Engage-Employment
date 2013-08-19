// <copyright file="ApplicationStatusListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2013
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Engage.Annotations;

    /// <summary>Displays and edits the list of application statuses</summary>
    public partial class ApplicationStatusListing : ModuleBase
    {
        /// <summary>The maximum length for a status name</summary>
        private const int StatusMaxLength = 255;

        /// <summary>Gets the regular expression to limit the length of the status name.</summary>
        /// <value>The regular expression to limit the length of the status name.</value>
        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(StatusMaxLength); }
        }

        /// <summary>Gets the error message to use when the status name is longer than the maximum</summary>
        /// <value>The max length validation text.</value>
        protected string MaxLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("StatusMaxLength"), StatusMaxLength); }
        }

        /// <summary>Raises the <see cref="Control.Init"/> event.</summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            if (!PermissionController.CanManageApplications(this))
            {
                this.DenyAccess();
                return;
            }

            this.Load += this.Page_Load;
            this.AddButton.Click += this.AddButton_Click;
            this.BackButton.Click += this.BackButton_Click;
            this.CancelNewStatusButton.Click += this.CancelNewStatusButton_Click;
            this.SaveNewStatusButton.Click += this.SaveNewStatusButton_Click;
            this.StatusesGrid.RowCancelingEdit += this.StatusesGrid_RowCancelingEdit;
            this.StatusesGrid.RowCommand += this.StatusesGrid_RowCommand;
            this.StatusesGrid.RowDataBound += this.StatusesGrid_RowDataBound;
            this.StatusesGrid.RowDeleting += this.StatusesGrid_RowDeleting;
            this.StatusesGrid.RowEditing += this.StatusesGrid_RowEditing;

            base.OnInit(e);
        }

        /// <summary>Gets the ID of the status represented in the given row.</summary>
        /// <param name="row">The row representing a status.</param>
        /// <returns>The ID of the status represented in the given row</returns>
        private static int? GetStatusId(Control row)
        {
            var statusIdHiddenField = (HiddenField)row.FindControl("StatusIdHiddenField");

            int statusId;
            if (statusIdHiddenField != null && int.TryParse(statusIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                return statusId;
            }

            return null;
        }

        /// <summary>Handles the <see cref="Control.Load"/> event of this control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Letting execeptions go blows up the whole page, instead of just the module")]
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.IsPostBack)
                {
                    return;
                }

                Localization.LocalizeGridView(ref this.StatusesGrid, this.LocalResourceFile);
                this.SetupStatusLengthValidation();
                this.BindData();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Handles the <see cref="LinkButton.Click"/> event of the <see cref="BackButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.ManageApplications.ToString()));
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="AddButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPanel.Visible = true;
            this.txtNewStatus.Focus();
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="SaveNewStatusButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveNewStatusButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (!this.IsStatusNameUnique(null, this.txtNewStatus.Text))
            {
                this.cvDuplicateStatus.IsValid = false;
                return;
            }
            
            ApplicationStatus.InsertStatus(this.txtNewStatus.Text, this.PortalId);
            this.HideAndClearNewStatusPanel();
            this.BindData();
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="CancelNewStatusButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelNewStatusButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewStatusPanel();
        }

        /// <summary>Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="StatusesGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void StatusesGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.StatusesGrid.EditIndex = -1;
            this.BindData();
        }

        /// <summary>Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="StatusesGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void StatusesGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var row = e.Row;
            if (row == null)
            {
                return;
            }

            var deleteButton = (Button)row.FindControl("DeleteButton");
            if (deleteButton == null)
            {
                return;
            }

            var statusId = GetStatusId(row);
            if (statusId.HasValue && ApplicationStatus.IsStatusUsed(statusId.Value))
            {
                deleteButton.Enabled = false;
                return;
            }
            
            deleteButton.OnClientClick = string.Format(
                CultureInfo.CurrentCulture,
                "return confirm('{0}');",
                ClientAPI.GetSafeJSString(this.Localize("DeleteConfirm")));
        }

        /// <summary>Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="StatusesGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void StatusesGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var statusId = GetStatusId(e.RowIndex);
            if (!statusId.HasValue)
            {
                return;
            }

            ApplicationStatus.DeleteStatus(statusId.Value);
            this.BindData();
        }

        /// <summary>Handles the <see cref="GridView.RowEditing"/> event of the <see cref="StatusesGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void StatusesGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.StatusesGrid.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatusPanel();
            this.BindData();
        }

        /// <summary>Handles the <see cref="GridView.RowCommand"/> event of the <see cref="StatusesGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        private void StatusesGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!this.Page.IsValid)
            {
                return;
            }

            int rowIndex;
            if (!int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
            {
                return;
            }

            var statusId = this.GetStatusId(rowIndex);
            if (!statusId.HasValue)
            {
                return;
            }

            var newStatusName = this.GetStatusName(rowIndex);
            if (!this.IsStatusNameUnique(statusId, newStatusName))
            {
                this.cvDuplicateStatus.IsValid = false;
                return;
            }
            
            ApplicationStatus.UpdateStatus(statusId.Value, newStatusName);
            this.StatusesGrid.EditIndex = -1;
            this.BindData();
        }

        /// <summary>Fills <see cref="StatusesGrid"/> with the list of all <see cref="ApplicationStatus"/> instances for this portal</summary>
        private void BindData()
        {
            var statuses = ApplicationStatus.GetStatuses(this.PortalId).ToArray();
            this.StatusesGrid.DataSource = statuses;
            this.StatusesGrid.DataBind();

            this.NewPanel.CssClass = statuses.Length % 2 == 0 
                ? this.StatusesGrid.RowStyle.CssClass 
                : this.StatusesGrid.AlternatingRowStyle.CssClass;

            this.rowNewHeader.Visible = !statuses.Any();
        }

        /// <summary>Determines whether the given status name is valid for the status with the given ID.</summary>
        /// <param name="statusId">The status ID.</param>
        /// <param name="newStatusName">New name for the status.</param>
        /// <returns><c>true</c> if the given status name is valid for the status with the given ID; otherwise, <c>false</c>.</returns>
        private bool IsStatusNameUnique(int? statusId, string newStatusName)
        {
            var newStatusId = ApplicationStatus.GetStatusId(newStatusName, this.PortalId);
            return !newStatusId.HasValue || (statusId.HasValue && newStatusId.Value == statusId.Value);
        }

        /// <summary>Sets up the validation of the status name length.</summary>
        private void SetupStatusLengthValidation()
        {
            this.regexNewStatus.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewStatus.ErrorMessage = this.MaxLengthValidationText;
        }

        /// <summary>Hides the and clears the panel for adding a new <see cref="ApplicationStatus"/>.</summary>
        private void HideAndClearNewStatusPanel()
        {
            this.NewPanel.Visible = false;
            this.txtNewStatus.Text = string.Empty;
        }

        /// <summary>Gets the name of the status in the given row.</summary>
        /// <param name="rowIndex">Index of the row representing the status.</param>
        /// <returns>The name of the status in the given row, or <c>null</c></returns>
        [CanBeNull]
        private string GetStatusName(int rowIndex)
        {
            if (this.StatusesGrid == null || this.StatusesGrid.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.StatusesGrid.Rows[rowIndex];
            var statusTextBox = row.FindControl("StatusTextBox") as TextBox;
            Debug.Assert(statusTextBox != null, "StatusTextBox not found in row");
            return statusTextBox.Text;
        }

        /// <summary>Gets the ID of the status represented in the row with the given index.</summary>
        /// <param name="rowIndex">Index of the row representing the status.</param>
        /// <returns>The ID of the status in the given row</returns>
        private int? GetStatusId(int rowIndex)
        {
            if (this.StatusesGrid == null || this.StatusesGrid.Rows.Count <= rowIndex)
            {
                return null;
            }

            return GetStatusId(this.StatusesGrid.Rows[rowIndex]);
        }
    }
}