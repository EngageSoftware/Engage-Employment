// <copyright file="Document.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Engage.Dnn.Employment.Data;

    internal class Document
    {
        public int DocumentId { get; private set; }

        public int DocumentTypeId { get; private set; }

        public string FileName { get; private set; }

        /// <summary>
        /// Gets a list of IDs of the job groups that can view the document with the given ID.
        /// </summary>
        /// <remarks>
        /// Note that the user who submitted this document should still be able to view it regardless of job group permissions
        /// </remarks>
        /// <param name="documentId">The document ID.</param>
        /// <returns>A list of the IDs of the job groups that can view the document with the given ID</returns>
        public static IList<int> GetDocumentJobGroups(int documentId)
        {
            using (var jobGroupsReader = DataProvider.Instance().GetDocumentJobGroups(documentId))
            {
                var jobGroupIds = new List<int>();
                while (jobGroupsReader.Read())
                {
                    jobGroupIds.Add((int)jobGroupsReader[0]);
                }

                return jobGroupIds;
            }
        }

        public static List<Document> GetDocuments(int applicationId)
        {
            var dt = DataProvider.Instance().GetApplicationDocuments(applicationId);
            return FillDocuments(dt.Rows.Cast<DataRow>());
        }

        public static List<Document> FillDocuments(IEnumerable<DataRow> documentRows)
        {
            return documentRows.Select(row => FillDocument(row)).ToList();
        }

        private static Document FillDocument(DataRow dr)
        {
            return new Document
                {
                    DocumentId = (int)dr["DocumentId"],
                    DocumentTypeId = (int)dr["DocumentTypeId"],
                    FileName = (string)dr["FileName"]
                };
        }
    }
}