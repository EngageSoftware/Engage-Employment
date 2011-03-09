// <copyright file="ApplicationPropertyDefinition.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2011
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Engage.Dnn.Employment.Data;

    public class ApplicationPropertyDefinition
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "It actually is immutable")]
        public static readonly ApplicationPropertyDefinition Lead = new ApplicationPropertyDefinition("Lead");

        private int? id; //// = null;
        private int dataType;
        private string defaultValue;
        private string name;
        private bool required;
        private int viewOrder;
        private bool visible;

        private ApplicationPropertyDefinition(string name)
        {
            this.name = name;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetId()
        {
            this.InitializeObject();
            return this.id ?? -1;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetDataType()
        {
            this.InitializeObject();
            return this.dataType;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public string GetDefaultValue()
        {
            this.InitializeObject();
            return this.defaultValue;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public string GetName()
        {
            this.InitializeObject();
            return this.name;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public bool GetRequired()
        {
            this.InitializeObject();
            return this.required;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetViewOrder()
        {
            this.InitializeObject();
            return this.viewOrder;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public bool GetVisible()
        {
            this.InitializeObject();
            return this.visible;
        }

        private void InitializeObject()
        {
            if (!this.id.HasValue)
            {
                using (var dr = DataProvider.Instance().GetApplicationProperty(this.name, null))
                {
                    if (dr.Read())
                    {
                        this.id = Convert.ToInt32(dr["ApplicationPropertyId"], CultureInfo.InvariantCulture);
                        this.dataType = Convert.ToInt32(dr["DataType"], CultureInfo.InvariantCulture);
                        this.defaultValue = Convert.ToString(dr["DefaultValue"], CultureInfo.InvariantCulture);
                        this.name = Convert.ToString(dr["PropertyName"], CultureInfo.InvariantCulture);
                        this.required = Convert.ToBoolean(dr["Required"], CultureInfo.InvariantCulture);
                        this.viewOrder = Convert.ToInt32(dr["ViewOrder"], CultureInfo.InvariantCulture);
                        this.visible = Convert.ToBoolean(dr["Visible"], CultureInfo.InvariantCulture);
                    }
                }
            }
        }
    }
}

