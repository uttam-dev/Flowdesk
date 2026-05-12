using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.RoleId);

            builder.Property(x => x.RoleName)
                .IsRequired()
                .HasMaxLength(100);

            // Unique role names (important)
            builder.HasIndex(x => x.RoleName)
                .IsUnique();

            // Relation handled from User side
        }
    }
}
