using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Infrastructure.Data.Config
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(x => x.CommentId);

            builder.Property(x => x.CommentText)
                .IsRequired();

            builder.Property(x => x.CreatedOn)
                .IsRequired();

            builder.Property(x => x.RequestId)
                .IsRequired();

            builder.Property(x => x.CommentById)
                .IsRequired();

            builder.HasOne(x => x.Request)
                .WithMany()
                .HasForeignKey(x => x.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Commenter)
                .WithMany()
                .HasForeignKey(x => x.CommentById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
