// <copyright file="StateListing.ascx.cs" company="Engage Software">
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
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("StateMaxLength", LocalResourceFile), StateNameMaxLength); }
        }

        protected string MaxAbbreviationLengthValidationText
        {
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("AbbreviationMaxLength", LocalResourceFile), AbbreviationMaxLength); }
        }

        protected override void OnInit(EventArgs e)
        {
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
                if (!Page.IsPostBack)
                {
                    Localization.LocalizeGridView(ref this.StatesGridView, this.LocalResourceFile);
                    this.SetupLengthValidation();
                    this.LoadStates();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl(ControlKey.Edit.ToString()));
        }

        private static int? GetStateId(Control row)
        {
            var hdnStateId = (HiddenField)row.FindControl("hdnStateId");

            int stateId;
            if (hdnStateId != null && int.TryParse(hdnStateId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId))
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
            if (Page.IsValid)
            {
                if (this.IsStateNameUnique(null, this.txtNewState.Text))
                {
                    State.InsertState(this.txtNewState.Text, this.txtNewAbbreviation.Text, this.PortalId);
                    this.HideAndClearNewStatePanel();
                    this.LoadStates();
                }
                else
                {
                    this.cvDuplicateState.IsValid = false;
                }
            }
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
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    Button btnDelete = (Button)row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        int? stateId = GetStateId(row);
                        if (stateId.HasValue && State.IsStateUsed(stateId.Value))
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

        private void StatesGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? stateId = GetStateId(e.RowIndex);
            if (stateId.HasValue)
            {
                State.DeleteState(stateId.Value);
                this.LoadStates();
            }
        }

        private void StatesGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.StatesGridView.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatePanel();
            this.LoadStates();
        }

        private void StatesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? stateId = GetStateId(rowIndex);
                        if (stateId.HasValue)
                        {
                            var newStateName = this.GetStateName(rowIndex);
                            if (this.IsStateNameUnique(stateId, newStateName))
                            {
                                State.UpdateState(stateId.Value, newStateName, this.GetStateAbbreviation(rowIndex));
                                this.StatesGridView.EditIndex = -1;
                                this.LoadStates();
                            }
                            else
                            {
                                this.cvDuplicateState.IsValid = false;
                            }
                        }
                    }
                }
            }
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

            if (states == null || states.Count % 2 == 0)
            {
                this.NewPanel.CssClass = this.StatesGridView.RowStyle.CssClass;
            }
            else
            {
                this.NewPanel.CssClass = this.StatesGridView.AlternatingRowStyle.CssClass;
            }

            this.rowNewHeader.Visible = states == null || states.Count < 1;
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
            if (this.StatesGridView != null && this.StatesGridView.Rows.Count > rowIndex)
            {
                var row = this.StatesGridView.Rows[rowIndex];
                var txtState = row.FindControl("txtState") as TextBox;
                Debug.Assert(txtState != null, "txtState was not found in row");
                return txtState.Text;
            }

            return null;
        }

        private string GetStateAbbreviation(int rowIndex)
        {
            if (this.StatesGridView != null && this.StatesGridView.Rows.Count > rowIndex)
            {
                var row = this.StatesGridView.Rows[rowIndex];
                var txtAbbreviation = row.FindControl("txtAbbreviation") as TextBox;
                Debug.Assert(txtAbbreviation != null, "txtAbbreviation was not found in row");
                return txtAbbreviation.Text;
            }

            return null;
        }

        private int? GetStateId(int rowIndex)
        {
            if (this.StatesGridView != null && this.StatesGridView.Rows.Count > rowIndex)
            {
                return GetStateId(this.StatesGridView.Rows[rowIndex]);
            }

            return null;
        }
    }
}