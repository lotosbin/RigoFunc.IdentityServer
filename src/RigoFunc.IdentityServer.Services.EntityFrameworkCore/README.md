#

# Usage
```
    var connectionString = @"Server=(local);Database=identityserver;Trusted_Connection=True;";
    services.AddIdentityServerEntityFrameworkCoreServices(options => options.UseSqlServer(connectionString));
```