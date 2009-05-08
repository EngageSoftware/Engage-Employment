using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{

    public class DocumentType
    {
        public static readonly DocumentType Resume = new DocumentType("Resume");
        public static readonly DocumentType CoverLetter = new DocumentType("Cover Letter");

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _description = string.Empty;
		private int? _id = null;

        private DocumentType(string description)
		{
			this._description = description;
		}

        public string Description
        {
            [DebuggerStepThrough()]
            get { return this._description; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetId()
		{
			if (!_id.HasValue)
			{
                using (IDataReader dr = DataProvider.Instance().GetDocumentType(this.Description, null))
                {
                    if (dr.Read())
                    {
                        _id = Convert.ToInt32(dr["DocumentTypeID"], CultureInfo.InvariantCulture);
                    }
                }
			}

			return _id ?? -1;
		}

        internal static DocumentType GetDocumentType(int documentTypeId)
        {
            using (IDataReader dr = DataProvider.Instance().GetDocumentType(documentTypeId))
            {
                if (dr.Read())
                {
                    DocumentType documentType = new DocumentType((string)dr["Description"]);
                    documentType._id = documentTypeId;
                    return documentType;
                }
            }
            return null;
        }
    }
}

