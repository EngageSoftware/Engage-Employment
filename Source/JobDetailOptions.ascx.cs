// <copyright file="JobDetailOptions.ascx.cs" company="Engage Software">
// Engage: Employment
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Engage.Dnn.Employment.Data;

    /// <summary>
    /// Allows a module editor to set the options for the details module
    /// </summary>
    public partial class JobDetailOptions : ModuleBase
    {
        /// <summary>
        /// Raises the <see cref="Control.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e) 
        {
            if (!PermissionController.CanManageJobDetailOptions(this))
            {
                DenyAccess();
                return;
            }

            this.Load += this.Page_Load;
            this.UpdateButton.Click += this.UpdateButton_Click;
            this.CancelButton.Click += this.CancelButton_Click;
            this.LeadItemsGridView.RowDataBound += this.LeadItemsGridView_RowDataBound;
            this.LeadItemsGridView.RowCancelingEdit += this.LeadItemsGridView_RowCancelingEdit;
            this.LeadItemsGridView.RowEditing += this.LeadItemsGridView_RowEditing;
            this.LeadItemsGridView.RowCommand += this.LeadItemsGridView_RowCommand;
            this.LeadItemsGridView.RowDeleting += this.LeadItemsGridView_RowDeleting;
            this.NewLeadItemButton.Click += this.NewLeadItemButton_Click;
            this.SaveNewLeadButton.Click += this.SaveNewLeadButton_Click;
            this.NewLeadUniqueValidator.ServerValidate += NewLeadUniqueValidator_ServerValidate;
            this.SaveLeadRequirementValidator.ServerValidate += this.SaveLeadRequirementValidator_ServerValidate;
            this.DisplayLeadRadioButtonList.SelectedIndexChanged += this.DisplayLeadRadioButtonList_SelectedIndexChanged;
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the <see cref="CustomValidator.ServerValidate"/> event of the <see cref="NewLeadUniqueValidator"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private static void NewLeadUniqueValidator_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            int leadId;
            if (e != null && Engage.Utility.HasValue(e.Value) && int.TryParse(e.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                e.IsValid = !DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets the ID of the lead entry in the given row.
        /// </summary>
        /// <param name="row">The row from which to get the lead ID.</param>
        /// <returns>THe ID of the lead in the given row, or <c>null</c> if there isn't a lead in that row</returns>
        private static int? GetLeadItemId(GridViewRow row)
        {
            var hdnLeadId = row.FindControl("hdnLeadId") as HiddenField;

            int leadId;
            if (hdnLeadId != null && int.TryParse(hdnLeadId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                return leadId;
            }

            return null;
        }

        /// <summary>
        /// Handles the <see cref="Control.Load"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "DNN Error handling is ugly if not caught by the module")]
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    this.ApplicationEmailRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx; 
                    this.FriendEmailRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;

                    this.txtApplicationEmailAddress.Text = ModuleSettings.JobDetailApplicationEmailAddress.GetValueAsStringFor(this) ?? this.PortalSettings.Email;
                    this.txtFriendEmailAddress.Text = ModuleSettings.JobDetailFriendEmailAddress.GetValueAsStringFor(this) ?? this.PortalSettings.Email;
                    this.RequireRegistrationCheckBox.Checked = ModuleSettings.JobDetailRequireRegistration.GetValueAsBooleanFor(this).Value;
                    this.EnableDnnSearchCheckBox.Checked = ModuleSettings.JobDetailEnableDnnSearch.GetValueAsBooleanFor(this).Value;
                    this.ShowCloseDateCheckBox.Checked = ModuleSettings.JobDetailShowCloseDate.GetValueAsBooleanFor(this).Value;

                    this.DisplayLeadRadioButtonList.DataSource = Enum.GetValues(typeof(Visibility));
                    this.DisplayLeadRadioButtonList.DataBind();

                    var displayLead = ModuleSettings.JobDetailDisplayLead.GetValueAsEnumFor<Visibility>(this);
                    this.DisplayLeadRadioButtonList.SelectedValue = displayLead.ToString();
                    this.rowLeadItems.Visible = displayLead != Visibility.Hidden;
                    Dnn.Utility.LocalizeListControl(this.DisplayLeadRadioButtonList, LocalResourceFile);

                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Hidden.ToString()), Visibility.Hidden.ToString()));
                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Optional.ToString()), Visibility.Optional.ToString()));
                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Required.ToString()), Visibility.Required.ToString()));
                    this.DisplaySalaryRadioButtonList.SelectedValue = ModuleSettings.JobDetailDisplaySalaryRequirement.GetValueAsEnumFor<Visibility>(this).ToString();

                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Hidden.ToString()), Visibility.Hidden.ToString()));
                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Optional.ToString()), Visibility.Optional.ToString()));
                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Required.ToString()), Visibility.Required.ToString()));
                    this.DisplayCoverLetterRadioButtonList.SelectedValue = ModuleSettings.JobDetailDisplayCoverLetter.GetValueAsEnumFor<Visibility>(this).ToString();

                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Hidden.ToString()), Visibility.Hidden.ToString()));
                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Optional.ToString()), Visibility.Optional.ToString()));
                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(this.Localize(Visibility.Required.ToString()), Visibility.Required.ToString()));
                    this.DisplayMessageRadioButtonList.SelectedValue = ModuleSettings.JobDetailDisplayMessage.GetValueAsEnumFor<Visibility>(this).ToString();

                    Localization.LocalizeGridView(ref this.LeadItemsGridView, this.LocalResourceFile);
                    this.BindLeadItems();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="UpdateButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "DNN Error handling is ugly if not caught by the module")]
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!this.Page.IsValid)
                {
                    return;
                }

                ModuleSettings.JobDetailApplicationEmailAddress.Set(this, this.txtApplicationEmailAddress.Text);
                ModuleSettings.JobDetailFriendEmailAddress.Set(this, this.txtFriendEmailAddress.Text);
                ModuleSettings.JobDetailRequireRegistration.Set(this, this.RequireRegistrationCheckBox.Checked);
                ModuleSettings.JobDetailDisplayLead.Set(this, this.DisplayLeadRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplaySalaryRequirement.Set(this, this.DisplaySalaryRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayCoverLetter.Set(this, this.DisplayCoverLetterRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayMessage.Set(this, this.DisplayMessageRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailEnableDnnSearch.Set(this, this.EnableDnnSearchCheckBox.Checked);
                ModuleSettings.JobDetailShowCloseDate.Set(this, this.ShowCloseDateCheckBox.Checked);

                this.Response.Redirect(Globals.NavigateURL(this.TabId));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="CancelButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // return to the main page view 
            Response.Redirect(Globals.NavigateURL(), false);
        }

        /// <summary>
        /// Handles the <see cref="ListControl.SelectedIndexChanged"/> event of the <see cref="DisplayLeadRadioButtonList"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DisplayLeadRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var leadVisibility = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true);
            this.rowLeadItems.Visible = leadVisibility != Visibility.Hidden;
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="LeadItemsGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e != null && e.Row.RowType == DataControlRowType.DataRow)
            {
                var btnDelete = e.Row.FindControl("btnDelete") as Button;
                if (btnDelete != null)
                {
                    int? leadId = GetLeadItemId(e.Row);
                    if (leadId.HasValue && DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.Value.ToString(CultureInfo.InvariantCulture)))
                    {
                        btnDelete.Enabled = false;
                    }
                    else
                    {
                        btnDelete.OnClientClick = "return confirm('" + this.Localize("DeleteConfirm").Replace("'", "\'") + "');";
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="NewLeadItemButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NewLeadItemButton_Click(object sender, EventArgs e)
        {
            this.NewLeadItemPanel.Visible = true;
            this.txtNewLeadText.Focus();
        }

        /// <summary>
        /// Handles the <see cref="Button.Click"/> event of the <see cref="SaveNewLeadButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveNewLeadButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            (new ListController()).AddListEntry(new ListEntryInfo 
                {
                    Text = this.txtNewLeadText.Text,
                    DefinitionID = Null.NullInteger,
                    PortalID = this.PortalId,
                    ListName = Utility.LeadListName,
                    Value = string.Empty
                });

            this.NewLeadItemPanel.Visible = false;
            this.txtNewLeadText.Text = string.Empty;
            this.BindLeadItems();
        }

        /// <summary>
        /// Handles the <see cref="CustomValidator.ServerValidate"/> event of the <see cref="SaveLeadRequirementValidator"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private void SaveLeadRequirementValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true) == Visibility.Hidden || (new ListController()).GetListEntryInfoCollection(Utility.LeadListName).Count > 0;
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowCommand"/> event of the <see cref="LeadItemsGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowCommand(object sender, CommandEventArgs e)
        {
            if (Page.IsValid && e != null)
            {
                int rowIndex;
                if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                {
                    if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        int? leadId = GetLeadItemId(rowIndex);
                        if (leadId.HasValue)
                        {
                            var lists = new ListController();
                            ListEntryInfo leadItem = lists.GetListEntryInfo(leadId.Value);
                            string newLeadText = this.GetLeadItemText(rowIndex);
                            string oldLeadText = leadItem.Text;

                            if (string.Equals(newLeadText, oldLeadText, StringComparison.CurrentCultureIgnoreCase) || lists.GetListEntryInfo(Utility.LeadListName, newLeadText) == null)
                            {
                                leadItem.Text = newLeadText;
                                lists.UpdateListEntry(leadItem);
                                this.LeadItemsGridView.EditIndex = -1;
                                this.BindLeadItems();
                            }
                            else
                            {
                                this.cvLeadEdit.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="LeadItemsGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? leadId = GetLeadItemId(e.RowIndex);
            if (leadId.HasValue)
            {
                (new ListController()).DeleteListEntryByID(leadId.Value, true);
                this.BindLeadItems();
            }
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowEditing"/> event of the <see cref="LeadItemsGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.LeadItemsGridView.EditIndex = e.NewEditIndex;
            this.BindLeadItems();
        }

        /// <summary>
        /// Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="LeadItemsGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.LeadItemsGridView.EditIndex = -1;
            this.BindLeadItems();
        }

        /// <summary>
        /// Binds the list of leads to the <see cref="LeadItemsGridView"/>.
        /// </summary>
        private void BindLeadItems()
        {
            ListEntryInfoCollection leadItems = (new ListController()).GetListEntryInfoCollection(Utility.LeadListName);

            this.LeadItemsGridView.DataSource = leadItems;
            this.LeadItemsGridView.DataBind();

            if (leadItems == null || leadItems.Count % 2 == 0)
            {
                this.NewLeadItemPanel.CssClass = this.LeadItemsGridView.RowStyle.CssClass;
            }
            else
            {
                this.NewLeadItemPanel.CssClass = this.LeadItemsGridView.AlternatingRowStyle.CssClass;
            }

            this.rowNewLeadItemHeader.Visible = leadItems == null || leadItems.Count < 1;
        }

        /// <summary>
        /// Gets the text for lead in the given row.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>The name of the lead</returns>
        private string GetLeadItemText(int rowIndex)
        {
            if (this.LeadItemsGridView != null && this.LeadItemsGridView.Rows.Count > rowIndex)
            {
                var row = this.LeadItemsGridView.Rows[rowIndex];
                var txtLeadText = row.FindControl("txtLeadText") as TextBox;

                return txtLeadText != null ? txtLeadText.Text : null;
            }

            return null;
        }

        /// <summary>
        /// Gets the ID of the lead item in the given row.
        /// </summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>ID of the lead, or <c>null</c> if there's no lead in that row or the row doesn't exist</returns>
        private int? GetLeadItemId(int rowIndex)
        {
            if (this.LeadItemsGridView != null && this.LeadItemsGridView.Rows.Count > rowIndex)
            {
                return GetLeadItemId(this.LeadItemsGridView.Rows[rowIndex]);
            }

            return null;
        }
    }
}

