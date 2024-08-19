using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace pastebin;

public partial class PastebinContext : DbContext
{
    public PastebinContext()
    {
    }

    public PastebinContext(DbContextOptions<PastebinContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;user=root;password=1R2o3m4a?;database=Pastebin", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.35-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Hash).HasName("PRIMARY");

            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.Author)
                .HasMaxLength(255)
                .HasColumnName("author");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
