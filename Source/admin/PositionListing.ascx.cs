// <copyright file="PositionListing.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("JobTitleMaxLength", this.LocalResourceFile), JobTitleMaxLength); }
        }

        /// <summary>
        /// Raises the <see cref="Control.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
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

        /// <summary>
        /// Handles the ServerValidate event of the JobDescriptionRequiredValidator control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void JobDescriptionRequiredValidator_ServerValidate(object sender, ServerValidateEventArgs args)
        {
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
            if (this.PositionsGrid != null && this.PositionsGrid.Rows.Count > rowIndex)
            {
                GridViewRow row = this.PositionsGrid.Rows[rowIndex];
                var txtJobTitle = (TextBox)row.FindControl("txtJobTitle");
                return txtJobTitle.Text;
            }

            return null;
        }

        private string GetJobDescription(int rowIndex)
        {
            if (this.PositionsGrid != null && this.PositionsGrid.Rows.Count > rowIndex)
            {
                GridViewRow row = this.PositionsGrid.Rows[rowIndex];
                var txtJobDescription = (TextEditor)row.FindControl("txtJobDescription");
                return txtJobDescription.Text;
            }

            return null;
        }

        private int? GetPositionId(int rowIndex)
        {
            if (this.PositionsGrid != null && this.PositionsGrid.Rows.Count > rowIndex)
            {
                return GetPositionId(this.PositionsGrid.Rows[rowIndex]);
            }

            return null;
        }

        private void HideAndClearNewPositionPanel()
        {
            this.NewPositionPanel.Visible = false;
            this.NewJobTitleTextBox.Text = string.Empty;
            this.NewJobDescriptionTextEditor.Text = string.Empty;
        }

        private bool IsJobTitleUnique(int? positionId, string newJobTitle)
        {
            int? newPositionId = Position.GetPositionId(newJobTitle, this.PortalId);
            return !newPositionId.HasValue || (positionId.HasValue && newPositionId.Value == positionId.Value);
        }

        private void LoadPositions()
        {
            List<Position> positions = Position.LoadPositions(null, this.PortalId);
            this.PositionsGrid.DataSource = positions;
            this.PositionsGrid.DataBind();

            if (positions == null || positions.Count % 2 == 0)
            {
                this.NewPositionPanel.CssClass = this.PositionsGrid.RowStyle.CssClass;
            }
            else
            {
                this.NewPositionPanel.CssClass = this.PositionsGrid.AlternatingRowStyle.CssClass;
            }

            this.NewPositionHeaderRow.Visible = positions == null || positions.Count < 1;
        }

        private void SetupLengthValidation()
        {
            this.NewJobTitleLengthValidator.ValidationExpression = MaxLengthValidationExpression;
            this.NewJobTitleLengthValidator.ErrorMessage = this.MaxLengthValidationText;
            this.NewJobTitleTextBox.MaxLength = JobTitleMaxLength;
        }

        /// <summary>
        /// Handles the Click event of the AddButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPositionPanel.Visible = true;
            this.NewJobTitleTextBox.Focus();
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        /// <summary>
        /// Handles the Click event of the CancelNewPositionButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CancelNewPositionButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewPositionPanel();
        }

        /// <summary>
        /// Handles the ServerValidate event of the NewJobDescriptionRequiredValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        private void NewJobDescriptionRequiredValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = TextEditorHasValue(this.NewJobDescriptionTextEditor.Text);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    Dnn.Utility.LocalizeGridView(ref this.PositionsGrid, this.LocalResourceFile);
                    this.SetupLengthValidation();
                    this.LoadPositions();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the RowCancelingEdit event of the PositionsGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.PositionsGrid.EditIndex = -1;
            this.LoadPositions();
        }

        /// <summary>
        /// Handles the RowCommand event of the PositionsGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewCommandEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (this.Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? positionId = this.GetPositionId(rowIndex);
                        if (positionId.HasValue)
                        {
                            string newJobTitle = this.GetJobTitle(rowIndex);
                            if (this.IsJobTitleUnique(positionId, newJobTitle))
                            {
                                Position.UpdatePosition(positionId.Value, newJobTitle, this.GetJobDescription(rowIndex));
                                this.PositionsGrid.EditIndex = -1;
                                this.LoadPositions();
                            }
                            else
                            {
                                this.DuplicatePositionValidator.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the PositionsGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    var btnDelete = (Button)row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        int? positionId = GetPositionId(row);
                        if (positionId.HasValue && Position.IsPositionUsed(positionId.Value))
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
        /// Handles the RowDeleting event of the PositionsGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? positionId = this.GetPositionId(e.RowIndex);
            if (positionId.HasValue)
            {
                Position.DeletePosition(positionId.Value);
                this.LoadPositions();
            }
        }

        /// <summary>
        /// Handles the RowEditing event of the PositionsGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewEditEventArgs"/> instance containing the event data.</param>
        private void PositionsGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.PositionsGrid.EditIndex = e.NewEditIndex;
            this.HideAndClearNewPositionPanel();
            this.LoadPositions();
        }

        /// <summary>
        /// Handles the Click event of the SaveNewPositionButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SaveNewPositionButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                if (this.IsJobTitleUnique(null, this.NewJobTitleTextBox.Text))
                {
                    Position.InsertPosition(this.NewJobTitleTextBox.Text, this.NewJobDescriptionTextEditor.Text, this.PortalId);
                    this.HideAndClearNewPositionPanel();
                    this.LoadPositions();
                }
                else
                {
                    this.DuplicatePositionValidator.IsValid = false;
                }
            }
        }
    }
}