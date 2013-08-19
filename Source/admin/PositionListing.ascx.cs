// <copyright file="PositionListing.ascx.cs" company="Engage Software">
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
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.UI.Utilities;

    public partial class PositionListing : ModuleBase
    {
        protected const int JobTitleMaxLength = 255;

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(JobTitleMaxLength); }
        }

        protected string MaxLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("JobTitleMaxLength"), JobTitleMaxLength); }
        }

        /// <summary>Raises the <see cref="Control.Init"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            if (!PermissionController.CanManageJobs(this))
            {
                this.DenyAccess();
                return;
            }

            this.Load += this.Page_Load;
            this.AddButton.Click += this.AddButton_Click;
            this.BackButton.Click += this.BackButton_Click;
            this.CancelNewPositionButton.Click += this.CancelNewPositionButton_Click;
            this.SaveNewPositionButton.Click += this.SaveNewPositionButton_Click;
            this.PositionsGrid.RowCancelingEdit += this.PositionsGrid_RowCancelingEdit;
            this.PositionsGrid.RowCommand += this.PositionsGrid_RowCommand;
            this.PositionsGrid.RowDataBound += this.PositionsGrid_RowDataBound;
            this.PositionsGrid.RowDeleting += this.PositionsGrid_RowDeleting;
            this.PositionsGrid.RowEditing += this.PositionsGrid_RowEditing;
            this.NewJobDescriptionRequiredValidator.ServerValidate += this.NewJobDescriptionRequiredValidator_ServerValidate;
            base.OnInit(e);
        }

        /// <summary>Handles the <see cref="CustomValidator.ServerValidate"/> event of the <c>JobDescriptionRequiredValidator</c> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void JobDescriptionRequiredValidator_ServerValidate(object sender, ServerValidateEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("args", @"args must not be null");
            }

            args.IsValid = TextEditorHasValue(this.GetJobDescription(this.PositionsGrid.EditIndex));
        }

        private static int? GetPositionId(Control row)
        {
            var hdnPositionId = (HiddenField)row.FindControl("hdnPositionId");

            int positionId;
            if (hdnPositionId != null && int.TryParse(hdnPositionId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out positionId))
            {
                return positionId;
            }

            return null;
        }

        private static bool TextEditorHasValue(string value)
        {
            return Engage.Utility.HasValue(HtmlUtils.Clean(value, false));
        }

        private string GetJobTitle(int rowIndex)
        {
            if (this.PositionsGrid == null || this.PositionsGrid.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.PositionsGrid.Rows[rowIndex];
            var txtJobTitle = (TextBox)row.FindControl("txtJobTitle");
            return txtJobTitle.Text;
        }

        private string GetJobDescription(int rowIndex)
        {
            if (this.PositionsGrid == null || this.PositionsGrid.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.PositionsGrid.Rows[rowIndex];
            var txtJobDescription = (TextEditor)row.FindControl("txtJobDescription");
            return txtJobDescription.Text;
        }

        private int? GetPositionId(int rowIndex)
        {
            if (this.PositionsGrid == null || this.PositionsGrid.Rows.Count <= rowIndex)
            {
                return null;
            }

            return GetPositionId(this.PositionsGrid.Rows[rowIndex]);
        }

        private void HideAndClearNewPositionPanel()
        {
            this.NewPositionPanel.Visible = false;
            this.NewJobTitleTextBox.Text = string.Empty;
            this.NewJobDescriptionTextEditor.Text = string.Empty;
        }

        private bool IsJobTitleUnique(int? positionId, string newJobTitle)
        {
            var newPositionId = Position.GetPositionId(newJobTitle, this.PortalId);
            return !newPositionId.HasValue || (positionId.HasValue && newPositionId.Value == positionId.Value);
        }

        private void LoadPositions()
        {
            var positions = Position.LoadPositions(null, this.PortalId);
            this.PositionsGrid.DataSource = positions;
            this.PositionsGrid.DataBind();

            this.NewPositionPanel.CssClass = positions.Count % 2 == 0 
                ? this.PositionsGrid.RowStyle.CssClass 
                : this.PositionsGrid.AlternatingRowStyle.CssClass;

            this.NewPositionHeaderRow.Visible = positions.Count < 1;
        }

        private void SetupLengthValidation()
        {
            this.NewJobTitleLengthValidator.ValidationExpression = MaxLengthValidationExpression;
            this.NewJobTitleLengthValidator.ErrorMessage = this.MaxLengthValidationText;
            this.NewJobTitleTextBox.MaxLength = JobTitleMaxLength;
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="AddButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPositionPanel.Visible = true;
            this.NewJobTitleTextBox.Focus();
        }

        /// <summary>Handles the <see cref="LinkButton.Click"/> event of the <see cref="BackButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="CancelNewPositionButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelNewPositionButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewPositionPanel();
        }

        /// <summary>Handles the <see cref="CustomValidator.ServerValidate"/> event of the <see cref="NewJobDescriptionRequiredValidator"/> control.</summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private void NewJobDescriptionRequiredValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = TextEditorHasValue(this.NewJobDescriptionTextEditor.Text);
        }

        /// <summary>Handles the <see cref="Control.Load"/> event of this control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.IsPostBack)
                {
                    return;
                }

                Localization.LocalizeGridView(ref this.PositionsGrid, this.LocalResourceFile);
                this.SetupLengthValidation();
                this.LoadPositions();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="PositionsGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.PositionsGrid.EditIndex = -1;
            this.LoadPositions();
        }

        /// <summary>Handles the <see cref="GridView.RowCommand"/> event of the <see cref="PositionsGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
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

            var positionId = this.GetPositionId(rowIndex);
            if (!positionId.HasValue)
            {
                return;
            }

            var newJobTitle = this.GetJobTitle(rowIndex);
            if (!this.IsJobTitleUnique(positionId, newJobTitle))
            {
                this.DuplicatePositionValidator.IsValid = false;
                return;
            }
            
            Position.UpdatePosition(positionId.Value, newJobTitle, this.FilterHtml(this.GetJobDescription(rowIndex)));
            this.PositionsGrid.EditIndex = -1;
            this.LoadPositions();
        }

        /// <summary>Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="PositionsGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
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

            var positionId = GetPositionId(row);
            if (positionId.HasValue && Position.IsPositionUsed(positionId.Value))
            {
                deleteButton.Enabled = false;
                return;
            }
            
            deleteButton.OnClientClick = string.Format(
                CultureInfo.CurrentCulture,
                "return confirm('{0}');",
                ClientAPI.GetSafeJSString(this.Localize("DeleteConfirm")));
        }

        /// <summary>Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="PositionsGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var positionId = this.GetPositionId(e.RowIndex);
            if (!positionId.HasValue)
            {
                return;
            }

            Position.DeletePosition(positionId.Value);
            this.LoadPositions();
        }

        /// <summary>Handles the <see cref="GridView.RowEditing"/> event of the <see cref="PositionsGrid"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.PositionsGrid.EditIndex = e.NewEditIndex;
            this.HideAndClearNewPositionPanel();
            this.LoadPositions();
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="SaveNewPositionButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveNewPositionButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (!this.IsJobTitleUnique(null, this.NewJobTitleTextBox.Text))
            {
                this.DuplicatePositionValidator.IsValid = false;
                return;
            }
            
            Position.InsertPosition(this.NewJobTitleTextBox.Text, this.FilterHtml(this.NewJobDescriptionTextEditor.Text), this.PortalId);
            this.HideAndClearNewPositionPanel();
            this.LoadPositions();
        }
    }
}