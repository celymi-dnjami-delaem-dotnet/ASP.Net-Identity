using IdentityWebApi.DAL.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityWebApi.DAL.Configuration;

public class AppRoleConfiguration : IEntityTypeConfiguration<AppRole>
{
    public void Configure(EntityTypeBuilder<AppRole> builder)
    {
        builder.Property(x => x.CreationDate)
            .HasDefaultValueSql("getdate()");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
