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
using System.Data;
using System.Diagnostics;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    internal class Category
    {
        /// <summary>
        /// Backing field for <see cref="CategoryId"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? categoryId;

        /// <summary>
        /// Backing field for <see cref="CategoryName"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string categoryName;

        /// <summary>
        /// Gets the category ID.
        /// </summary>
        /// <value>The category ID.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from data binding on ApplicationStatusListing.ascx")]
        public int? CategoryId
        {
            [DebuggerStepThrough]
            get { return this.categoryId; }
            //[DebuggerStepThrough]
            //set { categoryId = value; }
        }

        /// <summary>
        /// Gets or sets the name of this category.
        /// </summary>
        /// <value>The name of this category.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from data binding on ApplicationStatusListing.ascx")]
        public string CategoryName
        {
            [DebuggerStepThrough]
            get { return this.categoryName; }
            [DebuggerStepThrough]
            set { this.categoryName = value; }
        }

        ////public Category(string categoryName)
        ////    : this(null, categoryName)
        ////{
        ////}

        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="categoryName">Name of the category.</param>
        private Category(int? categoryId, string categoryName)
        {
            this.categoryId = categoryId;
            this.categoryName = categoryName;
        }

        ////public void Save(int portalId)
        ////{
        ////    if (this.categoryId.HasValue)
        ////    {
        ////        UpdateCategory(this.categoryId.Value, this.categoryName);
        ////    }
        ////    else
        ////    {
        ////        InsertCategory(this.categoryName, portalId);
        ////    }
        ////}

        ////public bool IsUsed()
        ////{
        ////    ValidateCategoryId();
        ////    return IsCategoryUsed(this.categoryId.Value);
        ////}

        ////public void Delete()
        ////{
        ////    ValidateCategoryId();
        ////    DeleteCategory(this.categoryId.Value);
        ////}

        ////private void ValidateCategoryId()
        ////{
        ////    if (!this.categoryId.HasValue)
        ////    {
        ////        throw new InvalidOperationException("This method is only valid for Categories that have been retrieved from the database");
        ////    }
        ////}

        public static List<Category> LoadCategories(int? jobGroupId, int portalId)
        {
            List<Category> categories = new List<Category>();
            using (IDataReader dr = DataProvider.Instance().GetCategories(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    categories.Add(FillCategory(dr));
                }
            }
            return categories;
        }

        ////public static Category LoadCategory(int categoryId)
        ////{
        ////    using (IDataReader dr = DataProvider.Instance().GetCategory(categoryId))
        ////    {
        ////        if (dr.Read())
        ////        {
        ////            return FillCategory(dr);
        ////        }
        ////    }
        ////    return null;
        ////}

        public static void UpdateCategory(int id, string description)
        {
            DataProvider.Instance().UpdateCategory(id, description);
        }

        public static void InsertCategory(string description, int portalId)
        {
            DataProvider.Instance().InsertCategory(description, portalId);
        }

        public static bool IsCategoryUsed(int categoryId)
        {
            return DataProvider.Instance().IsCategoryUsed(categoryId);
        }

        public static int? GetCategoryId(string categoryName, int portalId)
        {
            return DataProvider.Instance().GetCategoryId(categoryName, portalId);
        }

        public static void DeleteCategory(int id)
        {
            DataProvider.Instance().DeleteCategory(id);
        }

        private static Category FillCategory(IDataRecord dr)
        {
            return new Category((int)dr["CategoryId"], dr["CategoryName"].ToString());
        }
    }
}
