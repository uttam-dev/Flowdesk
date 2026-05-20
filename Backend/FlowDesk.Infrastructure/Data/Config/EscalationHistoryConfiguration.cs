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
            builder.HasKey(e => e.EscalationHistoryId);

            builder.HasOne(e => e.Request)
                .WithMany(r => r.EscalationHistories)
                .HasForeignKey(e => e.RequestId);

            builder.HasOne(e => e.EscalatedByUser)
                .WithMany(u => u.EscalationRequestHistory)
                .HasForeignKey(e => e.EscalatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
