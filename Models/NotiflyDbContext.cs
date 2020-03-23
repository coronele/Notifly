using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace NotiflyV0._1.Models
{
    public partial class NotiflyDbContext : DbContext
    {
        public NotiflyDbContext()
        {
        }

        public NotiflyDbContext(DbContextOptions<NotiflyDbContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }

        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<EventTable> EventTable { get; set; }
        public virtual DbSet<GroupMembers> GroupMembers { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<MemberRsvp> MemberRsvp { get; set; }
        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public IConfiguration Configuration { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<EventTable>(entity =>
            {
                entity.HasKey(e => e.EventId)
                    .HasName("PK__EventTab__7944C810E23EAC94");

                entity.Property(e => e.DateAndTime).HasColumnType("datetime");

                entity.Property(e => e.EventDescription).HasMaxLength(400);

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.GroupName).HasMaxLength(50);

                entity.Property(e => e.NotificationDate).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(e => e.Venue).HasMaxLength(100);

                entity.Property(e => e.VenueLocation).HasMaxLength(100);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.EventTable)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__EventTabl__Group__6383C8BA");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EventTable)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__EventTabl__UserI__6477ECF3");
            });

            modelBuilder.Entity<GroupMembers>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__GroupMem__0CF04B18D3A6E672");

                entity.Property(e => e.MemberName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<Groups>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("PK__Groups__149AF36A481640AC");

                entity.Property(e => e.GroupName)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Groups)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Groups__UserId__5EBF139D");
            });

            modelBuilder.Entity<MemberRsvp>(entity =>
            {
                entity.HasKey(e => e.Rsvpid)
                    .HasName("PK__MemberRS__BD17ED56371D50FB");

                entity.ToTable("MemberRSVP");

                entity.Property(e => e.Rsvpid).HasColumnName("RSVPId");

                entity.Property(e => e.MemberName).HasMaxLength(60);

                entity.Property(e => e.Rsvp).HasColumnName("RSVP");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.MemberRsvp)
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__MemberRSV__Event__6754599E");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.MemberRsvp)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__MemberRSV__Membe__66603565");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(60);

                entity.Property(e => e.LastName).HasMaxLength(60);

                entity.Property(e => e.UserId).HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserInfo)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserInfo__UserId__75A278F5");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
