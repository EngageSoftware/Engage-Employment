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

using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    internal class Document
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _documentId;
        public int DocumentId
        {
            [DebuggerStepThrough]
            get { return _documentId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _documentTypeId;
        public int DocumentTypeId
        {
            [DebuggerStepThrough]
            get { return _documentTypeId; }
        }

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private DocumentType _documentType;
        //public DocumentType DocumentType
        //{
        //    get
        //    {
        //        if (_documentType == null)
        //        {
        //            _documentType = Employment.DocumentType.GetDocumentType(_documentTypeId);
        //        }
        //        return _documentType;
        //    }
        //}

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _fileName;
        public string FileName
        {
            [DebuggerStepThrough]
            get { return _fileName; }
        }

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private int _contentLength;
        //public int ContentLength
        //{
        //    [DebuggerStepThrough]
        //    get { return _contentLength; }
        //}

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private string _contentType;
        //public string ContentType
        //{
        //    [DebuggerStepThrough]
        //    get { return _contentType; }
        //}

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private byte[] _data;
        //public byte[] Data
        //{
        //    [DebuggerStepThrough]
        //    get { return (byte[])_data.Clone(); }
        //}

        private static Document FillDocument(DataRow dr)
        {
            Document document = new Document();

            document._documentId = (int)dr["DocumentId"];
            document._documentTypeId = (int)dr["DocumentTypeId"];
            document._fileName = (string)dr["FileName"];
            //document._contentType = (string)dr["ContentType"];
            //document._contentLength = (int)dr["ContentLength"];
            //document._data = (byte[])dr["ResumeData"];

            return document;
        }

        public static List<Document> GetDocuments(int applicationId)
        {
            List<Document> documents = new List<Document>();
            DataTable dt = DataProvider.Instance().GetApplicationDocuments(applicationId);
            
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    documents.Add(FillDocument(dt.Rows[i]));
                }
            }
        
            return documents;
        }

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
            using (IDataReader jobGroupsReader = DataProvider.Instance().GetDocumentJobGroups(documentId))
            {
                IList<int> jobGroupIds = new List<int>();
                while (jobGroupsReader.Read())
                {
                    jobGroupIds.Add((int)jobGroupsReader[0]);
                }

                return jobGroupIds;
            }
        }
    }
}