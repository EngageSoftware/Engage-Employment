// <copyright file="JobSearch.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2009
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
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using Globals = DotNetNuke.Common.Globals;

    partial class JobSearch : ModuleBase
    {
        // , IActionable
        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.Load += this.Page_Load;
            this.rblLocation.SelectedIndexChanged += this.rblLocation_SelectedIndexChanged;
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.Control.Load"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    this.InitializeLocationButtons();
                    this.LoadJobTitles();
                    this.LoadLocations();
                    this.LoadCategories();

                    int searchId;
                    if (int.TryParse(this.Request.QueryString["usid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out searchId))
                    {
                        JobSearchQuery q = JobSearchQuery.Load(searchId);

                        this.rpSearchResults.DataSource = q.Execute();
                        this.rpSearchResults.DataBind();

                        this.UpdateSearchFields(q);
                    }

                    ClientAPI.RegisterKeyCapture(this.pnlSearchInput, this.btnSearch, (int)Keys.Enter);
                    ClientAPI.RegisterKeyCapture(this.pnlSaveSearch, this.btnSaveSearch, (int)Keys.Enter);
                }

                this.pnlSaveSearch.Visible = Engage.Utility.IsLoggedIn && this.IsPostBack;
                this.btnBack.Visible = Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) != this.TabId;
            }
            catch (Exception exc)
            {
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.ListControl.SelectedIndexChanged"/> event of the <see cref="rblLocation"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rblLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadLocations();
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="btnSearch"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = this.GetQueryObject().Execute();

                if (dt.Rows.Count > 0)
                {
                    this.rpSearchResults.DataSource = dt;
                    this.rpSearchResults.DataBind();
                }
                else
                {
                    this.lblNoResults.Visible = true;
                }

                this.pnlSaveSearch.Visible = Engage.Utility.IsLoggedIn;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="btnSaveSearch"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnSaveSearch_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                this.GetQueryObject().Save(this.UserId);
            }
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="btnBack"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnBack_Click(object source, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) ?? this.TabId));
        }

        #endregion

        private void InitializeLocationButtons()
        {
            this.rblLocation.Items.Clear();
            this.rblLocation.Items.Add(new ListItem(Localization.GetString("liCity", this.LocalResourceFile), LocationType.City.ToString()));
            this.rblLocation.Items.Add(new ListItem(Localization.GetString("liState", this.LocalResourceFile), LocationType.State.ToString()));

            this.rblLocation.Items[0].Selected = true;
        }

        private void LoadJobTitles()
        {
            this.ddlJobTitle.DataSource = Position.LoadPositions(this.JobGroupId, this.PortalId);
            this.ddlJobTitle.DataTextField = "JobTitle";
            this.ddlJobTitle.DataValueField = "PositionId";
            this.ddlJobTitle.DataBind();

            this.ddlJobTitle.Items.Insert(0, new ListItem(string.Empty));
        }

        private void LoadLocations()
        {
            this.lblLocationHeader.Text = Localization.GetString(this.rblLocation.SelectedValue, this.LocalResourceFile);

            if (string.Equals(this.rblLocation.SelectedValue, LocationType.City.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                this.ddlLocation.Items.Clear();
                foreach (Location location in Location.LoadLocations(this.JobGroupId, this.PortalId))
                {
                    this.ddlLocation.Items.Add(new ListItem(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Localization.GetString("Location.Text", this.LocalResourceFile),
                            location.LocationName,
                            location.StateName,
                            location.StateAbbreviation),
                        location.LocationId.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }
            else
            {
                this.ddlLocation.DataSource = State.LoadStates(this.JobGroupId, this.PortalId);
                this.ddlLocation.DataTextField = "StateName";
                this.ddlLocation.DataValueField = "StateId";
                this.ddlLocation.DataBind();
            }

            this.ddlLocation.Items.Insert(0, new ListItem(string.Empty));
        }

        private void LoadCategories()
        {
            this.ddlCategory.DataSource = Category.LoadCategories(this.JobGroupId, this.PortalId);
            this.ddlCategory.DataTextField = "CategoryName";
            this.ddlCategory.DataValueField = "CategoryId";
            this.ddlCategory.DataBind();

            this.ddlCategory.Items.Insert(0, new ListItem(string.Empty));
        }

        private void UpdateSearchFields(JobSearchQuery q)
        {
            this.txtKeywords.Text = q.Keywords;
            if (q.JobPositionId.HasValue)
            {
                this.ddlJobTitle.SelectedValue = q.JobPositionId.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (q.CategoryId.HasValue)
            {
                this.ddlCategory.SelectedValue = q.CategoryId.Value.ToString(CultureInfo.InvariantCulture);
            }

            string value = string.Empty;
            if (!q.LocationId.HasValue)
            {
                this.rblLocation.SelectedValue = LocationType.State.ToString();
                this.LoadLocations();
                if (q.StateId.HasValue)
                {
                    value = q.StateId.Value.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                this.rblLocation.SelectedValue = LocationType.City.ToString();
                this.LoadLocations();
                value = q.LocationId.Value.ToString(CultureInfo.InvariantCulture);
            }

            this.ddlLocation.SelectedValue = value;
        }

        private JobSearchQuery GetQueryObject()
        {
            JobSearchQuery q = new JobSearchQuery();

            q.Description = this.txtSearchName.Text;

            int result;
            if (int.TryParse(this.ddlJobTitle.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                q.JobPositionId = result;
            }

            if (int.TryParse(this.ddlCategory.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                q.CategoryId = result;
            }

            if (string.Equals(this.rblLocation.SelectedValue, LocationType.City.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(this.ddlLocation.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    q.LocationId = result;
                }
            }
            else
            {
                if (int.TryParse(this.ddlLocation.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    q.StateId = result;
                }
            }

            if (Engage.Utility.HasValue(this.txtKeywords.Text))
            {
                q.Keywords = this.txtKeywords.Text;
            }

            q.JobGroupId = this.JobGroupId;

            return q;
        }

        protected string GetJobDetailUrl(object jobId)
        {
            return Utility.GetJobDetailUrl(jobId, this.JobGroupId, this.PortalSettings);
        }
    }
}