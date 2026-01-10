using Microsoft.AspNetCore.Identity;

namespace Labora.Infrastructure
{
    public class LaboraUser : IdentityUser<Guid>
    {
        public virtual ICollection<LaboraUserClaim> Claims { get; set; }
        // Si bien los IdentityUsers pueden tener claims y los claims corresponden a un user, no existe la propiedad de navegacion de manera predeterminada, por eso la agregamos. Esto sirve para hacer lazyloading y queries con .Include(x=>x.Claims)

        public virtual ICollection<LaboraUserLogin> Logins { get; set; }
        public virtual ICollection<LaboraUserToken> Tokens { get; set; }
        public virtual ICollection<LaboraUserRole> UserRoles { get; set; }

    }

    public class LaboraRole : IdentityRole<Guid>
    {
        public virtual ICollection<LaboraUserRole> UserRoles { get; set; }
        public virtual ICollection<LaboraRoleClaim> RoleClaims { get; set; }
    }

    public class LaboraUserRole : IdentityUserRole<Guid>
    {
        public virtual LaboraUser User { get; set; }
        public virtual LaboraRole Role { get; set; }
    }

    public class LaboraUserClaim : IdentityUserClaim<Guid>
    {
        public virtual LaboraUser User { get; set; }
    }

    public class LaboraUserLogin : IdentityUserLogin<Guid>
    {
        public virtual LaboraUser User { get; set; }
    }

    public class LaboraRoleClaim : IdentityRoleClaim<Guid>
    {
        public virtual LaboraRole Role { get; set; }
    }

    public class LaboraUserToken : IdentityUserToken<Guid>
    {
        public virtual LaboraUser User { get; set; }
    }
}
