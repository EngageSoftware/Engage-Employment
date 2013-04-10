// <copyright file="Extensions.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2013
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

    /// <summary>
    /// Extension methods to enhance core types
    /// </summary>
    public static class Extensions 
    {
        /// <summary>
        /// Determines whether a sequence contains exactly one element.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/></typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/> to check for singularity.</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="source"/> sequence contains exactly one element; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSingle<T>(this IEnumerable<T> source)
        {
            if (source == null) 
            {
                throw new ArgumentNullException("source");
            }

            using (var enumerator = source.GetEnumerator())
            {
                return enumerator.MoveNext() && !enumerator.MoveNext();
            }
        }
    }
}