using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Data.Config
{


    public class RemoteSessionConfiguration : IEntityTypeConfiguration<RemoteSession>
    {
        public void Configure(EntityTypeBuilder<RemoteSession> builder)
        {
            builder.ToTable("RemoteSessions");

            builder.HasKey(x => x.RemoteSessionId);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ResolutionNotes)
                .HasMaxLength(2000);

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .IsRequired();

            // ── Relationships ────────────────────────────────────────────────

            // Many sessions can belong to one request (history of remote attempts)
            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Support user who initiated
            builder.HasOne(x => x.InitiatedBy)
                .WithMany()
                .HasForeignKey(x => x.InitiatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee / manager whose machine is being accessed
            builder.HasOne(x => x.TargetUser)
                .WithMany()
                .HasForeignKey(x => x.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Indexes ──────────────────────────────────────────────────────

            builder.HasIndex(x => x.RequestId)
                .HasDatabaseName("IX_RemoteSessions_RequestId");

            builder.HasIndex(x => x.TargetUserId)
                .HasDatabaseName("IX_RemoteSessions_TargetUserId");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_RemoteSessions_Status");
        }
    }

};