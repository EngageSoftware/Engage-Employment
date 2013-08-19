// <copyright file="StateListing.ascx.cs" company="Engage Software">
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
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    public partial class StateListing : ModuleBase
    {
        private const int StateNameMaxLength = 255;
        private const int AbbreviationMaxLength = 10;

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(StateNameMaxLength); }
        }

        protected static string MaxAbbreviationLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(AbbreviationMaxLength); }
        }

        protected string MaxLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("StateMaxLength"), StateNameMaxLength); }
        }

        protected string MaxAbbreviationLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("AbbreviationMaxLength"), AbbreviationMaxLength); }
        }

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
            this.CancelNewButton.Click += this.CancelNewButton_Click;
            this.SaveNewButton.Click += this.SaveNewButton_Click;
            this.StatesGridView.RowCancelingEdit += this.StatesGridView_RowCancelingEdit;
            this.StatesGridView.RowCommand += this.StatesGridView_RowCommand;
            this.StatesGridView.RowDataBound += this.StatesGridView_RowDataBound;
            this.StatesGridView.RowDeleting += this.StatesGridView_RowDeleting;
            this.StatesGridView.RowEditing += this.StatesGridView_RowEditing;
            
            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Letting execeptions go blows up the whole page, instead of just the module")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsPostBack)
                {
                    return;
                }

                Localization.LocalizeGridView(ref this.StatesGridView, this.LocalResourceFile);
                this.SetupLengthValidation();
                this.LoadStates();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        private static int? GetStateId(Control row)
        {
            var stateIdHiddenField = (HiddenField)row.FindControl("StateIdHiddenField");

            int stateId;
            if (stateIdHiddenField != null && int.TryParse(stateIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId))
            {
                return stateId;
            }

            return null;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPanel.Visible = true;
            this.txtNewState.Focus();
        }

        private void SaveNewButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (!this.IsStateNameUnique(null, this.txtNewState.Text))
            {
                this.cvDuplicateState.IsValid = false;
                return;
            }
            
            State.InsertState(this.txtNewState.Text, this.txtNewAbbreviation.Text, this.PortalId);
            this.HideAndClearNewStatePanel();
            this.LoadStates();
        }

        private void CancelNewButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewStatePanel();
        }

        private void StatesGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.StatesGridView.EditIndex = -1;
            this.LoadStates();
        }

        private void StatesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
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

            var stateId = GetStateId(row);
            if (stateId.HasValue && State.IsStateUsed(stateId.Value))
            {
                deleteButton.Enabled = false;
                return;
            }

            deleteButton.Attributes["data-confirm-click"] = this.Localize("DeleteConfirm");
            Dnn.Utility.RequestEmbeddedScript(this.Page, "confirmClick.js");
        }

        private void StatesGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var stateId = GetStateId(e.RowIndex);
            if (!stateId.HasValue)
            {
                return;
            }

            State.DeleteState(stateId.Value);
            this.LoadStates();
        }

        private void StatesGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.StatesGridView.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatePanel();
            this.LoadStates();
        }

        private void StatesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
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
            
            var stateId = this.GetStateId(rowIndex);
            if (!stateId.HasValue)
            {
                return;
            }
            
            var newStateName = this.GetStateName(rowIndex);
            if (!this.IsStateNameUnique(stateId, newStateName))
            {
                this.cvDuplicateState.IsValid = false;
                return;
            }
            
            State.UpdateState(stateId.Value, newStateName, this.GetStateAbbreviation(rowIndex));
            this.StatesGridView.EditIndex = -1;
            this.LoadStates();
        }

        private bool IsStateNameUnique(int? stateId, string newStateName)
        {
            var newStateId = State.GetStateId(newStateName, PortalId);
            return !newStateId.HasValue || (stateId.HasValue && newStateId.Value == stateId.Value);
        }

        private void LoadStates()
        {
            var states = State.LoadStates(null, PortalId);
            this.StatesGridView.DataSource = states;
            this.StatesGridView.DataBind();

            this.NewPanel.CssClass = states.Count % 2 == 0 
                ? this.StatesGridView.RowStyle.CssClass 
                : this.StatesGridView.AlternatingRowStyle.CssClass;

            this.rowNewHeader.Visible = states.Count < 1;
        }

        private void SetupLengthValidation()
        {
            this.regexNewState.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewState.ErrorMessage = this.MaxLengthValidationText;
            this.regexNewAbbreviation.ValidationExpression = MaxAbbreviationLengthValidationExpression;
            this.regexNewAbbreviation.ErrorMessage = this.MaxAbbreviationLengthValidationText;
        }

        private void HideAndClearNewStatePanel()
        {
            this.NewPanel.Visible = false;
            this.txtNewState.Text = string.Empty;
            this.txtNewAbbreviation.Text = string.Empty;
        }

        private string GetStateName(int rowIndex)
        {
            if (this.StatesGridView == null || this.StatesGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.StatesGridView.Rows[rowIndex];
            var stateTextBox = row.FindControl("StateTextBox") as TextBox;
            Debug.Assert(stateTextBox != null, "StateTextBox was not found in row");
            return stateTextBox.Text;
        }

        private string GetStateAbbreviation(int rowIndex)
        {
            if (this.StatesGridView == null || this.StatesGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.StatesGridView.Rows[rowIndex];
            var txtAbbreviation = row.FindControl("txtAbbreviation") as TextBox;
            Debug.Assert(txtAbbreviation != null, "txtAbbreviation was not found in row");
            return txtAbbreviation.Text;
        }

        private int? GetStateId(int rowIndex)
        {
            if (this.StatesGridView == null || this.StatesGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            return GetStateId(this.StatesGridView.Rows[rowIndex]);
        }
    }
}