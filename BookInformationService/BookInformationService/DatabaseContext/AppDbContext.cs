using BookInformationService.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;


/* Ensure Migration and Database Initialization:

Add the package:
"
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Cosmos" Version="8.0.7" />
"

Open Package Manager Console:
Navigate to Tools > NuGet Package Manager > Package Manager Console in Visual Studio.

Go to the .csproj folder.

Execute the following command:
    Add-Migration InitialCreate
    Update-Database

*/

namespace BookInformationService.DatabaseContext;

public class AppDbContext : DbContext
{
    public DbSet<BookInformation> BookInformations { get; set; } // EF will create a table by the name "BookInformations" 

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Specify container name if needed
        modelBuilder.Entity<BookInformation>()
            .ToContainer("BookInformations") // Ensure this matches your container name
            .HasPartitionKey(b => b.Id); // Specify the partition key property
    }
}


