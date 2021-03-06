// <copyright file="DocumentType.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Engage.Dnn.Employment.Data;

    public class DocumentType
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable.")]
        public static readonly DocumentType Resume = new DocumentType("Resume");
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It's immutable.")]
        public static readonly DocumentType CoverLetter = new DocumentType("Cover Letter");

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string description = string.Empty;

        private int? id = null;

        private DocumentType(string description)
        {
            this.description = description;
        }

        public string Description
        {
            [DebuggerStepThrough]
            get { return this.description; }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetId()
        {
            if (!this.id.HasValue)
            {
                using (var dr = DataProvider.Instance().GetDocumentType(this.Description, null))
                {
                    if (dr.Read())
                    {
                        this.id = Convert.ToInt32(dr["DocumentTypeID"], CultureInfo.InvariantCulture);
                    }
                }
            }

            return this.id ?? -1;
        }

        internal static DocumentType GetDocumentType(int documentTypeId)
        {
            using (var dr = DataProvider.Instance().GetDocumentType(documentTypeId))
            {
                if (dr.Read())
                {
                    return new DocumentType((string)dr["Description"])
                        {
                            id = documentTypeId
                        };
                }
            }

            return null;
        }
    }
}