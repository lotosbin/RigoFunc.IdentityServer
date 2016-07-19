using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Host.EntityFrameworkCore {
    public class AppUser : IdentityUser<int> {
        public AppUser() { }

        public AppUser(string userName) : this() {
            UserName = userName;
        }
    }
}
