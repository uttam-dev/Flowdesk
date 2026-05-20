using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class EscalationHistoryConfiguration : IEntityTypeConfiguration<EscalationHistory>
    {
        public void Configure(EntityTypeBuilder<EscalationHistory> builder)
        {
            builder.HasKey(e => e.EscalationId);

            builder.Property(e => e.EscalationReason)
                .IsRequired();

            builder.Property(e => e.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            builder.HasOne(e => e.Request)
                .WithMany(r => r.EscalationHistory)
                .HasForeignKey(e => e.RequestId);

            builder.HasOne(e => e.EscalatedByUser)
                .WithMany(u => u.Escalations)
                .HasForeignKey(e => e.EscalatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
