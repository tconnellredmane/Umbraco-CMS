using System.Collections.Generic;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// The user manager for members
    /// </summary>
    public interface IMemberManager : IUmbracoUserManager<MemberIdentityUser>
    {
        /// <summary>
        /// Checks if the current member is authorized based on the parameters provided.
        /// </summary>
        /// <param name="allowTypes">Allowed types.</param>
        /// <param name="allowGroups">Allowed groups.</param>
        /// <param name="allowMembers">Allowed individual members.</param>
        /// <returns>True or false if the currently logged in member is authorized</returns>
        bool IsMemberAuthorized(
            IEnumerable<string> allowTypes = null,
            IEnumerable<string> allowGroups = null,
            IEnumerable<int> allowMembers = null);
    }
}
