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

    public virtual DbSet<Actor> Actors { get; set; }

    public virtual DbSet<ActorImage> ActorImages { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Label> Labels { get; set; }

    public virtual DbSet<Producer> Producers { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<VideoActor> VideoActors { get; set; }

    public virtual DbSet<VideoImage> VideoImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=avedia_media;Username=postgres;Password=Johnny@1411!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Npgsql:CollationDefinition:public.case_insensitive", "und-u-ks-level1,und-u-ks-level1,icu,False");

        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("actors_pkey");

            entity.ToTable("actors");

            entity.HasIndex(e => e.Status, "idx_actors_status").UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Company)
                .HasMaxLength(200)
                .UseCollation("case_insensitive")
                .HasColumnName("company");
            entity.Property(e => e.CupSize)
                .HasMaxLength(5)
                .UseCollation("case_insensitive")
                .HasColumnName("cup_size");
            entity.Property(e => e.DebutDate).HasColumnName("debut_date");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FullName)
                .UseCollation("case_insensitive")
                .HasColumnName("full_name");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.Height)
                .HasMaxLength(5)
                .UseCollation("case_insensitive")
                .HasColumnName("height");
            entity.Property(e => e.Nationality)
                .HasMaxLength(100)
                .UseCollation("case_insensitive")
                .HasColumnName("nationality");
            entity.Property(e => e.Size)
                .HasMaxLength(20)
                .UseCollation("case_insensitive")
                .HasColumnName("size");
            entity.Property(e => e.StageName)
                .UseCollation("case_insensitive")
                .HasColumnName("stage_name");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Created'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
        });

        modelBuilder.Entity<ActorImage>(entity =>
        {
            entity.HasKey(e => new { e.ActorId, e.ImageId }).HasName("actor_images_pkey");

            entity.ToTable("actor_images");

            entity.HasIndex(e => e.ActorId, "idx_actor_images_actor_id");

            entity.HasIndex(e => e.ImageId, "idx_actor_images_image_id");

            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.IsAvartar).HasColumnName("is_avartar");
            entity.Property(e => e.IsCover).HasColumnName("is_cover");
            entity.Property(e => e.OrderNumerical)
                .HasDefaultValue((short)1)
                .HasColumnName("order_numerical");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .UseCollation("case_insensitive")
                .HasColumnName("status");

            entity.HasOne(d => d.Actor).WithMany(p => p.ActorImages)
                .HasForeignKey(d => d.ActorId)
                .HasConstraintName("actor_images_actor_id_fkey");

            entity.HasOne(d => d.Image).WithMany(p => p.ActorImages)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("actor_images_image_id_fkey");
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
                .HasDefaultValueSql("'Created'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .UseCollation("case_insensitive")
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
                .HasDefaultValueSql("'Created'::character varying")
                .UseCollation("case_insensitive")
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
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .UseCollation("case_insensitive")
                .HasColumnName("title");
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
                .UseCollation("case_insensitive")
                .HasColumnName("country");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstablishDate).HasColumnName("establish_date");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .UseCollation("case_insensitive")
                .HasColumnName("name");
            entity.Property(e => e.OtherName)
                .HasMaxLength(200)
                .HasColumnName("other_name");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Created'::character varying")
                .UseCollation("case_insensitive")
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
                .UseCollation("case_insensitive")
                .HasColumnName("slug");
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

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("videos_pkey");

            entity.ToTable("videos");

            entity.HasIndex(e => e.Code, "idx_videos_code").UseCollation(new[] { "case_insensitive" });

            entity.HasIndex(e => e.Status, "idx_videos_status").UseCollation(new[] { "case_insensitive" });

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(100)
                .UseCollation("case_insensitive")
                .HasColumnName("code");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director).HasColumnName("director");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Episode)
                .HasDefaultValue((short)1)
                .HasColumnName("episode");
            entity.Property(e => e.Language)
                .HasMaxLength(100)
                .UseCollation("case_insensitive")
                .HasColumnName("language");
            entity.Property(e => e.OriginalTitle)
                .UseCollation("case_insensitive")
                .HasColumnName("original_title");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Series)
                .HasMaxLength(200)
                .UseCollation("case_insensitive")
                .HasColumnName("series");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Created'::character varying")
                .UseCollation("case_insensitive")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .UseCollation("case_insensitive")
                .HasColumnName("title");

            entity.HasMany(d => d.Genres).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoGenre",
                    r => r.HasOne<Genre>().WithMany()
                        .HasForeignKey("GenreId")
                        .HasConstraintName("video_genres_genre_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .HasConstraintName("video_genres_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "GenreId").HasName("video_genres_pkey");
                        j.ToTable("video_genres");
                        j.HasIndex(new[] { "GenreId" }, "idx_video_genres_genre_id");
                        j.HasIndex(new[] { "VideoId" }, "idx_video_genres_video_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("GenreId").HasColumnName("genre_id");
                    });

            entity.HasMany(d => d.Labels).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoLabel",
                    r => r.HasOne<Label>().WithMany()
                        .HasForeignKey("LabelId")
                        .HasConstraintName("video_labels_label_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .HasConstraintName("video_labels_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "LabelId").HasName("video_labels_pkey");
                        j.ToTable("video_labels");
                        j.HasIndex(new[] { "LabelId" }, "idx_video_labels_label_id");
                        j.HasIndex(new[] { "VideoId" }, "idx_video_labels_video_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("LabelId").HasColumnName("label_id");
                    });

            entity.HasMany(d => d.Producers).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoProducer",
                    r => r.HasOne<Producer>().WithMany()
                        .HasForeignKey("ProducerId")
                        .HasConstraintName("video_producers_producer_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .HasConstraintName("video_producers_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "ProducerId").HasName("video_producers_pkey");
                        j.ToTable("video_producers");
                        j.HasIndex(new[] { "ProducerId" }, "idx_video_producers_producer_id");
                        j.HasIndex(new[] { "VideoId" }, "idx_video_producers_video_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("ProducerId").HasColumnName("producer_id");
                    });

            entity.HasMany(d => d.Tags).WithMany(p => p.Videos)
                .UsingEntity<Dictionary<string, object>>(
                    "VideoTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("video_tags_tag_id_fkey"),
                    l => l.HasOne<Video>().WithMany()
                        .HasForeignKey("VideoId")
                        .HasConstraintName("video_tags_video_id_fkey"),
                    j =>
                    {
                        j.HasKey("VideoId", "TagId").HasName("video_tags_pkey");
                        j.ToTable("video_tags");
                        j.HasIndex(new[] { "TagId" }, "idx_video_tags_tag_id");
                        j.HasIndex(new[] { "VideoId" }, "idx_video_tags_video_id");
                        j.IndexerProperty<Guid>("VideoId").HasColumnName("video_id");
                        j.IndexerProperty<Guid>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<VideoActor>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.ActorId }).HasName("video_actors_pkey");

            entity.ToTable("video_actors");

            entity.HasIndex(e => e.ActorId, "idx_video_actors_actor_id");

            entity.HasIndex(e => e.VideoId, "idx_video_actors_video_id");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.ActorId).HasColumnName("actor_id");
            entity.Property(e => e.RoleMain).HasColumnName("role_main");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .UseCollation("case_insensitive")
                .HasColumnName("status");

            entity.HasOne(d => d.Actor).WithMany(p => p.VideoActors)
                .HasForeignKey(d => d.ActorId)
                .HasConstraintName("video_actors_actor_id_fkey");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoActors)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("video_actors_video_id_fkey");
        });

        modelBuilder.Entity<VideoImage>(entity =>
        {
            entity.HasKey(e => new { e.VideoId, e.ImageId }).HasName("video_images_pkey");

            entity.ToTable("video_images");

            entity.HasIndex(e => e.ImageId, "idx_video_images_image_id");

            entity.HasIndex(e => e.VideoId, "idx_video_images_video_id");

            entity.Property(e => e.VideoId).HasColumnName("video_id");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.IsMain).HasColumnName("is_main");
            entity.Property(e => e.OrderNumerical)
                .HasDefaultValue((short)1)
                .HasColumnName("order_numerical");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .UseCollation("case_insensitive")
                .HasColumnName("status");

            entity.HasOne(d => d.Image).WithMany(p => p.VideoImages)
                .HasForeignKey(d => d.ImageId)
                .HasConstraintName("video_images_image_id_fkey");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoImages)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("video_images_video_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
