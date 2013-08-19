// <copyright file="JobGroupAdmin.ascx.cs" company="Engage Software">
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
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Engage.Dnn.Employment.Data;

    public partial class JobGroupAdmin : ModuleBase
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Letting execeptions go blows up the whole page, instead of just the module")]
        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                this.JobGroupsGridView.RowDataBound += this.JobGroupsGridView_RowDataBound;
                this.JobGroupsGridView.RowCancelingEdit += this.JobGroupsGridView_RowCancelingEdit;
                this.JobGroupsGridView.RowEditing += this.JobGroupsGridView_RowEditing;
                this.JobGroupsGridView.RowCommand += this.JobGroupsGridView_RowCommand;
                this.JobGroupsGridView.RowDeleting += this.JobGroupsGridView_RowDeleting;
                this.JobsRepeater.ItemDataBound += this.JobsRepeater_ItemDataBound;
                this.SaveAssignmentsButton.Click += this.SaveAssignmentsButton_Click;
                this.NewJobGroupButton.Click += this.NewJobGroupButton_Click;
                this.EditAssignmentsButton.Click += this.EditAssignmentsButton_Click;
                this.SaveNewJobGroupButton.Click += this.SaveNewJobGroupButton_Click;
                this.EditJobGroupsButton.Click += this.EditJobGroupsButton_Click;
                this.NewJobGroupValidator.ServerValidate += this.NewJobGroupValidator_ServerValidate;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Letting execeptions go blows up the whole page, instead of just the module")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.AuthorizationMultiview.SetActiveView(this.IsEditable ? this.JobGroupsView : this.UnauthorizedView);
                if (Page.IsPostBack)
                {
                    return;
                }

                Localization.LocalizeGridView(ref this.JobGroupsGridView, this.LocalResourceFile);

                this.BindJobGroups();
            }
            catch (Exception exc) 
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private static int? GetJobGroupId(Control row)
        {
            var jobGroupIdHiddenField = (HiddenField)row.FindControl("JobGroupIdHiddenField");

            int jobGroupId;
            if (jobGroupIdHiddenField != null && int.TryParse(jobGroupIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out jobGroupId))
            {
                return jobGroupId;
            }

            return null;
        }

        private void BindJobGroups()
        {
            var jobGroups = DataProvider.Instance().GetJobGroups(PortalId);

            this.JobGroupsGridView.DataSource = jobGroups;
            this.JobGroupsGridView.DataBind();

            if (jobGroups == null || jobGroups.Rows.Count % 2 == 0)
            {
                this.NewJobGroupPanel.CssClass = this.JobGroupsGridView.RowStyle.CssClass;
            }
            else
            {
                this.NewJobGroupPanel.CssClass = this.JobGroupsGridView.AlternatingRowStyle.CssClass;
            }

            this.rowNewJobGroupHeader.Visible = jobGroups == null || jobGroups.Rows.Count < 1;
            this.EditAssignmentsButton.Visible = jobGroups != null && jobGroups.Rows.Count > 0;
        }

        private void BindJobGroupAssignments()
        {
            var ds = DataProvider.Instance().GetAssignedJobGroups(PortalId);
            this.JobsRepeater.DataSource = ds.Tables["Jobs"];
            this.JobsRepeater.DataBind();
        }

        private void JobsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e == null || (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem))
            {
                return;
            }

            var listJobGroups = (ListControl)e.Item.FindControl("cblJobGroups");
            var row = (DataRowView)e.Item.DataItem;
            var jobGroupsTable = DataProvider.Instance().GetJobGroups(this.PortalId);
            var jobGroupsList = new Dictionary<int, string>(jobGroupsTable.Rows.Count);
            foreach (DataRow jobGroupRow in jobGroupsTable.Rows)
            {
                jobGroupsList.Add((int)jobGroupRow["JobGroupId"], HttpUtility.HtmlEncode((string)jobGroupRow["Name"]));
            }

            listJobGroups.DataSource = jobGroupsList;
            listJobGroups.DataTextField = "Value";
            listJobGroups.DataValueField = "Key";
            listJobGroups.DataBind();

            foreach (var listingRow in row.Row.GetChildRows(row.DataView.Table.ChildRelations["JobJobGroup"]))
            {
                var li = listJobGroups.Items.FindByValue(listingRow["JobGroupId"].ToString());
                if (li != null)
                {
                    li.Selected = true; 
                }
            }
        }

        private void SaveAssignmentsButton_Click(object sender, EventArgs e)
        {
            foreach (RepeaterItem row in this.JobsRepeater.Items)
            {
                int jobId;
                var hdnJobId = row.FindControl("hdnJobId") as HiddenField;
                if (hdnJobId == null || !int.TryParse(hdnJobId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                {
                    continue;
                }

                var listJobGroups = row.FindControl("cblJobGroups") as ListControl;
                if (listJobGroups == null)
                {
                    continue;
                }

                var jobGroupIds = new List<int>();
                foreach (ListItem li in listJobGroups.Items)
                {
                    int jobGroupId;
                    if (li.Selected && int.TryParse(li.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out jobGroupId))
                    {
                        jobGroupIds.Add(jobGroupId);
                    }
                }

                DataProvider.Instance().AssignJobToJobGroups(jobId, jobGroupIds);
            }

            this.AuthorizationMultiview.SetActiveView(this.JobGroupsView);
            this.BindJobGroups();
        }

        private void EditJobGroupsButton_Click(object sender, EventArgs e)
        {
            this.AuthorizationMultiview.SetActiveView(this.JobGroupsView);
        }

        private void JobGroupsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e == null || e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }
            
            var row = e.Row;
            if (row == null)
            {
                return;
            }

            var deleteButton = e.Row.FindControl("DeleteButton") as Button;
            if (deleteButton == null)
            {
                return;
            }

            var jobGroupId = GetJobGroupId(e.Row);
            if (jobGroupId.HasValue && DataProvider.Instance().IsJobGroupUsed(jobGroupId.Value))
            {
                deleteButton.Enabled = false;
                return;
            }

            deleteButton.OnClientClick = string.Format(
                CultureInfo.CurrentCulture,
                "return confirm('{0}');",
                this.Localize("DeleteConfirm").Replace("'", "\'"));
        }

        private void EditAssignmentsButton_Click(object sender, EventArgs e)
        {
            this.AuthorizationMultiview.SetActiveView(this.AssignmentView);
            this.BindJobGroupAssignments();
        }

        private void NewJobGroupButton_Click(object sender, EventArgs e)
        {
            this.NewJobGroupPanel.Visible = true;
            this.txtNewJobGroupName.Focus();
        }

        private void SaveNewJobGroupButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            DataProvider.Instance().InsertJobGroup(this.txtNewJobGroupName.Text, this.PortalId);
            this.NewJobGroupPanel.Visible = false;
            this.txtNewJobGroupName.Text = string.Empty;
            this.BindJobGroups();
        }

        private void NewJobGroupValidator_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (e == null || !Engage.Utility.HasValue(e.Value))
            {
                return;
            }

            e.IsValid = !DataProvider.Instance().IsJobGroupNameUsed(e.Value, this.PortalId);
        }

        private void JobGroupsGridView_RowCommand(object sender, CommandEventArgs e)
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

            var jobGroupId = this.GetJobGroupId(rowIndex);
            if (!jobGroupId.HasValue)
            {
                return;
            }

            var newJobGroupName = this.GetJobGroupName(rowIndex);
            var oldJobGroupName = string.Empty;
            var jobGroup = DataProvider.Instance().GetJobGroup(jobGroupId.Value);
            Debug.Assert(jobGroup.Rows.Count > 0, "Specified Job Group doesn't exist");
            if (jobGroup.Rows.Count > 0)
            {
                oldJobGroupName = jobGroup.Rows[0]["Name"] as string;
            }

            if (!string.Equals(newJobGroupName, oldJobGroupName, StringComparison.CurrentCultureIgnoreCase)
                && DataProvider.Instance().IsJobGroupNameUsed(newJobGroupName, this.PortalId))
            {
                this.cvJobGroupEdit.IsValid = false;
                return;
            }
            
            DataProvider.Instance().UpdateJobGroup(jobGroupId.Value, newJobGroupName);
            this.JobGroupsGridView.EditIndex = -1;
            this.BindJobGroups();
        }

        private void JobGroupsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var jobGroupId = GetJobGroupId(e.RowIndex);
            if (!jobGroupId.HasValue)
            {
                return;
            }

            DataProvider.Instance().DeleteJobGroup(jobGroupId.Value);
            this.BindJobGroups();
        }

        private void JobGroupsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            this.JobGroupsGridView.EditIndex = e.NewEditIndex;
            this.BindJobGroups();
        }

        private void JobGroupsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.JobGroupsGridView.EditIndex = -1;
            this.BindJobGroups();
        }

        private string GetJobGroupName(int rowIndex)
        {
            if (this.JobGroupsGridView == null || this.JobGroupsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.JobGroupsGridView.Rows[rowIndex];
            var jobGroupNameTextBox = row.FindControl("JobGroupNameTextBox") as TextBox;

            return jobGroupNameTextBox != null ? jobGroupNameTextBox.Text : null;
        }

        private int? GetJobGroupId(int rowIndex)
        {
            if (this.JobGroupsGridView != null && this.JobGroupsGridView.Rows.Count > rowIndex)
            {
                return GetJobGroupId(this.JobGroupsGridView.Rows[rowIndex]);
            }

            return null;
        }
    }
}