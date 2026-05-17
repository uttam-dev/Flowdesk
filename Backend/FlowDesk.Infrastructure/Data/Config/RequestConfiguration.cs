using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(x => x.RequestId);

            builder.Property(x => x.RequestNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique();

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UpdatedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ClosedOn)
                .IsRequired(false);

            builder.Property(x => x.Priority)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.EmployeeId)
                .IsRequired();

            builder.Property(x => x.CategoryId)
                .IsRequired();


            builder.HasOne(r => r.AssignedUser)
                .WithMany(u => u.AssignedRequests)
                .HasForeignKey(r => r.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Employee)
                .WithMany(u => u.CreatedRequests)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Category)
                .WithMany(c => c.Requests)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
