// <copyright file="CategoryListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    public partial class CategoryListing : ModuleBase
    {
        private const int CategoryMaxLength = 255;

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(CategoryMaxLength); }
        }

        protected string MaxLengthValidationText
        {
            get { return string.Format(CultureInfo.CurrentCulture, this.Localize("CategoryMaxLength"), CategoryMaxLength); }
        }

        protected override void OnInit(EventArgs e)
        {
            this.Load += this.Page_Load;
            this.AddButton.Click += this.AddButton_Click;
            this.BackButton.Click += this.BackButton_Click;
            this.CancelNewButton.Click += this.CancelNewButton_Click;
            this.SaveNewButton.Click += this.SaveNewButton_Click;
            this.CategoriesGridView.RowCancelingEdit += this.CategoriesGridView_RowCancelingEdit;
            this.CategoriesGridView.RowCommand += this.CategoriesGridView_RowCommand;
            this.CategoriesGridView.RowDataBound += this.CategoriesGridView_RowDataBound;
            this.CategoriesGridView.RowDeleting += this.CategoriesGridView_RowDeleting;
            this.CategoriesGridView.RowEditing += this.CategoriesGridView_RowEditing;

            base.OnInit(e);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Letting execeptions go blows up the whole page, instead of just the module")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Localization.LocalizeGridView(ref this.CategoriesGridView, this.LocalResourceFile);
                    this.SetupLengthValidation();
                    this.LoadCategories();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void BackButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl(ControlKey.Edit.ToString()));
        }

        private static int? GetCategoryId(Control row)
        {
            var hdnCategoryId = (HiddenField)row.FindControl("hdnCategoryId");

            int categoryId;
            if (hdnCategoryId != null && int.TryParse(hdnCategoryId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out categoryId))
            {
                return categoryId;
            }

            return null;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            this.PanelNew.Visible = true;
            this.txtNewCategoryName.Focus();
        }

        private void SaveNewButton_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (this.IsCategoryNameUnique(null, this.txtNewCategoryName.Text))
                {
                    Category.InsertCategory(this.txtNewCategoryName.Text, this.PortalId);
                    this.HideAndClearNewCategoryPanel();
                    this.LoadCategories();
                }
                else
                {
                    this.cvDuplicateCategory.IsValid = false;
                }
            }
        }

        private void CancelNewButton_Click(object sender, EventArgs e)
        {
            this.HideAndClearNewCategoryPanel();
        }

        private void CategoriesGridView_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            this.CategoriesGridView.EditIndex = -1;
            this.LoadCategories();
        }

        private void CategoriesGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var row = e.Row;
                if (row != null)
                {
                    var btnDelete = (Button)row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        int? categoryId = GetCategoryId(row);
                        if (categoryId.HasValue && Category.IsCategoryUsed(categoryId.Value))
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(CultureInfo.CurrentCulture, "return confirm('{0}');", ClientAPI.GetSafeJSString(this.Localize("DeleteConfirm")));
                        }
                    }
                }
            }
        }

        private void CategoriesGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var categoryId = GetCategoryId(e.RowIndex);
            if (categoryId.HasValue)
            {
                Category.DeleteCategory(categoryId.Value);
                this.LoadCategories();
            }
        }

        private void CategoriesGridView_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.CategoriesGridView.EditIndex = e.NewEditIndex;
            this.HideAndClearNewCategoryPanel();
            this.LoadCategories();
        }

        private void CategoriesGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (string.Equals("Save", e.CommandName, StringComparison.OrdinalIgnoreCase))
            {
                if (Page.IsValid)
                {
                    int rowIndex;
                    if (int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out rowIndex))
                    {
                        int? categoryId = GetCategoryId(rowIndex);
                        if (categoryId.HasValue)
                        {
                            var newCategoryName = this.GetCategoryName(rowIndex);
                            if (this.IsCategoryNameUnique(categoryId, newCategoryName))
                            {
                                Category.UpdateCategory(categoryId.Value, newCategoryName);
                                this.CategoriesGridView.EditIndex = -1;
                                this.LoadCategories();
                            }
                            else
                            {
                                this.cvDuplicateCategory.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        private void LoadCategories()
        {
            List<Category> categories = Category.LoadCategories(null, PortalId);
            this.CategoriesGridView.DataSource = categories;
            this.CategoriesGridView.DataBind();

            if (categories == null || categories.Count % 2 == 0)
            {
                this.PanelNew.CssClass = this.CategoriesGridView.RowStyle.CssClass;
            }
            else
            {
                this.PanelNew.CssClass = this.CategoriesGridView.AlternatingRowStyle.CssClass;
            }

            this.rowNewHeader.Visible = categories == null || categories.Count < 1;
        }

        private bool IsCategoryNameUnique(int? categoryId, string newCategoryName)
        {
            int? newCategoryId = Category.GetCategoryId(newCategoryName, PortalId);
            return !newCategoryId.HasValue || (categoryId.HasValue && newCategoryId.Value == categoryId.Value);
        }

        private void SetupLengthValidation()
        {
            this.regexNewCategoryName.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewCategoryName.ErrorMessage = this.MaxLengthValidationText;
        }

        private void HideAndClearNewCategoryPanel()
        {
            this.PanelNew.Visible = false;
            this.txtNewCategoryName.Text = string.Empty;
        }

        private string GetCategoryName(int rowIndex)
        {
            if (this.CategoriesGridView != null && this.CategoriesGridView.Rows.Count > rowIndex)
            {
                GridViewRow row = this.CategoriesGridView.Rows[rowIndex];
                var txtCategoryName = row.FindControl("txtCategoryName") as TextBox;
                Debug.Assert(txtCategoryName != null, "txtCategoryName not found in row");
                return txtCategoryName.Text;
            }

            return null;
        }

        private int? GetCategoryId(int rowIndex)
        {
            if (this.CategoriesGridView != null && this.CategoriesGridView.Rows.Count > rowIndex)
            {
                return GetCategoryId(this.CategoriesGridView.Rows[rowIndex]);
            }

            return null;
        }
    }
 }