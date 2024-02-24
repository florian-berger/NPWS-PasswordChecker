using System;

namespace PasswordChecker.UI.BindingObjects
{
    /// <summary>
    ///     Binding object for third party licenses
    /// </summary>
    public class ThirdPartyLicenseBinding
    {
        #region Properties

        /// <summary>
        ///     Name of the licensed object
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Author of the licensed object
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        ///     Version of the licensed object
        /// </summary>
        public Version? Version { get; set; }

        /// <summary>
        ///     License type of the object
        /// </summary>
        public string? LicenseType { get; set; }

        /// <summary>
        ///     License text
        /// </summary>
        public string? LicenseText { get; set; }

        /// <summary>
        ///     Information if the license text represents a URI
        /// </summary>
        public bool IsLicenseTextUri { get; set; }

        /// <summary>
        ///     Uri of the licensed objects homepage
        /// </summary>
        public string? Uri { get; set; }

        #endregion Properties
    }
}
