// <copyright file="DatePickerOptionsConverter.cs" company="Engage Software">
// Engage: Rotator - http://www.engagemodules.com
// Copyright (c) 2004-2009
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
    using System.Collections.ObjectModel;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Implementation of <see cref="JavaScriptConverter"/> for <see cref="DatePickerOptions"/>
    /// </summary>
    public class DatePickerOptionsConverter : JavaScriptConverter
    {
        /// <summary>
        /// Gets a collection of the supported types
        /// </summary>
        /// <value>An object that implements <see cref="IEnumerable{T}"/> that represents the types supported by the converter. </value>
        public override IEnumerable<Type> SupportedTypes
        {
            get
            {
                return new ReadOnlyCollection<Type>(new Type[] { typeof(DatePickerOptions) });
            }
        }

        /// <summary>
        /// Converts the provided dictionary into an object of the specified type. 
        /// </summary>
        /// <param name="dictionary">An <see cref="IDictionary{TKey,TValue}"/> instance of property data stored as name/value pairs. </param>
        /// <param name="type">The type of the resulting object.</param>
        /// <param name="serializer">The <see cref="JavaScriptSerializer"/> instance. </param>
        /// <returns>The deserialized object. </returns>
        /// <exception cref="InvalidOperationException">We only serialize <see cref="CycleOptions"/></exception>
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new InvalidOperationException("We only serialize DatePickerOptions");
        }

        /// <summary>
        /// Builds a dictionary of name/value pairs
        /// </summary>
        /// <param name="obj">The object to serialize. </param>
        /// <param name="serializer">The object that is responsible for the serialization. </param>
        /// <returns>An object that contains key/value pairs that represent the object’s data. </returns>
        /// <exception cref="InvalidOperationException"><paramref name="obj"/> must be of the <see cref="CycleOptions"/> type</exception>
        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            DatePickerOptions opts = obj as DatePickerOptions;
            if (opts == null)
            {
                throw new InvalidOperationException("object must be of the DatePickerOptions type");
            }

            IDictionary<string, object> datePickerOptions = new Dictionary<string, object>(24);
            datePickerOptions.Add("clearText", opts.ClearText);
            datePickerOptions.Add("clearStatus", opts.ClearStatus);
            datePickerOptions.Add("closeText", opts.CloseText);
            datePickerOptions.Add("closeStatus", opts.CloseStatus);
            datePickerOptions.Add("prevText", opts.PreviousText);
            datePickerOptions.Add("prevStatus", opts.PreviousStatus);
            datePickerOptions.Add("nextText", opts.NextText);
            datePickerOptions.Add("nextStatus", opts.NextStatus);
            datePickerOptions.Add("currentText", opts.CurrentText);
            datePickerOptions.Add("currentStatus", opts.CurrentStatus);
            datePickerOptions.Add("monthNames", opts.GetMonthNames());
            datePickerOptions.Add("monthNamesShort", opts.GetMonthNamesShort());
            datePickerOptions.Add("monthStatus", opts.MonthStatus);
            datePickerOptions.Add("yearStatus", opts.YearStatus);
            datePickerOptions.Add("weekHeader", opts.WeekHeader);
            datePickerOptions.Add("weekStatus", opts.WeekStatus);
            datePickerOptions.Add("dayNames", opts.GetDayNames());
            datePickerOptions.Add("dayNamesShort", opts.GetDayNamesShort());
            datePickerOptions.Add("dayNamesMin", opts.GetDayNamesMin());
            datePickerOptions.Add("dayStatus", opts.DayStatus);
            datePickerOptions.Add("dateStatus", opts.DateStatus);
            datePickerOptions.Add("dateFormat", opts.DateFormat);
            datePickerOptions.Add("firstDay", opts.FirstDay);
            datePickerOptions.Add("isRTL", opts.IsRightToLeft);

            return datePickerOptions;
        }
    }
}