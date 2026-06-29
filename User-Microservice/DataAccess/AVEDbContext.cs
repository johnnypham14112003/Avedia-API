using System;
using System.Collections.Generic;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public partial class AVEDbContext : DbContext
{
    public AVEDbContext()
    {
    }

    public AVEDbContext(DbContextOptions<AVEDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountBadge> AccountBadges { get; set; }

    public virtual DbSet<AccountNotification> AccountNotifications { get; set; }

    public virtual DbSet<Badge> Badges { get; set; }

    public virtual DbSet<Contribution> Contributions { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pg_trgm")
            .HasAnnotation("Npgsql:CollationDefinition:public.case_insensitive", "und-u-ks-level1,und-u-ks-level1,icu,False");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.HasIndex(e => e.UserName, "accounts_user_name_key").IsUnique();

            entity.HasIndex(e => e.Email, "idx_accounts_email_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.Nationality, "idx_accounts_nationality").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.Role, "idx_accounts_role").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.Status, "idx_accounts_status").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.UserName, "idx_accounts_username_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .UseCollation("case_insensitive")
                .HasColumnName("email");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("joined_date");
            entity.Property(e => e.MeritPoint).HasColumnName("merit_point");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
                .UseCollation("case_insensitive")
                .HasColumnName("nationality");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(20)
                .HasColumnName("otp_code");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.RefreshToken).HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenExpirytime)
                .HasColumnType("timestamp(0) without time zone")
                .HasColumnName("refresh_token_expirytime");
            entity.Property(e => e.Role)
                .HasMaxLength(30)
                .HasDefaultValueSql("'User'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .UseCollation("case_insensitive")
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<AccountBadge>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.BadgeId }).HasName("account_badges_pkey");

            entity.ToTable("account_badges");

            entity.HasIndex(e => e.AccountId, "idx_account_badges_account_id");

            entity.HasIndex(e => e.BadgeId, "idx_account_badges_badge_id");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.AwardedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("awarded_date");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountBadges)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("account_badges_account_id_fkey");

            entity.HasOne(d => d.Badge).WithMany(p => p.AccountBadges)
                .HasForeignKey(d => d.BadgeId)
                .HasConstraintName("account_badges_badge_id_fkey");
        });

        modelBuilder.Entity<AccountNotification>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.NotificationId }).HasName("account_notifications_pkey");

            entity.ToTable("account_notifications");

            entity.HasIndex(e => e.AccountId, "idx_account_notif_account_id");

            entity.HasIndex(e => e.NotificationId, "idx_account_notif_notif_id");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsRead).HasColumnName("is_read");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("account_notifications_account_id_fkey");

            entity.HasOne(d => d.Notification).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.NotificationId)
                .HasConstraintName("account_notifications_notification_id_fkey");
        });

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("badges_pkey");

            entity.ToTable("badges");

            entity.HasIndex(e => e.Description, "idx_badges_description_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.RareLevel, "idx_badges_rare_level");

            entity.HasIndex(e => e.Status, "idx_badges_status").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.Title, "idx_badges_title_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .UseCollation("case_insensitive")
                .HasColumnName("description");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.LocalePath).HasColumnName("locale_path");
            entity.Property(e => e.RareLevel)
                .HasDefaultValue((short)1)
                .HasColumnName("rare_level");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .UseCollation("case_insensitive")
                .HasColumnName("title");
        });

        modelBuilder.Entity<Contribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contributions_pkey");

            entity.ToTable("contributions");

            entity.HasIndex(e => e.ActionType, "idx_contributions_action_type_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.ContributorId, "idx_contributions_contributor_id");

            entity.HasIndex(e => e.Status, "idx_contributions_status").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.TargetId, "idx_contributions_target_id");

            entity.HasIndex(e => e.TargetType, "idx_contributions_target_type_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(10)
                .UseCollation("case_insensitive")
                .HasColumnName("action_type");
            entity.Property(e => e.AdminApproved).HasColumnName("admin_approved");
            entity.Property(e => e.ApproverId).HasColumnName("approver_id");
            entity.Property(e => e.ContributorId).HasColumnName("contributor_id");
            entity.Property(e => e.HandledDate).HasColumnName("handled_date");
            entity.Property(e => e.ProposedData)
                .HasColumnType("jsonb")
                .HasColumnName("proposed_data");
            entity.Property(e => e.RequestedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("requested_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
                .UseCollation("case_insensitive")
                .HasColumnName("target_type");

            entity.HasOne(d => d.Approver).WithMany(p => p.ContributionApprovers)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("contributions_approver_id_fkey");

            entity.HasOne(d => d.Contributor).WithMany(p => p.ContributionContributors)
                .HasForeignKey(d => d.ContributorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contributions_contributor_id_fkey");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("favorites_pkey");

            entity.ToTable("favorites");

            entity.HasIndex(e => e.AccountId, "idx_favorites_account_id");

            entity.HasIndex(e => e.TargetId, "idx_favorites_target_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
                .UseCollation("case_insensitive")
                .HasColumnName("target_type");

            entity.HasOne(d => d.Account).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("favorites_account_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.Title, "idx_notifications_title_trgm")
                .HasMethod("gin")
                .HasOperators(new[] { "gin_trgm_ops" })
                .UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("created_date");
            entity.Property(e => e.IsGlobal).HasColumnName("is_global");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .UseCollation("case_insensitive")
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .HasColumnName("type");
            entity.Property(e => e.TypeId).HasColumnName("type_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
