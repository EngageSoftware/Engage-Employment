// <copyright file="JobDetailOptions.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2014
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
    using Engage.Dnn.Framework;

    /// <summary>Allows a module editor to set the options for the details module</summary>
    public partial class JobDetailOptions : ModuleBase
    {
        /// <summary>Raises the <see cref="Control.Init"/> event.</summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e) 
        {
            if (!PermissionController.CanManageJobDetailOptions(this))
            {
                this.DenyAccess();
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

        /// <summary>Handles the <see cref="CustomValidator.ServerValidate"/> event of the <see cref="NewLeadUniqueValidator"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private static void NewLeadUniqueValidator_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            int leadId;
            if (e == null || !Engage.Utility.HasValue(e.Value)
                || !int.TryParse(e.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                return;
            }

            e.IsValid = !DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Gets the ID of the lead entry in the given row.</summary>
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

        /// <summary>Handles the <see cref="Control.Load"/> event of this control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "DNN Error handling is ugly if not caught by the module")]
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.IsPostBack)
                {
                    return;
                }

                this.ApplicationEmailRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx; 
                this.FromEmailRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;

                this.txtApplicationEmailAddress.Text = ModuleSettings.JobDetailApplicationEmailAddresses.GetValueAsStringFor(this) ?? this.PortalSettings.Email;
                this.txtFromEmailAddress.Text = ModuleSettings.JobDetailFromEmailAddress.GetValueAsStringFor(this) ?? this.PortalSettings.Email;
                this.RequireRegistrationCheckBox.Checked = ModuleSettings.JobDetailRequireRegistration.GetValueAsBooleanFor(this).Value;
                this.EnableDnnSearchCheckBox.Checked = ModuleSettings.JobDetailEnableDnnSearch.GetValueAsBooleanFor(this).Value;
                this.ShowCloseDateCheckBox.Checked = ModuleSettings.JobDetailShowCloseDate.GetValueAsBooleanFor(this).Value;

                this.SetupVisibilityList(this.DisplayNameRadioButtonList, ModuleSettings.JobDetailDisplayName);
                this.SetupVisibilityList(this.DisplayEmailRadioButtonList, ModuleSettings.JobDetailDisplayEmail);
                this.SetupVisibilityList(this.DisplayPhoneRadioButtonList, ModuleSettings.JobDetailDisplayPhone);
                this.SetupVisibilityList(this.DisplayMessageRadioButtonList, ModuleSettings.JobDetailDisplayMessage);
                this.SetupVisibilityList(this.DisplaySalaryRadioButtonList, ModuleSettings.JobDetailDisplaySalaryRequirement);
                this.SetupVisibilityList(this.DisplayCoverLetterRadioButtonList, ModuleSettings.JobDetailDisplayCoverLetter);
                this.SetupVisibilityList(this.DisplayResumeRadioButtonList, ModuleSettings.JobDetailDisplayResume);
                this.SetupVisibilityList(this.DisplayLeadRadioButtonList, ModuleSettings.JobDetailDisplayLead);
                this.rowLeadItems.Visible = this.DisplayLeadRadioButtonList.SelectedValue != Visibility.Hidden.ToString();

                Localization.LocalizeGridView(ref this.LeadItemsGridView, this.LocalResourceFile);
                this.BindLeadItems();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Sets up a visibility radio button list.</summary>
        /// <param name="list">The radio button list.</param>
        /// <param name="visibilitySetting">The visibility setting.</param>
        private void SetupVisibilityList(ListControl list, Setting<Visibility> visibilitySetting)
        {
            list.Items.Add(new ListItem(this.Localize(Visibility.Hidden.ToString()), Visibility.Hidden.ToString()));
            list.Items.Add(new ListItem(this.Localize(Visibility.Optional.ToString()), Visibility.Optional.ToString()));
            list.Items.Add(new ListItem(this.Localize(Visibility.Required.ToString()), Visibility.Required.ToString()));
            list.SelectedValue = visibilitySetting.GetValueAsEnumFor<Visibility>(this).ToString();
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="UpdateButton"/> control.</summary>
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

                ModuleSettings.JobDetailApplicationEmailAddresses.Set(this, this.txtApplicationEmailAddress.Text);
                ModuleSettings.JobDetailFromEmailAddress.Set(this, this.txtFromEmailAddress.Text);
                ModuleSettings.JobDetailRequireRegistration.Set(this, this.RequireRegistrationCheckBox.Checked);
                ModuleSettings.JobDetailDisplayName.Set(this, this.DisplayNameRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayEmail.Set(this, this.DisplayEmailRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayPhone.Set(this, this.DisplayPhoneRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayMessage.Set(this, this.DisplayMessageRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplaySalaryRequirement.Set(this, this.DisplaySalaryRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayCoverLetter.Set(this, this.DisplayCoverLetterRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayResume.Set(this, this.DisplayResumeRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailDisplayLead.Set(this, this.DisplayLeadRadioButtonList.SelectedValue);
                ModuleSettings.JobDetailEnableDnnSearch.Set(this, this.EnableDnnSearchCheckBox.Checked);
                ModuleSettings.JobDetailShowCloseDate.Set(this, this.ShowCloseDateCheckBox.Checked);

                this.Response.Redirect(Globals.NavigateURL(this.TabId));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="CancelButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // return to the main page view 
            this.Response.Redirect(Globals.NavigateURL(), false);
        }

        /// <summary>Handles the <see cref="ListControl.SelectedIndexChanged"/> event of the <see cref="DisplayLeadRadioButtonList"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DisplayLeadRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var leadVisibility = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true);
            this.rowLeadItems.Visible = leadVisibility != Visibility.Hidden;
        }

        /// <summary>Handles the <see cref="GridView.RowDataBound"/> event of the <see cref="LeadItemsGridView"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e == null || e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var deleteButton = e.Row.FindControl("DeleteButton") as Button;
            if (deleteButton == null)
            {
                return;
            }

            var leadId = GetLeadItemId(e.Row);
            if (leadId.HasValue
                && DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.Value.ToString(CultureInfo.InvariantCulture)))
            {
                deleteButton.Enabled = false;
                return;
            }

            deleteButton.Attributes["data-confirm-click"] = this.Localize("DeleteConfirm");
            Dnn.Utility.RequestEmbeddedScript(this.Page, "confirmClick.js");
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="NewLeadItemButton"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NewLeadItemButton_Click(object sender, EventArgs e)
        {
            this.NewLeadItemPanel.Visible = true;
            this.txtNewLeadText.Focus();
        }

        /// <summary>Handles the <see cref="Button.Click"/> event of the <see cref="SaveNewLeadButton"/> control.</summary>
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
                    Value = this.txtNewLeadText.Text,
                    DefinitionID = Null.NullInteger,
                    PortalID = this.PortalId,
                    ListName = Utility.LeadListName,
                });

            this.NewLeadItemPanel.Visible = false;
            this.txtNewLeadText.Text = string.Empty;
            this.BindLeadItems();
        }

        /// <summary>Handles the <see cref="CustomValidator.ServerValidate"/> event of the <see cref="SaveLeadRequirementValidator"/> control.</summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        private void SaveLeadRequirementValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true) == Visibility.Hidden
                           || (new ListController()).GetListEntryInfoCollection(Utility.LeadListName, Null.NullString, this.PortalId).Count > 0;
        }

        /// <summary>Handles the <see cref="GridView.RowCommand"/> event of the <see cref="LeadItemsGridView"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowCommand(object sender, CommandEventArgs e)
        {
            if (!this.Page.IsValid || e == null)
            {
                return;
            }

            int rowIndex;
            if (!int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
            {
                return;
            }

            if (!string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var leadId = this.GetLeadItemId(rowIndex);
            if (!leadId.HasValue)
            {
                return;
            }

            var lists = new ListController();
            var leadItem = lists.GetListEntryInfo(leadId.Value);
            var newLeadText = this.GetLeadItemText(rowIndex);
            var oldLeadText = leadItem.Text;

            if (string.Equals(newLeadText, oldLeadText, StringComparison.CurrentCultureIgnoreCase) || lists.GetListEntryInfo(Utility.LeadListName, newLeadText) == null)
            {
                leadItem.Value = newLeadText;
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

        /// <summary>Handles the <see cref="GridView.RowDeleting"/> event of the <see cref="LeadItemsGridView"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewDeleteEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var leadId = GetLeadItemId(e.RowIndex);
            if (!leadId.HasValue)
            {
                return;
            }

            (new ListController()).DeleteListEntryByID(leadId.Value, true);
            this.BindLeadItems();
        }

        /// <summary>Handles the <see cref="GridView.RowEditing"/> event of the <see cref="LeadItemsGridView"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewEditEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.LeadItemsGridView.EditIndex = e.NewEditIndex;
            this.BindLeadItems();
        }

        /// <summary>Handles the <see cref="GridView.RowCancelingEdit"/> event of the <see cref="LeadItemsGridView"/> control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCancelEditEventArgs"/> instance containing the event data.</param>
        private void LeadItemsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.LeadItemsGridView.EditIndex = -1;
            this.BindLeadItems();
        }

        /// <summary>Binds the list of leads to the <see cref="LeadItemsGridView"/>.</summary>
        private void BindLeadItems()
        {
            var leadItems = (new ListController()).GetListEntryInfoCollection(Utility.LeadListName, Null.NullString, this.PortalId);

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

        /// <summary>Gets the text for lead in the given row.</summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>The name of the lead</returns>
        private string GetLeadItemText(int rowIndex)
        {
            if (this.LeadItemsGridView == null || this.LeadItemsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.LeadItemsGridView.Rows[rowIndex];
            var txtLeadText = row.FindControl("txtLeadText") as TextBox;

            return txtLeadText != null ? txtLeadText.Text : null;
        }

        /// <summary>Gets the ID of the lead item in the given row.</summary>
        /// <param name="rowIndex">Index of the row.</param>
        /// <returns>ID of the lead, or <c>null</c> if there's no lead in that row or the row doesn't exist</returns>
        private int? GetLeadItemId(int rowIndex)
        {
            if (this.LeadItemsGridView == null || this.LeadItemsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            return GetLeadItemId(this.LeadItemsGridView.Rows[rowIndex]);
        }
    }
}