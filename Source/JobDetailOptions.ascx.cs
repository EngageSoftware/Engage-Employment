// <copyright file="JobDetailOptions.ascx.cs" company="Engage Software">
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
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    public partial class JobDetailOptions : ModuleBase
    {
        #region Event Handlers
        protected override void OnInit(EventArgs e) 
        {
            this.Load += Page_Load;
            this.UpdateButton.Click += this.UpdateButton_Click;
            CancelButton.Click += CancelButton_Click;
            this.LeadItemsGridView.RowDataBound += this.LeadItemsGridView_RowDataBound;
            this.LeadItemsGridView.RowCancelingEdit += this.LeadItemsGridView_RowCancelingEdit;
            this.LeadItemsGridView.RowEditing += this.LeadItemsGridView_RowEditing;
            this.LeadItemsGridView.RowCommand += this.LeadItemsGridView_RowCommand;
            this.LeadItemsGridView.RowDeleting += this.LeadItemsGridView_RowDeleting;
            this.NewLeadItemButton.Click += this.NewLeadItemButton_Click;
            this.SaveNewLeadButton.Click += this.SaveNewLeadButton_Click;
            this.NewLeadUniqueValidator.ServerValidate += NewLeadUniqueValidator_ServerValidate;
            this.SaveLeadRequirementValidator.ServerValidate += this.SaveLeadRequirementValidator_ServerValidate;
            this.DisplayLeadRadioButtonList.SelectedIndexChanged += this.DisplayLeadRadioButtonListSelectedIndexChanged;
            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    this.ApplicationEmailRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx;
                    this.FriendEmailRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;

                    txtApplicationEmailAddress.Text = ApplicationEmailAddress;
                    txtFriendEmailAddress.Text = FriendEmailAddress;
                    this.RequireRegistrationCheckBox.Checked = RequireRegistration;
                    this.EnableDnnSearchCheckBox.Checked = EnableDnnSearch;

                    this.DisplayLeadRadioButtonList.DataSource = Enum.GetValues(typeof(Visibility));
                    this.DisplayLeadRadioButtonList.DataBind();
                    //rblDisplayLead.Items.Add(new ListItem(Localization.GetString(Visibility.Hidden.ToString(), LocalResourceFile), Visibility.Hidden.ToString()));
                    //rblDisplayLead.Items.Add(new ListItem(Localization.GetString(Visibility.Optional.ToString(), LocalResourceFile), Visibility.Optional.ToString()));
                    //rblDisplayLead.Items.Add(new ListItem(Localization.GetString(Visibility.Required.ToString(), LocalResourceFile), Visibility.Required.ToString()));
                    this.DisplayLeadRadioButtonList.SelectedValue = DisplayLead.ToString();
                    rowLeadItems.Visible = (DisplayLead != Visibility.Hidden);
                    Engage.Dnn.Utility.LocalizeListControl(this.DisplayLeadRadioButtonList, LocalResourceFile);

                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Hidden.ToString(), LocalResourceFile), Visibility.Hidden.ToString()));
                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Optional.ToString(), LocalResourceFile), Visibility.Optional.ToString()));
                    this.DisplaySalaryRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Required.ToString(), LocalResourceFile), Visibility.Required.ToString()));
                    this.DisplaySalaryRadioButtonList.SelectedValue = DisplaySalaryRequirement.ToString();

                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Hidden.ToString(), LocalResourceFile), Visibility.Hidden.ToString()));
                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Optional.ToString(), LocalResourceFile), Visibility.Optional.ToString()));
                    this.DisplayCoverLetterRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Required.ToString(), LocalResourceFile), Visibility.Required.ToString()));
                    this.DisplayCoverLetterRadioButtonList.SelectedValue = DisplayCoverLetter.ToString();

                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Hidden.ToString(), LocalResourceFile), Visibility.Hidden.ToString()));
                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Optional.ToString(), LocalResourceFile), Visibility.Optional.ToString()));
                    this.DisplayMessageRadioButtonList.Items.Add(new ListItem(Localization.GetString(Visibility.Required.ToString(), LocalResourceFile), Visibility.Required.ToString()));
                    this.DisplayMessageRadioButtonList.SelectedValue = DisplayMessage.ToString();

                    Engage.Dnn.Utility.LocalizeGridView(ref this.LeadItemsGridView, LocalResourceFile);
                    BindLeadItems();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    ModuleController modules = new ModuleController();
                    modules.UpdateTabModuleSetting(this.TabModuleId, "ApplicationEmailAddress", txtApplicationEmailAddress.Text);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "FriendEmailAddress", txtFriendEmailAddress.Text);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "RequireRegistration", this.RequireRegistrationCheckBox.Checked.ToString(CultureInfo.InvariantCulture));
                    modules.UpdateTabModuleSetting(this.TabModuleId, "DisplayLead", this.DisplayLeadRadioButtonList.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "DisplaySalaryRequirement", this.DisplaySalaryRadioButtonList.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "DisplayCoverLetter", this.DisplayCoverLetterRadioButtonList.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "DisplayMessage", this.DisplayMessageRadioButtonList.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, Utility.EnableDnnSearchSetting, this.EnableDnnSearchCheckBox.Checked.ToString(CultureInfo.InvariantCulture));

                    Response.Redirect(Globals.NavigateURL(TabId));
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void CancelButton_Click(object sender, EventArgs e)
        {
            //return to the main page view 
            Response.Redirect(Globals.NavigateURL(TabId), false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void DisplayLeadRadioButtonListSelectedIndexChanged(object sender, EventArgs e)
        {
            Visibility leadVisibility = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true);
            rowLeadItems.Visible = (leadVisibility != Visibility.Hidden);
        }

        #region Lead GridView Event Handlers
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void LeadItemsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e != null && e.Row.RowType == DataControlRowType.DataRow)
            {
                Button btnDelete = e.Row.FindControl("btnDelete") as Button;
                if (btnDelete != null)
                {
                    int? leadId = GetLeadItemId(e.Row);
                    if (leadId.HasValue && DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.Value.ToString(CultureInfo.InvariantCulture)))
                    {
                        btnDelete.Enabled = false;
                    }
                    else
                    {
                        btnDelete.OnClientClick = "return confirm('" + Localization.GetString("DeleteConfirm", LocalResourceFile).Replace("'", "\'") + "');";
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void NewLeadItemButton_Click(object sender, EventArgs e)
        {
            this.NewLeadItemPanel.Visible = true;
            txtNewLeadText.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void SaveNewLeadButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                ListEntryInfo listItem = new ListEntryInfo();
                listItem.Text = txtNewLeadText.Text;
                listItem.DefinitionID = Null.NullInteger;
                listItem.PortalID = PortalId;
                listItem.ListName = Utility.LeadListName;
                listItem.Value = string.Empty;

                (new ListController()).AddListEntry(listItem);

                this.NewLeadItemPanel.Visible = false;
                txtNewLeadText.Text = string.Empty;
                BindLeadItems();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private static void NewLeadUniqueValidator_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            int leadId;
            if (e != null && Engage.Utility.HasValue(e.Value) && int.TryParse(e.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                e.IsValid = !DataProvider.Instance().IsPropertyValueUsed(ApplicationPropertyDefinition.Lead.GetId(), leadId.ToString(CultureInfo.InvariantCulture));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void SaveLeadRequirementValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = (Visibility)Enum.Parse(typeof(Visibility), this.DisplayLeadRadioButtonList.SelectedValue, true) == Visibility.Hidden || (new ListController()).GetListEntryInfoCollection(Utility.LeadListName).Count > 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
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
                            ListController lists = new ListController();
                            ListEntryInfo leadItem = lists.GetListEntryInfo(leadId.Value);
                            string newLeadText = GetLeadItemText(rowIndex);
                            string oldLeadText = leadItem.Text;

                            if (string.Equals(newLeadText, oldLeadText, StringComparison.CurrentCultureIgnoreCase) || lists.GetListEntryInfo(Utility.LeadListName, newLeadText) == null)
                            {
                                leadItem.Text = newLeadText;
                                lists.UpdateListEntry(leadItem);
                                this.LeadItemsGridView.EditIndex = -1;
                                BindLeadItems();
                            }
                            else
                            {
                                cvLeadEdit.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void LeadItemsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? leadId = GetLeadItemId(e.RowIndex);
            if (leadId.HasValue)
            {
                (new ListController()).DeleteListEntryByID(leadId.Value, true);
                BindLeadItems();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void LeadItemsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            if (e != null)
            {
                this.LeadItemsGridView.EditIndex = e.NewEditIndex;
                BindLeadItems();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        private void LeadItemsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.LeadItemsGridView.EditIndex = -1;
            BindLeadItems();
        }
        #endregion
        #endregion

        #region Lead GridView Helpers
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

            rowNewLeadItemHeader.Visible = (leadItems == null || leadItems.Count < 1);
        }

        private string GetLeadItemText(int rowIndex)
        {
            if (this.LeadItemsGridView != null && this.LeadItemsGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.LeadItemsGridView.Rows[rowIndex];
                TextBox txtLeadText = row.FindControl("txtLeadText") as TextBox;

                return txtLeadText != null ? txtLeadText.Text : null;
            }
            return null;
        }

#pragma warning disable 1692
#pragma warning disable SuggestBaseTypeForParameter
        private static int? GetLeadItemId(GridViewRow row)
#pragma warning restore SuggestBaseTypeForParameter
#pragma warning restore 1692
        {
            HiddenField hdnLeadId = row.FindControl("hdnLeadId") as HiddenField;

            int leadId;
            if (hdnLeadId != null && int.TryParse(hdnLeadId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                return leadId;
            }
            return null;
        }

        private int? GetLeadItemId(int rowIndex)
        {
            if (this.LeadItemsGridView != null && this.LeadItemsGridView.Rows.Count > rowIndex)
            {
                return GetLeadItemId(this.LeadItemsGridView.Rows[rowIndex]);
            }
            return null;
        }
        #endregion

        #region Module Settings
        private bool EnableDnnSearch
        {
            get
            {
                return Engage.Dnn.Utility.GetBoolSetting(Settings, Utility.EnableDnnSearchSetting, true);
            }
        }

        private string ApplicationEmailAddress
        {
            get
            {
                return Engage.Dnn.Utility.GetStringSetting(Settings, "ApplicationEmailAddress", PortalSettings.Email);
            }
        }

        private string FriendEmailAddress
        {
            get
            {
                return Engage.Dnn.Utility.GetStringSetting(Settings, "FriendEmailAddress", PortalSettings.Email);
            }
        }

        private bool RequireRegistration
        {
            get
            {
                return Engage.Dnn.Utility.GetBoolSetting(Settings, "RequireRegistration", true);
            }
        }

        private Visibility DisplayLead
        {
            get
            {
                return Engage.Dnn.Utility.GetEnumSetting(Settings, "DisplayLead", Visibility.Hidden);
            }
        }

        private Visibility DisplaySalaryRequirement
        {
            get
            {
                return Engage.Dnn.Utility.GetEnumSetting(Settings, "DisplaySalaryRequirement", Visibility.Optional);
            }
        }

        private Visibility DisplayCoverLetter
        {
            get
            {
                return Engage.Dnn.Utility.GetEnumSetting(Settings, "DisplayCoverLetter", Visibility.Hidden);
            }
        }

        private Visibility DisplayMessage
        {
            get
            {
                return Engage.Dnn.Utility.GetEnumSetting(Settings, "DisplayMessage", Visibility.Optional);
            }
        }
        #endregion
    }
}

