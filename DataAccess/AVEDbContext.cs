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

    public virtual DbSet<Actor> Actors { get; set; }

    public virtual DbSet<ActorImage> ActorImages { get; set; }

    public virtual DbSet<Badge> Badges { get; set; }

    public virtual DbSet<Contribution> Contributions { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Label> Labels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<VideoActor> VideoActors { get; set; }

    public virtual DbSet<VideoImage> VideoImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.Email, "accounts_email_key").IsUnique();

            entity.HasIndex(e => e.UserName, "accounts_user_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .HasColumnName("email");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("joined_date");
            entity.Property(e => e.MeritPoint).HasColumnName("merit_point");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
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
                .HasColumnName("role");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<AccountBadge>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.BadgeId }).HasName("account_badges_pkey");

            entity.ToTable("account_badges");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.AwardedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("awarded_date");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountBadges)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("account_badges_account_id_fkey");

            entity.HasOne(d => d.Badge).WithMany(p => p.AccountBadges)
                .HasForeignKey(d => d.BadgeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("account_badges_badge_id_fkey");
        });

        modelBuilder.Entity<AccountNotification>(entity =>
        {
            entity.HasKey(e => new { e.AccountId, e.NotificationId }).HasName("account_notifications_pkey");

            entity.ToTable("account_notifications");

            entity.HasIndex(e => e.AccountId, "idx_account_notifications_unread").HasFilter("(is_read = false)");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.NotificationId).HasColumnName("notification_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.IsRead).HasColumnName("is_read");

            entity.HasOne(d => d.Account).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("account_notifications_account_id_fkey");

            entity.HasOne(d => d.Notification).WithMany(p => p.AccountNotifications)
                .HasForeignKey(d => d.NotificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("account_notifications_notification_id_fkey");
        });

        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("actors_pkey");

            entity.ToTable("actors");

            entity.HasIndex(e => e.CupSize, "idx_actors_cup_size");

            entity.HasIndex(e => e.DebutDate, "idx_actors_debut_date");

            entity.HasIndex(e => e.FullName, "idx_actors_full_name");

            entity.HasIndex(e => e.Gender, "idx_actors_gender");

            entity.HasIndex(e => e.Height, "idx_actors_height");

            entity.HasIndex(e => e.Nationality, "idx_actors_nationality");

            entity.HasIndex(e => e.StageName, "idx_actors_stage_name");

            entity.HasIndex(e => e.Status, "idx_actors_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Company)
                .HasMaxLength(200)
                .HasColumnName("company");
            entity.Property(e => e.CupSize)
                .HasMaxLength(5)
                .HasColumnName("cup_size");
            entity.Property(e => e.DebutDate).HasColumnName("debut_date");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Height)
                .HasMaxLength(5)
                .HasColumnName("height");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
                .HasColumnName("nationality");
            entity.Property(e => e.Size)
                .HasMaxLength(20)
                .HasColumnName("size");
            entity.Property(e => e.StageName).HasColumnName("stage_name");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<ActorImage>(entity =>
        {
            entity.HasKey(e => new { e.ActorId, e.ImageId }).HasName("actor_images_pkey");

            entity.ToTable("actor_images");

            entity.HasIndex(e => e.ImageId, "idx_actor_images_image_id");

            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.OrderNumerical)
                .HasDefaultValue((short)1)
                .HasColumnName("order_numerical");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Actor).WithMany(p => p.ActorImages)
                .HasForeignKey(d => d.ActorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("actor_images_actor_id_fkey");

            entity.HasOne(d => d.Image).WithMany(p => p.ActorImages)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("actor_images_image_id_fkey");
        });

        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("badges_pkey");

            entity.ToTable("badges");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("created_date");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IconUrl).HasColumnName("icon_url");
            entity.Property(e => e.LocalePath).HasColumnName("locale_path");
            entity.Property(e => e.RareLevel)
                .HasDefaultValue((short)1)
                .HasColumnName("rare_level");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Contribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contributions_pkey");

            entity.ToTable("contributions");

            entity.HasIndex(e => e.RequestedDate, "idx_contributions_pending").HasFilter("((status)::text = 'Pending'::text)");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "idx_contributions_target");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ActionType)
                .HasMaxLength(10)
                .HasColumnName("action_type");
            entity.Property(e => e.AdminReviewed).HasColumnName("admin_reviewed");
            entity.Property(e => e.ApproverId).HasColumnName("approver_id");
            entity.Property(e => e.ContributorId).HasColumnName("contributor_id");
            entity.Property(e => e.ProposedData)
                .HasColumnType("jsonb")
                .HasColumnName("proposed_data");
            entity.Property(e => e.RequestedDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("requested_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
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

            entity.HasIndex(e => e.LoverId, "idx_favorites_lover_id");

            entity.HasIndex(e => new { e.TargetType, e.TargetId }, "idx_favorites_target");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.LoverId).HasColumnName("lover_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.TargetType)
                .HasMaxLength(30)
                .HasColumnName("target_type");

            entity.HasOne(d => d.Lover).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.LoverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("favorites_lover_id_fkey");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("genres_pkey");

            entity.ToTable("genres");

            entity.HasIndex(e => e.Title, "genres_title_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("images_pkey");

            entity.ToTable("images");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.LocalePath).HasColumnName("locale_path");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.UploadDate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("upload_date");
            entity.Property(e => e.Url).HasColumnName("url");
        });

        modelBuilder.Entity<Label>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("labels_pkey");

            entity.ToTable("labels");

            entity.HasIndex(e => e.Title, "labels_title_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

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
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(30)
                .HasColumnName("type");
            entity.Property(e => e.TypeId).HasColumnName("type_id");
        });

        modelBuilder.Entity<Producer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("producers_pkey");

            entity.ToTable("producers");

            entity.HasIndex(e => e.Name, "producers_name_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstablishDate).HasColumnName("establish_date");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.OtherName)
                .HasMaxLength(200)
                .HasColumnName("other_name");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.HasIndex(e => e.Slug, "tags_slug_key").IsUnique();

            entity.HasIndex(e => e.Title, "tags_title_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Slug)
                .HasMaxLength(50)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Available'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("videos_pkey");

            entity.ToTable("videos");

            entity.HasIndex(e => e.Code, "idx_videos_code");

            entity.HasIndex(e => e.Description, "idx_videos_description");

            entity.HasIndex(e => e.Director, "idx_videos_director");

            entity.HasIndex(e => e.OriginalTitle, "idx_videos_original_title");

            entity.HasIndex(e => e.ReleaseDate, "idx_videos_release_date");

            entity.HasIndex(e => e.Series, "idx_videos_series");

            entity.HasIndex(e => e.Status, "idx_videos_status");

            entity.HasIndex(e => e.Title, "idx_videos_title");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .HasColumnName("code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director).HasColumnName("director");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Episode)
                .HasDefaultValue((short)1)
                .HasColumnName("episode");
            entity.Property(e => e.Language)
                .HasMaxLength(100)
                .HasColumnName("language");
            entity.Property(e => e.OriginalTitle).HasColumnName("original_title");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Series)
                .HasMaxLength(200)
                .HasColumnName("series");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasMany(d => d.Genres).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_genres_genre_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_genres_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "GenreId").HasName("video_genres_pkey");
                        j.ToTable("video_genres");
                        j.HasIndex(new[] { "GenreId" }, "idx_video_genres_genre_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("GenreId").HasColumnName("genre_id");
                    });

            entity.HasMany(d => d.Labels).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoLabel",
                    r => r.HasOne<Label>().WithMany()
                        .HasForeignKey("LabelId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_labels_label_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_labels_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "LabelId").HasName("video_labels_pkey");
                        j.ToTable("video_labels");
                        j.HasIndex(new[] { "LabelId" }, "idx_video_labels_label_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("LabelId").HasColumnName("label_id");
                    });

            entity.HasMany(d => d.Producers).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoProducer",
                    r => r.HasOne<Producer>().WithMany()
                        .HasForeignKey("ProducerId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_producers_producer_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_producers_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "ProducerId").HasName("video_producers_pkey");
                        j.ToTable("video_producers");
                        j.HasIndex(new[] { "ProducerId" }, "idx_video_producers_producer_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("ProducerId").HasColumnName("producer_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_tags_tag_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("video_tags_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "TagId").HasName("video_tags_pkey");
                        j.ToTable("video_tags");
                        j.HasIndex(new[] { "TagId" }, "idx_video_tags_tag_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<VideoActor>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.ActorId }).HasName("video_actors_pkey");

            entity.ToTable("video_actors");

            entity.HasIndex(e => e.ActorId, "idx_video_actors_actor_id");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.RoleMain).HasColumnName("role_main");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Actor).WithMany(p => p.VideoActors)
                .HasForeignKey(d => d.ActorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("video_actors_actor_id_fkey");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoActors)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("video_actors_video_id_fkey");
        });

        modelBuilder.Entity<VideoImage>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.ImageId }).HasName("video_images_pkey");

            entity.ToTable("video_images");

            entity.HasIndex(e => e.ImageId, "idx_video_images_image_id");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.OrderNumerical)
                .HasDefaultValue((short)1)
                .HasColumnName("order_numerical");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");

            entity.HasOne(d => d.Image).WithMany(p => p.VideoImages)
                .HasForeignKey(d => d.ImageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("video_images_image_id_fkey");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoImages)
                .HasForeignKey(d => d.VideoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("video_images_video_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
