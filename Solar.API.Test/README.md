# API Integration Tests

Go to your solution, right click and add a new xUnit test project

Install `Microsoft.AspNetCore.Mvc.Testing`
Install `Microsoft.EntityFrameworkCore.InMemory`
Install `xunit.runner.visualstudio`

Add the following to the bottom of your `Program.cs` to make it public:

```
public partial class Program { }
```

Add your `Solar.API` project as a project reference in `Solar.API.Test`
Add your `Solar.Data` project as a project reference in `Solar.API.Test`
Add your `Solar.DTOs` project as a project reference in `Solar.API.Test`

In your `Program.cs` you need to only migrate the database if we are able to, this allows us to use an in memory database:

```csharp
var db = services.GetRequiredService<SolarDbContext>();
if (db.Database.IsSqlServer())
{
    db.Database.Migrate();
}
```

## References

* https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0
* https://stackoverflow.com/questions/70900451/c-sharp-netcore-webapi-integration-testing-httpclient-uses-https-for-get-reques
