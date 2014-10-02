// <copyright file="Category.cs" company="Engage Software">
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
    using System.Collections.Generic;
    using System.Data;

    using Engage.Annotations;
    using Engage.Dnn.Employment.Data;

    /// <summary>A job opening's category</summary>
    internal class Category
    {
        /// <summary>Initializes a new instance of the <see cref="Category"/> class.</summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="categoryName">Name of the category.</param>
        private Category(int? categoryId, string categoryName)
        {
            this.CategoryId = categoryId;
            this.CategoryName = categoryName;
        }

        /// <summary>Gets the category ID.</summary>
        /// <value>The category ID.</value>
        public int? CategoryId { get; private set; }

        /// <summary>Gets or sets the name of this category.</summary>
        /// <value>The name of this category.</value>
        public string CategoryName { get; set; }

        /// <summary>Loads the categories.</summary>
        /// <param name="jobGroupId">The ID of the job group, or <c>null</c>.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A <see cref="List{TCategory}" /> of <see cref="Category"/> instances.</returns>
        [NotNull]
        public static List<Category> LoadCategories(int? jobGroupId, int portalId)
        {
            var categories = new List<Category>();
            using (var dr = DataProvider.Instance().GetCategories(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    categories.Add(FillCategory(dr));
                }
            }

            return categories;
        }

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