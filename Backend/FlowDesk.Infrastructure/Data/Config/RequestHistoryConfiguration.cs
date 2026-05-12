using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class RequestHistoryConfiguration : IEntityTypeConfiguration<RequestHistory>
    {
        public void Configure(EntityTypeBuilder<RequestHistory> builder)
        {
            builder.HasKey(x => x.HistoryId);

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

            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.OldStatus)
                .HasConversion<int>();

            builder.Property(x => x.NewStatus)
                .HasConversion<int>();
        }
    }
}
