using FlexNet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlexNet.Infrastructure.Migrations;

public class ApplicationDbContextBuilder : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {




        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("DefaultConnection");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}