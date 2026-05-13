using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class RequestHistoryConfiguration : IEntityTypeConfiguration<RequestHistory>
    {
        public void Configure(EntityTypeBuilder<RequestHistory> builder)
        {
            builder.HasKey(x => x.RequestHistoryId);

            builder.Property(x => x.OldStatus)
                .IsRequired();

            builder.Property(x => x.NewStatus)
                .IsRequired();

            builder.Property(x => x.ChangedOn)
                .IsRequired();

            builder.Property(x => x.Remarks)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.ChangedById)
                .IsRequired();

            builder.Property(x => x.RequestId)
                .IsRequired();

            builder.Property(x => x.OldStatus)
                .HasConversion<int>();

            builder.Property(x => x.NewStatus)
                .HasConversion<int>();

            builder.HasOne(h => h.Request)
                .WithMany(r => r.Histories)
                .HasForeignKey(h => h.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.ChangedByUser)
                .WithMany(u => u.RequestHistories)
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
