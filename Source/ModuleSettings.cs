// <copyright file="ModuleSettings.cs" company="Engage Software">
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
    using System.Diagnostics.CodeAnalysis;

    using Engage.Dnn.Framework;

    /// <summary>
    /// All of the settings for Engage: Employment
    /// </summary>
    public static class ModuleSettings
    {
        #region Global Settings

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable."),
        SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is rediculous.")]
        public static readonly Setting<int?> JobGroupId = new Setting<int?>("JobGroupId", SettingScope.TabModule, null);
        
        #endregion

        #region JobListingOptions

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobListingShowOnlyHotJobs = new Setting<bool>("ShowOnlyHotJobs", SettingScope.TabModule, false);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable."),
        SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is rediculous.")]
        public static readonly Setting<int?> JobListingMaximumNumberOfJobsDisplayed = new Setting<int?>("MaximumNumberOfJobsDisplayed", SettingScope.TabModule, 5);
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobListingLimitJobsRandomly = new Setting<bool>("LimitJobsRandomly", SettingScope.TabModule, false);
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobListingShowCloseDate = new Setting<bool>("ShowCloseDate", SettingScope.TabModule, false);

        #endregion

        #region JobDetailOptions

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobDetailEnableDnnSearch = new Setting<bool>("EnableDnnSearch", SettingScope.TabModule, true);
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<string> JobDetailApplicationEmailAddresses = new Setting<string>("ApplicationEmailAddress", SettingScope.TabModule, null);
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<string> JobDetailFromEmailAddress = new Setting<string>("FriendEmailAddress", SettingScope.TabModule, null);
        
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobDetailRequireRegistration = new Setting<bool>("RequireRegistration", SettingScope.TabModule, true);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayName = new Setting<Visibility>("DisplayName", SettingScope.TabModule, Visibility.Hidden);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayEmail = new Setting<Visibility>("DisplayEmail", SettingScope.TabModule, Visibility.Hidden);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayPhone = new Setting<Visibility>("DisplayPhone", SettingScope.TabModule, Visibility.Hidden);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayMessage = new Setting<Visibility>("DisplayMessage", SettingScope.TabModule, Visibility.Optional);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplaySalaryRequirement = new Setting<Visibility>("DisplaySalaryRequirement", SettingScope.TabModule, Visibility.Optional);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayCoverLetter = new Setting<Visibility>("DisplayCoverLetter", SettingScope.TabModule, Visibility.Hidden);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayResume = new Setting<Visibility>("DisplayResume", SettingScope.TabModule, Visibility.Required);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<Visibility> JobDetailDisplayLead = new Setting<Visibility>("DisplayLead", SettingScope.TabModule, Visibility.Hidden);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Reference type is immutable.")]
        public static readonly Setting<bool> JobDetailShowCloseDate = new Setting<bool>("ShowCloseDate", SettingScope.TabModule, false);

        #endregion 
    }
}