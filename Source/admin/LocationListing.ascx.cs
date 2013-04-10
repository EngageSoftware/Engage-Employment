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
    using System.Collections.Generic;
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
                if (!this.IsPostBack)
                {
                    Localization.LocalizeGridView(ref this.LocationsGridView, this.LocalResourceFile);
                    this.SetupLengthValidation();
                    this.BindStates(this.NewStateDropDownList);
                    this.LoadLocations();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        private static int? GetLocationId(Control row)
        {
            var hdnLocationId = (HiddenField)row.FindControl("hdnLocationId");

            int locationId;
            if (hdnLocationId != null && int.TryParse(hdnLocationId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out locationId))
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
            if (Page.IsValid)
            {
                int stateId;
                if (int.TryParse(this.NewStateDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId)
                    && this.IsLocationNameUnique(null, this.txtNewLocationName.Text, stateId))
                {
                    Location.InsertLocation(this.txtNewLocationName.Text, stateId, this.PortalId);
                    this.HideAndClearNewStatusPanel();
                    this.LoadLocations();
                }
                else
                {
                    this.cvDuplicateLocation.IsValid = false;
                }
            }
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
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    var location = (Location)e.Row.DataItem;
                    if ((e.Row.RowState & DataControlRowState.Edit) == 0)
                    {
                        var btnDelete = (Button)row.FindControl("btnDelete");
                        if (location.IsUsed())
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(
                                CultureInfo.CurrentCulture,
                                "return confirm('{0}');",
                                ClientAPI.GetSafeJSString(this.Localize("DeleteConfirm")));
                        }
                    }
                    else if ((e.Row.RowState & DataControlRowState.Edit) != 0)
                    {
                        var ddlState = (DropDownList)row.FindControl("ddlState");
                        this.BindStates(ddlState, location.StateId);
                    }
                }
            }
        }

        private void LocationsGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? locationId = GetLocationId(e.RowIndex);
            if (locationId.HasValue)
            {
                Location.DeleteLocation(locationId.Value);
                this.LoadLocations();
            }
        }

        private void LocationsGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.LocationsGridView.EditIndex = e.NewEditIndex;
            this.HideAndClearNewStatusPanel();
            this.LoadLocations();
        }

        private void LocationsGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? locationId = GetLocationId(rowIndex);
                        if (locationId.HasValue)
                        {
                            int? stateId = this.GetStateId(rowIndex);
                            string newLocationName = this.GetLocationName(rowIndex);
                            if (this.IsLocationNameUnique(locationId, newLocationName, stateId))
                            {
                                Location.UpdateLocation(locationId.Value, newLocationName, stateId.Value);
                                this.LocationsGridView.EditIndex = -1;
                                this.LoadLocations();
                            }
                            else
                            {
                                this.cvDuplicateLocation.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        private bool IsLocationNameUnique(int? locationId, string newLocationName, int? stateId)
        {
            int? newLocationId = Location.GetLocationId(newLocationName, stateId, PortalId);
            return (!newLocationId.HasValue || (locationId.HasValue && newLocationId.Value == locationId.Value)) && stateId.HasValue;
        }

        private void LoadLocations()
        {
            List<Location> locations = Location.LoadLocations(null, PortalId);
            this.LocationsGridView.DataSource = locations;
            this.LocationsGridView.DataBind();

            if (locations == null || locations.Count % 2 == 0)
            {
                this.NewPanel.CssClass = this.LocationsGridView.RowStyle.CssClass;
            }
            else
            {
                this.NewPanel.CssClass = this.LocationsGridView.AlternatingRowStyle.CssClass;
            }

            this.rowNewHeader.Visible = locations == null || locations.Count < 1;
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
            if (this.LocationsGridView != null && this.LocationsGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.LocationsGridView.Rows[rowIndex];
                var txtLocationName = row.FindControl("txtLocationName") as TextBox;
                
                Debug.Assert(txtLocationName != null, "txtLocationName not found in row");
                return txtLocationName.Text;
            }

            return null;
        }

        private int? GetStateId(int rowIndex)
        {
            if (this.LocationsGridView != null && this.LocationsGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.LocationsGridView.Rows[rowIndex];
                var ddlState = row.FindControl("ddlState") as DropDownList;
                Debug.Assert(ddlState != null, "dllState not found in row");

                int stateId;
                if (int.TryParse(ddlState.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId))
                {
                    return stateId;
                }
            }

            return null;
        }

        private int? GetLocationId(int rowIndex)
        {
            if (this.LocationsGridView != null && this.LocationsGridView.Rows.Count > rowIndex)
            {
                return GetLocationId(this.LocationsGridView.Rows[rowIndex]);
            }

            return null;
        }
    }
}