using Microsoft.AspNetCore.Authorization;


namespace Professionals.Infrastructure
{
    public class MinimumAgeAuthorizeAttribute(int age) : AuthorizeAttribute,
      IAuthorizationRequirement, IAuthorizationRequirementData
    {
        public int Age { get; set; } = age;

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return this;
        }
    }
}
