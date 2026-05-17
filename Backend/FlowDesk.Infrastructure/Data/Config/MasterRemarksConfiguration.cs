using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Infrastructure.Data.Config
{
    internal class MasterRemarksConfiguration : IEntityTypeConfiguration<MasterRemarks>
    {
        public void Configure(EntityTypeBuilder<MasterRemarks> builder)
        {
            builder.HasKey(x => x.MasterRemarksId);
            builder.Property(x => x.RemarksText)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(x => x.ActionType)
                .HasConversion<int>()
                .IsRequired();
            builder.Property(x => x.IsActive)
               .IsRequired();
        }
    }
}
