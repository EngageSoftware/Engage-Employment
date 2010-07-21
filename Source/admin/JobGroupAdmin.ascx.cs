// <copyright file="JobGroupAdmin.ascx.cs" company="Engage Software">
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment.Admin
{
    partial class JobGroupAdmin : ModuleBase
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Init(object sender, EventArgs e)
        {
            try
            {
                this.JobGroupsGridView.RowDataBound += this.JobGroupsGridView_RowDataBound;
                this.JobGroupsGridView.RowCancelingEdit += this.JobGroupsGridView_RowCancelingEdit;
                this.JobGroupsGridView.RowEditing += this.JobGroupsGridView_RowEditing;
                this.JobGroupsGridView.RowCommand += this.JobGroupsGridView_RowCommand;
                this.JobGroupsGridView.RowDeleting += this.JobGroupsGridView_RowDeleting;
                rpJobs.ItemDataBound += rpJobs_ItemDataBound;
                this.SaveAssignmentsButton.Click += this.SaveAssignmentsButton_Click;
                this.NewJobGroupButton.Click += this.NewJobGroupButton_Click;
                EditAssignmentsButton.Click += EditAssignmentsButton_Click;
                this.SaveNewJobGroupButton.Click += this.SaveNewJobGroupButton_Click;
                this.EditJobGroupsButton.Click += this.EditJobGroupsButton_Click;
                cvNewJobGroup.ServerValidate += cvNewJobGroup_ServerValidate;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                this.AuthorizationMultiview.SetActiveView(IsEditable ? vwJobGroups : vwUnauthorized);
                if (!Page.IsPostBack)
                {
                    Engage.Dnn.Utility.LocalizeGridView(ref this.JobGroupsGridView, LocalResourceFile);

                    BindJobGroups();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindJobGroups()
        {
            DataTable jobGroups = DataProvider.Instance().GetJobGroups(PortalId);

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

            rowNewJobGroupHeader.Visible = (jobGroups == null || jobGroups.Rows.Count < 1);
            this.EditAssignmentsButton.Visible = (jobGroups != null && jobGroups.Rows.Count > 0);
        }

        private void BindJobGroupAssignments()
        {
            DataSet ds = DataProvider.Instance().GetAssignedJobGroups(PortalId);
            rpJobs.DataSource = ds.Tables["Jobs"];
            rpJobs.DataBind();
        }

        #region vwAssignment Events
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void rpJobs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                ListControl listJobGroups = (ListControl)e.Item.FindControl("cblJobGroups");
                DataRowView row = (DataRowView)e.Item.DataItem;
                listJobGroups.DataSource = DataProvider.Instance().GetJobGroups(PortalId);
                listJobGroups.DataTextField = "Name";
                listJobGroups.DataValueField = "JobGroupId";
                listJobGroups.DataBind();

                ListItem li;
                foreach (DataRow listingRow in row.Row.GetChildRows(row.DataView.Table.ChildRelations["JobJobGroup"]))
                {
                    li = listJobGroups.Items.FindByValue(listingRow["JobGroupId"].ToString());
                    if (li != null)
                    {
                        li.Selected = true; 
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void SaveAssignmentsButton_Click(object sender, EventArgs e)
        {
            foreach (RepeaterItem row in rpJobs.Items)
            {
                int jobId;
                HiddenField hdnJobId = row.FindControl("hdnJobId") as HiddenField;
                if (hdnJobId != null && int.TryParse(hdnJobId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                {
                    ListControl listJobGroups = row.FindControl("cblJobGroups") as ListControl;
                    if (listJobGroups != null)
                    {
                        List<int> jobGroupIds = new List<int>();
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
                }
            }

            this.AuthorizationMultiview.SetActiveView(vwJobGroups);
            BindJobGroups();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void EditJobGroupsButton_Click(object sender, EventArgs e)
        {
            this.AuthorizationMultiview.SetActiveView(vwJobGroups);
        }
        #endregion

        #region vwJobGroups Events
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void JobGroupsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e != null && e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    Button btnDelete = e.Row.FindControl("btnDelete") as Button;
                    if (btnDelete != null)
                    {
                        int? jobGroupId = GetJobGroupId(e.Row);
                        if (jobGroupId.HasValue && DataProvider.Instance().IsJobGroupUsed(jobGroupId.Value))
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(CultureInfo.CurrentCulture, "return confirm('{0}');", Localization.GetString("DeleteConfirm", LocalResourceFile).Replace("'", "\'"));
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void EditAssignmentsButton_Click(object sender, EventArgs e)
        {
            this.AuthorizationMultiview.SetActiveView(vwAssignment);
            BindJobGroupAssignments();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void NewJobGroupButton_Click(object sender, EventArgs e)
        {
            this.NewJobGroupPanel.Visible = true;
            txtNewJobGroupName.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void SaveNewJobGroupButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                DataProvider.Instance().InsertJobGroup(txtNewJobGroupName.Text, PortalId);
                this.NewJobGroupPanel.Visible = false;
                txtNewJobGroupName.Text = string.Empty;
                BindJobGroups();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void cvNewJobGroup_ServerValidate(object sender, ServerValidateEventArgs e)
        {
            if (e != null && Engage.Utility.HasValue(e.Value))
            {
                e.IsValid = !DataProvider.Instance().IsJobGroupNameUsed(e.Value, PortalId);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void JobGroupsGridView_RowCommand(object sender, CommandEventArgs e)
        {
            if (Page.IsValid && e != null)
            {
                int rowIndex;
                if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                {
                    if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
                    {
                        int? jobGroupId = GetJobGroupId(rowIndex);
                        if (jobGroupId.HasValue)
                        {
                            string newJobGroupName = GetJobGroupName(rowIndex);
                            string oldJobGroupName = string.Empty;
                            DataTable jobGroup = DataProvider.Instance().GetJobGroup(jobGroupId.Value);
                            Debug.Assert(jobGroup.Rows.Count > 0);
                            if (jobGroup.Rows.Count > 0)
                            {
                                oldJobGroupName = jobGroup.Rows[0]["Name"] as string;
                            }
                            if (string.Equals(newJobGroupName, oldJobGroupName, StringComparison.CurrentCultureIgnoreCase) || !DataProvider.Instance().IsJobGroupNameUsed(newJobGroupName, PortalId))
                            {
                                DataProvider.Instance().UpdateJobGroup(jobGroupId.Value, newJobGroupName);
                                this.JobGroupsGridView.EditIndex = -1;
                                BindJobGroups();
                            }
                            else
                            {
                                cvJobGroupEdit.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void JobGroupsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? jobGroupId = GetJobGroupId(e.RowIndex);
            if (jobGroupId.HasValue)
            {
                DataProvider.Instance().DeleteJobGroup(jobGroupId.Value);
                BindJobGroups();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void JobGroupsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            if (e != null)
            {
                this.JobGroupsGridView.EditIndex = e.NewEditIndex;
                BindJobGroups();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member")]
        private void JobGroupsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.JobGroupsGridView.EditIndex = -1;
            BindJobGroups();
        }

        private string GetJobGroupName(int rowIndex)
        {
            if (this.JobGroupsGridView != null && this.JobGroupsGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.JobGroupsGridView.Rows[rowIndex];
                TextBox txtJobGroupName = row.FindControl("txtJobGroupName") as TextBox;

                return txtJobGroupName != null ? txtJobGroupName.Text : null;
            }
            return null;
        }

        private static int? GetJobGroupId(Control row)
        {
            HiddenField hdnJobGroupId = (HiddenField)row.FindControl("hdnJobGroupId");

            int jobGroupId;
            if (hdnJobGroupId != null && int.TryParse(hdnJobGroupId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out jobGroupId))
            {
                return jobGroupId;
            }
            return null;
        }

        private int? GetJobGroupId(int rowIndex)
        {
            if (this.JobGroupsGridView != null && this.JobGroupsGridView.Rows.Count > rowIndex)
            {
                return GetJobGroupId(this.JobGroupsGridView.Rows[rowIndex]);
            }
            return null;
        }
        #endregion
    }
}