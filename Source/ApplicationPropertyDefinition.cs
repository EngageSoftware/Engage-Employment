//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Data;
using System.Globalization;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    public class ApplicationPropertyDefinition
    {
        public static readonly ApplicationPropertyDefinition Lead = new ApplicationPropertyDefinition("Lead");

        private int? _id;// = null;
        private int _dataType;
        private string _defaultValue;
		private string _name;
        private bool _required;
        private int _viewOrder;
        private bool _visible;

        private ApplicationPropertyDefinition(string name)
		{
            this._name = name;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetId()
        {
            InitializeObject();
            return _id ?? -1;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetDataType()
        {
            InitializeObject();
            return _dataType;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public string GetDefaultValue()
        {
            InitializeObject();
            return _defaultValue;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public string GetName()
        {
            InitializeObject();
            return _name;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public bool GetRequired()
        {
            InitializeObject();
            return _required;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetViewOrder()
        {
            InitializeObject();
            return _viewOrder;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public bool GetVisible()
        {
            InitializeObject();
            return _visible;
        }

        private void InitializeObject()
        {
            if (!_id.HasValue)
            {
                using (IDataReader dr = DataProvider.Instance().GetApplicationProperty(this._name, null))
                {
                    if (dr.Read())
                    {
                        _id = Convert.ToInt32(dr["ApplicationPropertyId"], CultureInfo.InvariantCulture);
                        _dataType = Convert.ToInt32(dr["DataType"], CultureInfo.InvariantCulture);
                        _defaultValue = Convert.ToString(dr["DefaultValue"], CultureInfo.InvariantCulture);
                        _name = Convert.ToString(dr["PropertyName"], CultureInfo.InvariantCulture);
                        _required = Convert.ToBoolean(dr["Required"], CultureInfo.InvariantCulture);
                        _viewOrder = Convert.ToInt32(dr["ViewOrder"], CultureInfo.InvariantCulture);
                        _visible = Convert.ToBoolean(dr["Visible"], CultureInfo.InvariantCulture);
                    }
                }
            }
        }
	}
}

