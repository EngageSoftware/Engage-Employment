//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

namespace Engage.Dnn.Employment.Admin
{
    partial class LocationListing : ModuleBase
    {
        protected const int LocationMaxLength = 255;

        protected string MaxLengthValidationText
        {
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("LocationMaxLength", LocalResourceFile), LocationMaxLength); }
        }

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(LocationMaxLength); }
        }

        protected override void OnInit(EventArgs e)
        {
            this.Load += Page_Load;
            btnAdd.Click += btnAdd_Click;
            btnBack.Click += btnBack_Click;
            btnCancelNew.Click += btnCancelNew_Click;
            btnSaveNew.Click += btnSaveNew_Click;
            gvLocations.RowCancelingEdit += gvLocations_RowCancelingEdit;
            gvLocations.RowCommand += gvLocations_RowCommand;
            gvLocations.RowDataBound += gvLocations_RowDataBound;
            gvLocations.RowDeleting += gvLocations_RowDeleting;
            gvLocations.RowEditing += gvLocations_RowEditing;

            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(Object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Engage.Dnn.Utility.LocalizeGridView(ref gvLocations, LocalResourceFile);
                    SetupLengthValidation();
                    BindStates(ddlNewState);
                    LoadLocations();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl(ControlKey.Edit.ToString()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnAdd_Click(object sender, EventArgs e)
        {
            pnlNew.Visible = true;
            txtNewLocationName.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnSaveNew_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                int stateId;
                if (int.TryParse(ddlNewState.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId)
                    && IsLocationNameUnique(null, txtNewLocationName.Text, stateId))
                {
                    Location.InsertLocation(txtNewLocationName.Text, stateId, PortalId);
                    HideAndClearNewStatusPanel();
                    LoadLocations();
                }
                else
                {
                    cvDuplicateLocation.IsValid = false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnCancelNew_Click(object sender, EventArgs e)
        {
            HideAndClearNewStatusPanel();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvLocations_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvLocations.EditIndex = -1;
            LoadLocations();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvLocations_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    Location location = (Location)e.Row.DataItem;
                    if ((e.Row.RowState & DataControlRowState.Edit) == 0)
                    {
                        Button btnDelete = (Button)row.FindControl("btnDelete");
                        if (location.IsUsed())
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(CultureInfo.CurrentCulture, "return confirm('{0}');",
                                    ClientAPI.GetSafeJSString(Localization.GetString("DeleteConfirm", LocalResourceFile)));
                        }
                    }
                    else if ((e.Row.RowState & DataControlRowState.Edit) != 0)
                    {
                        DropDownList ddlState = (DropDownList)row.FindControl("ddlState");
                        BindStates(ddlState, location.StateId);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvLocations_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? locationId = GetLocationId(e.RowIndex);
            if (locationId.HasValue)
            {
                Location.DeleteLocation(locationId.Value);
                LoadLocations();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvLocations_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvLocations.EditIndex = e.NewEditIndex;
            HideAndClearNewStatusPanel();
            LoadLocations();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvLocations_RowCommand(object sender, GridViewCommandEventArgs e)
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
                            int? stateId = GetStateId(rowIndex);
                            string newLocationName = GetLocationName(rowIndex);
                            if (IsLocationNameUnique(locationId, newLocationName, stateId))
                            {
                                Location.UpdateLocation(locationId.Value, newLocationName, stateId.Value);
                                gvLocations.EditIndex = -1;
                                LoadLocations();
                            }
                            else
                            {
                                cvDuplicateLocation.IsValid = false;
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
            this.gvLocations.DataSource = locations;
            this.gvLocations.DataBind();

            if (locations == null || locations.Count % 2 == 0)
            {
                pnlNew.CssClass = gvLocations.RowStyle.CssClass;
            }
            else
            {
                pnlNew.CssClass = gvLocations.AlternatingRowStyle.CssClass;
            }

            rowNewHeader.Visible = (locations == null || locations.Count < 1);
        }

        private void BindStates(ListControl ddlState) 
        {
            BindStates(ddlState, null);
        }

        private void BindStates(ListControl ddlState, int? stateId) 
        {
            ddlState.DataSource = State.LoadStates(null, PortalId);
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
            this.regexNewLocationName.ErrorMessage = MaxLengthValidationText;
        }

        private void HideAndClearNewStatusPanel()
        {
            this.pnlNew.Visible = false;
            this.txtNewLocationName.Text = string.Empty;
            this.ddlNewState.ClearSelection();
        }

        private string GetLocationName(int rowIndex)
        {
            if (gvLocations != null && gvLocations.Rows.Count > rowIndex)
            {
                GridViewRow row = gvLocations.Rows[rowIndex];
                TextBox txtLocationName = row.FindControl("txtLocationName") as TextBox;
                Debug.Assert(txtLocationName != null);
                return txtLocationName.Text;
            }
            return null;
        }

        private int? GetStateId(int rowIndex)
        {
            if (gvLocations != null && gvLocations.Rows.Count > rowIndex)
            {
                GridViewRow row = gvLocations.Rows[rowIndex];
                DropDownList ddlState = row.FindControl("ddlState") as DropDownList;
                Debug.Assert(ddlState != null);

                int stateId;
                if (int.TryParse(ddlState.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out stateId))
                {
                    return stateId;
                }
            }
            return null;
        }

        private static int? GetLocationId(Control row)
        {
            HiddenField hdnLocationId = (HiddenField)row.FindControl("hdnLocationId");

            int locationId;
            if (hdnLocationId != null && int.TryParse(hdnLocationId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out locationId))
            {
                return locationId;
            }
            return null;
        }

        private int? GetLocationId(int rowIndex)
        {
            if (gvLocations != null && gvLocations.Rows.Count > rowIndex)
            {
                return GetLocationId(gvLocations.Rows[rowIndex]);
            }
            return null;
        }
    }
}