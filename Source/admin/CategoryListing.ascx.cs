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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;

namespace Engage.Dnn.Employment.Admin
{
    partial class CategoryListing : ModuleBase
    {
        private const int CategoryMaxLength = 255;

        protected string MaxLengthValidationText
        {
            get { return String.Format(CultureInfo.CurrentCulture, Localization.GetString("CategoryMaxLength", LocalResourceFile), CategoryMaxLength); }
        }

        protected static string MaxLengthValidationExpression
        {
            get { return Utility.GetMaxLengthValidationExpression(CategoryMaxLength); }
        }

        protected override void OnInit(EventArgs e)
        {
            this.Load += Page_Load;
            btnAdd.Click += btnAdd_Click;
            btnBack.Click += btnBack_Click;
            btnCancelNew.Click += btnCancelNew_Click;
            btnSaveNew.Click += btnSaveNew_Click;
            gvCategories.RowCancelingEdit += gvCategories_RowCancelingEdit;
            gvCategories.RowCommand += gvCategories_RowCommand;
            gvCategories.RowDataBound += gvCategories_RowDataBound;
            gvCategories.RowDeleting += gvCategories_RowDeleting;
            gvCategories.RowEditing += gvCategories_RowEditing;

            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(Object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Engage.Dnn.Utility.LocalizeGridView(ref gvCategories, LocalResourceFile);
                    SetupLengthValidation();
                    LoadCategories();
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
            txtNewCategoryName.Focus();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnSaveNew_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                if (IsCategoryNameUnique(null, txtNewCategoryName.Text))
                {
                    Category.InsertCategory(txtNewCategoryName.Text, PortalId);
                    HideAndClearNewCategoryPanel();
                    LoadCategories();
                }
                else
                {
                    cvDuplicateCategory.IsValid = false;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void btnCancelNew_Click(object sender, EventArgs e)
        {
            HideAndClearNewCategoryPanel();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvCategories_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvCategories.EditIndex = -1;
            LoadCategories();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvCategories_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                GridViewRow row = e.Row;
                if (row != null)
                {
                    Button btnDelete = (Button)row.FindControl("btnDelete");
                    if (btnDelete != null)
                    {
                        int? categoryId = GetCategoryId(row);
                        if (categoryId.HasValue && Category.IsCategoryUsed(categoryId.Value))
                        {
                            btnDelete.Enabled = false;
                        }
                        else
                        {
                            btnDelete.OnClientClick = string.Format(CultureInfo.CurrentCulture, "return confirm('{0}');", ClientAPI.GetSafeJSString(Localization.GetString("DeleteConfirm", LocalResourceFile)));
                        }
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvCategories_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int? categoryId = GetCategoryId(e.RowIndex);
            if (categoryId.HasValue)
            {
                Category.DeleteCategory(categoryId.Value);
                LoadCategories();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvCategories_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvCategories.EditIndex = e.NewEditIndex;
            HideAndClearNewCategoryPanel();
            LoadCategories();
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void gvCategories_RowCommand(object sender, GridViewCommandEventArgs e)
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
                            string newCategoryName = GetCategoryName(rowIndex);
                            if (IsCategoryNameUnique(categoryId, newCategoryName))
                            {
                                Category.UpdateCategory(categoryId.Value, newCategoryName);
                                gvCategories.EditIndex = -1;
                                LoadCategories();
                            }
                            else
                            {
                                cvDuplicateCategory.IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        private void LoadCategories()
        {
            List<Category> categories = Category.LoadCategories(null, PortalId);
            this.gvCategories.DataSource = categories;
            this.gvCategories.DataBind();

            if (categories == null || categories.Count % 2 == 0)
            {
                pnlNew.CssClass = gvCategories.RowStyle.CssClass;
            }
            else
            {
                pnlNew.CssClass = gvCategories.AlternatingRowStyle.CssClass;
            }

            rowNewHeader.Visible = (categories == null || categories.Count < 1);
        }

        private bool IsCategoryNameUnique(int? categoryId, string newCategoryName)
        {
            int? newCategoryId = Category.GetCategoryId(newCategoryName, PortalId);
            return !newCategoryId.HasValue || (categoryId.HasValue && newCategoryId.Value == categoryId.Value);
        }

        private void SetupLengthValidation()
        {
            this.regexNewCategoryName.ValidationExpression = MaxLengthValidationExpression;
            this.regexNewCategoryName.ErrorMessage = MaxLengthValidationText;
        }

        private void HideAndClearNewCategoryPanel()
        {
            this.pnlNew.Visible = false;
            this.txtNewCategoryName.Text = string.Empty;
        }

        private string GetCategoryName(int rowIndex)
        {
            if (gvCategories != null && gvCategories.Rows.Count > rowIndex)
            {
                GridViewRow row = gvCategories.Rows[rowIndex];
                TextBox txtCategoryName = row.FindControl("txtCategoryName") as TextBox;
                Debug.Assert(txtCategoryName != null);
                return txtCategoryName.Text;
            }
            return null;
        }

        private static int? GetCategoryId(Control row)
        {
            HiddenField hdnCategoryId = (HiddenField)row.FindControl("hdnCategoryId");

            int categoryId;
            if (hdnCategoryId != null && int.TryParse(hdnCategoryId.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out categoryId))
            {
                return categoryId;
            }
            return null;
        }

        private int? GetCategoryId(int rowIndex)
        {
            if (gvCategories != null && gvCategories.Rows.Count > rowIndex)
            {
                return GetCategoryId(gvCategories.Rows[rowIndex]);
            }
            return null;
        }
    }
 }