// <copyright file="JobSearch.ascx.cs" company="Engage Software">
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
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;
    using Globals = DotNetNuke.Common.Globals;

    public partial class JobSearch : ModuleBase
    {
        protected string GetJobDetailUrl(object jobId)
        {
            return Utility.GetJobDetailUrl(jobId, this.JobGroupId, this.PortalSettings);
        }

        /// <summary>
        /// Raises the <see cref="System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            this.Load += this.Page_Load;
            this.BackButton.Click += this.BackButton_Click;
            this.LocationRadioButtonList.SelectedIndexChanged += this.LocationRadioButtonList_SelectedIndexChanged;
            this.SaveSearchButton.Click += this.SaveSearchButton_Click;
            this.SearchButton.Click += this.SearchButton_Click;
            base.OnInit(e);
        }

        private JobSearchQuery GetQueryObject()
        {
            var query = new JobSearchQuery { Description = this.txtSearchName.Text, JobGroupId = this.JobGroupId };

            int result;
            if (int.TryParse(this.JobTitleDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                query.JobPositionId = result;
            }

            if (int.TryParse(this.CategoryDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                query.CategoryId = result;
            }

            if (string.Equals(this.LocationRadioButtonList.SelectedValue, LocationType.City.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(this.LocationDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    query.LocationId = result;
                }
            }
            else
            {
                if (int.TryParse(this.LocationDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    query.StateId = result;
                }
            }

            if (Engage.Utility.HasValue(this.txtKeywords.Text))
            {
                query.Keywords = this.txtKeywords.Text;
            }

            return query;
        }

        private void InitializeLocationButtons()
        {
            this.LocationRadioButtonList.Items.Clear();
            this.LocationRadioButtonList.Items.Add(new ListItem(this.Localize("liCity"), LocationType.City.ToString()));
            this.LocationRadioButtonList.Items.Add(new ListItem(this.Localize("liState"), LocationType.State.ToString()));

            this.LocationRadioButtonList.Items[0].Selected = true;
        }

        private void LoadCategories()
        {
            this.CategoryDropDownList.DataSource = Category.LoadCategories(this.JobGroupId, this.PortalId);
            this.CategoryDropDownList.DataTextField = "CategoryName";
            this.CategoryDropDownList.DataValueField = "CategoryId";
            this.CategoryDropDownList.DataBind();

            this.CategoryDropDownList.Items.Insert(0, new ListItem(string.Empty));
        }

        private void LoadJobTitles()
        {
            this.JobTitleDropDownList.DataSource = Position.LoadPositions(this.JobGroupId, this.PortalId);
            this.JobTitleDropDownList.DataTextField = "JobTitle";
            this.JobTitleDropDownList.DataValueField = "PositionId";
            this.JobTitleDropDownList.DataBind();

            this.JobTitleDropDownList.Items.Insert(0, new ListItem(string.Empty));
        }

        private void LoadLocations()
        {
            this.LocationHeaderLabel.Text = this.Localize(this.LocationRadioButtonList.SelectedValue);

            if (string.Equals(this.LocationRadioButtonList.SelectedValue, LocationType.City.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                this.LocationDropDownList.Items.Clear();
                foreach (Location location in Location.LoadLocations(this.JobGroupId, this.PortalId))
                {
                    this.LocationDropDownList.Items.Add(new ListItem(
                        string.Format(
                            CultureInfo.CurrentCulture, 
                            this.Localize("Location.Text"), 
                            location.LocationName, 
                            location.StateName, 
                            location.StateAbbreviation), 
                        location.LocationId.Value.ToString(CultureInfo.InvariantCulture)));
                }
            }
            else
            {
                this.LocationDropDownList.DataSource = State.LoadStates(this.JobGroupId, this.PortalId);
                this.LocationDropDownList.DataTextField = "StateName";
                this.LocationDropDownList.DataValueField = "StateId";
                this.LocationDropDownList.DataBind();
            }

            this.LocationDropDownList.Items.Insert(0, new ListItem(string.Empty));
        }

        private void UpdateSearchFields(JobSearchQuery q)
        {
            this.txtKeywords.Text = q.Keywords;
            if (q.JobPositionId.HasValue)
            {
                this.JobTitleDropDownList.SelectedValue = q.JobPositionId.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (q.CategoryId.HasValue)
            {
                this.CategoryDropDownList.SelectedValue = q.CategoryId.Value.ToString(CultureInfo.InvariantCulture);
            }

            string value = string.Empty;
            if (!q.LocationId.HasValue)
            {
                this.LocationRadioButtonList.SelectedValue = LocationType.State.ToString();
                this.LoadLocations();
                if (q.StateId.HasValue)
                {
                    value = q.StateId.Value.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                this.LocationRadioButtonList.SelectedValue = LocationType.City.ToString();
                this.LoadLocations();
                value = q.LocationId.Value.ToString(CultureInfo.InvariantCulture);
            }

            this.LocationDropDownList.SelectedValue = value;
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="BackButton"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object source, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) ?? this.TabId));
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.ListControl.SelectedIndexChanged"/> event of the <see cref="LocationRadioButtonList"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LocationRadioButtonList_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadLocations();
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.Control.Load"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
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

                        this.SearchResultsRepeater.DataSource = q.Execute();
                        this.SearchResultsRepeater.DataBind();

                        this.UpdateSearchFields(q);
                    }

                    ClientAPI.RegisterKeyCapture(this.SearchInputPanel, this.SearchButton, (int)Keys.Enter);
                    ClientAPI.RegisterKeyCapture(this.SaveSearchPanel, this.SaveSearchButton, (int)Keys.Enter);
                }

                this.SaveSearchPanel.Visible = Engage.Utility.IsLoggedIn && this.IsPostBack;
                this.BackButton.Visible = Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) != this.TabId;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="SaveSearchButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SaveSearchButton_Click(object sender, EventArgs e)
        {
            if (this.Page.IsValid)
            {
                this.GetQueryObject().Save(this.UserId);
            }
        }

        /// <summary>
        /// Handles the <see cref="System.Web.UI.WebControls.Button.Click"/> event of the <see cref="SearchButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = this.GetQueryObject().Execute();

                if (dt.Rows.Count > 0)
                {
                    this.SearchResultsRepeater.DataSource = dt;
                    this.SearchResultsRepeater.DataBind();
                }
                else
                {
                    this.NoResultsLabel.Visible = true;
                }

                this.SaveSearchPanel.Visible = Engage.Utility.IsLoggedIn;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}