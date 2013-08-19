// <copyright file="LocationListing.ascx.cs" company="Engage Software">
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

    public partial class LocationListing : ModuleBase
    {
        protected const int LocationMaxLength = 255;

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(LocationMaxLength); }
        }

        protected string MaxLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("LocationMaxLength"), LocationMaxLength); }
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
            this.LocationsGridView.RowCancelingEdit += this.LocationsGridView_RowCancelingEdit;
            this.LocationsGridView.RowCommand += this.LocationsGridView_RowCommand;
            this.LocationsGridView.RowDataBound += this.LocationsGridView_RowDataBound;
            this.LocationsGridView.RowDeleting += this.LocationsGridView_RowDeleting;
            this.LocationsGridView.RowEditing += this.LocationsGridView_RowEditing;

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.IsPostBack)
                {
                    return;
                }

                Localization.LocalizeGridView(ref this.LocationsGridView, this.LocalResourceFile);
                this.SetupLengthValidation();
                this.BindStates(this.NewStateDropDownList);
                this.LoadLocations();
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

        private static int? GetLocationId(Control row)
        {
            var locationIdHiddenField = (HiddenField)row.FindControl("LocationIdHiddenField");

            int locationId;
            if (locationIdHiddenField != null && int.TryParse(locationIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out locationId))
            {
                return locationId;
            }

            return null;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            this.NewPanel.Visible = true;
            this.txtNewLocationName.Focus();
        }

        private void SaveNewButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            int stateId;
            if (!int.TryParse(this.NewStateDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId)
                || !this.IsLocationNameUnique(null, this.txtNewLocationName.Text, stateId))
            {
                this.cvDuplicateLocation.IsValid = false;
                return;
            }

            Location.InsertLocation(this.txtNewLocationName.Text, stateId, this.PortalId);
            this.HideAndClearNewStatusPanel();
            this.LoadLocations();
        }

        private void CancelNewButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewStatusPanel();
        }

        private void LocationsGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.LocationsGridView.EditIndex = -1;
            this.LoadLocations();
        }

        private void LocationsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
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

            var location = (Location)e.Row.DataItem;
            if ((e.Row.RowState & DataControlRowState.Edit) != 0)
            {
                var ddlState = (DropDownList)row.FindControl("ddlState");
                this.BindStates(ddlState, location.StateId);
                return;
            }
            
            var deleteButton = (Button)row.FindControl("DeleteButton");
            if (location.IsUsed())
            {
                deleteButton.Enabled = false;
                return;
            }
            
            deleteButton.OnClientClick = string.Format(
                CultureInfo.CurrentCulture,
                "return confirm('{0}');",
                ClientAPI.GetSafeJSString(this.Localize("DeleteConfirm")));
        }

        private void LocationsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var locationId = GetLocationId(e.RowIndex);
            if (!locationId.HasValue)
            {
                return;
            }

            Location.DeleteLocation(locationId.Value);
            this.LoadLocations();
        }

        private void LocationsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.LocationsGridView.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatusPanel();
            this.LoadLocations();
        }

        private void LocationsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
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

            var locationId = this.GetLocationId(rowIndex);
            if (!locationId.HasValue)
            {
                return;
            }

            var stateId = this.GetStateId(rowIndex);
            var newLocationName = this.GetLocationName(rowIndex);
            if (!this.IsLocationNameUnique(locationId, newLocationName, stateId))
            {
                this.cvDuplicateLocation.IsValid = false;
                return;
            }
            
            Location.UpdateLocation(locationId.Value, newLocationName, stateId.Value);
            this.LocationsGridView.EditIndex = -1;
            this.LoadLocations();
        }

        private bool IsLocationNameUnique(int? locationId, string newLocationName, int? stateId)
        {
            var newLocationId = Location.GetLocationId(newLocationName, stateId, PortalId);
            return (!newLocationId.HasValue || (locationId.HasValue && newLocationId.Value == locationId.Value)) && stateId.HasValue;
        }

        private void LoadLocations()
        {
            var locations = Location.LoadLocations(null, PortalId);
            this.LocationsGridView.DataSource = locations;
            this.LocationsGridView.DataBind();

            this.NewPanel.CssClass = locations.Count % 2 == 0 
                ? this.LocationsGridView.RowStyle.CssClass 
                : this.LocationsGridView.AlternatingRowStyle.CssClass;

            this.rowNewHeader.Visible = locations.Count < 1;
        }

        private void BindStates(ListControl ddlState) 
        {
            this.BindStates(ddlState, null);
        }

        private void BindStates(ListControl ddlState, int? stateId) 
        {
            ddlState.DataSource = State.LoadStates(null, this.PortalId);
            ddlState.DataValueField = "StateId";
            ddlState.DataTextField = "StateName";
            ddlState.DataBind();

            if (stateId.HasValue)
            {
                ddlState.SelectedValue = stateId.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void SetupLengthValidation()
        {
            this.regexNewLocationName.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewLocationName.ErrorMessage = this.MaxLengthValidationText;
        }

        private void HideAndClearNewStatusPanel()
        {
            this.NewPanel.Visible = false;
            this.txtNewLocationName.Text = string.Empty;
            this.NewStateDropDownList.ClearSelection();
        }

        private string GetLocationName(int rowIndex)
        {
            if (this.LocationsGridView == null || this.LocationsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.LocationsGridView.Rows[rowIndex];
            var locationNameTextBox = row.FindControl("LocationNameTextBox") as TextBox;
                
            Debug.Assert(locationNameTextBox != null, "LocationNameTextBox not found in row");
            return locationNameTextBox.Text;
        }

        private int? GetStateId(int rowIndex)
        {
            if (this.LocationsGridView == null || this.LocationsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            var row = this.LocationsGridView.Rows[rowIndex];
            var ddlState = row.FindControl("ddlState") as DropDownList;
            Debug.Assert(ddlState != null, "dllState not found in row");

            int stateId;
            if (int.TryParse(ddlState.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId))
            {
                return stateId;
            }

            return null;
        }

        private int? GetLocationId(int rowIndex)
        {
            if (this.LocationsGridView == null || this.LocationsGridView.Rows.Count <= rowIndex)
            {
                return null;
            }

            return GetLocationId(this.LocationsGridView.Rows[rowIndex]);
        }
    }
}