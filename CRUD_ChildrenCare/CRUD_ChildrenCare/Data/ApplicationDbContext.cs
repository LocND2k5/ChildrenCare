using CRUD_ChildrenCare.Models;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Slider> Sliders => Set<Slider>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceImage> ServiceImages => Set<ServiceImage>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactHistory> ContactHistories => Set<ContactHistory>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<FeedbackImage> FeedbackImages => Set<FeedbackImage>();
    public DbSet<MedicalExamination> MedicalExaminations => Set<MedicalExamination>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var setting = modelBuilder.Entity<Setting>();
        setting.Property(item => item.Type).HasConversion<string>().HasMaxLength(50);
        setting.Property(item => item.Name).HasMaxLength(100);
        setting.Property(item => item.Value).HasMaxLength(255);
        setting.Property(item => item.Description).HasMaxLength(500);
        setting.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            setting.Property(item => item.Name).UseCollation("NOCASE");
        }

        setting.HasIndex(item => new { item.Type, item.Name }).IsUnique();

        var user = modelBuilder.Entity<User>();
        user.Property(item => item.FullName).HasMaxLength(100);
        user.Property(item => item.Gender).HasConversion<string>().HasMaxLength(20);
        user.Property(item => item.Email).HasMaxLength(100);
        user.Property(item => item.NormalizedEmail).HasMaxLength(100);
        user.Property(item => item.Mobile).HasMaxLength(10).IsUnicode(false);
        user.Property(item => item.Address).HasMaxLength(255);
        user.Property(item => item.AvatarUrl).HasMaxLength(255);
        user.Property(item => item.PasswordHash).HasMaxLength(255);
        user.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        user.Property(item => item.VerifyTokenHash).HasMaxLength(100);
        user.Property(item => item.ResetTokenHash).HasMaxLength(100);
        user.HasIndex(item => item.NormalizedEmail).IsUnique();
        user.HasOne(item => item.Role)
            .WithMany(item => item.Users)
            .HasForeignKey(item => item.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        var roleMenu = modelBuilder.Entity<RoleMenu>();
        roleMenu.HasKey(item => new { item.RoleId, item.MenuId });
        roleMenu.HasOne(item => item.Role)
            .WithMany(item => item.RoleMenuRoles)
            .HasForeignKey(item => item.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        roleMenu.HasOne(item => item.Menu)
            .WithMany(item => item.RoleMenuEntries)
            .HasForeignKey(item => item.MenuId)
            .OnDelete(DeleteBehavior.Restrict);

        var post = modelBuilder.Entity<Post>();
        post.Property(item => item.Title).HasMaxLength(200);
        post.Property(item => item.Thumbnail).HasMaxLength(255);
        post.Property(item => item.BriefInfo).HasMaxLength(500);
        post.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        post.HasOne(item => item.Category)
            .WithMany()
            .HasForeignKey(item => item.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        post.HasOne(item => item.Author)
            .WithMany()
            .HasForeignKey(item => item.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        var slider = modelBuilder.Entity<Slider>();
        slider.Property(item => item.Title).HasMaxLength(200);
        slider.Property(item => item.ImageUrl).HasMaxLength(255);
        slider.Property(item => item.BackLink).HasMaxLength(500);
        slider.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        slider.Property(item => item.Notes).HasMaxLength(500);

        var service = modelBuilder.Entity<Service>();
        service.Property(item => item.Title).HasMaxLength(200);
        service.Property(item => item.Thumbnail).HasMaxLength(255);
        service.Property(item => item.BriefInfo).HasMaxLength(500);
        service.Property(item => item.ListPrice).HasColumnType("decimal(18,0)");
        service.Property(item => item.SalePrice).HasColumnType("decimal(18,0)");
        service.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        service.HasOne(item => item.Category)
            .WithMany()
            .HasForeignKey(item => item.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        service.HasMany(item => item.ServiceImages)
            .WithOne(item => item.Service)
            .HasForeignKey(item => item.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        var serviceImage = modelBuilder.Entity<ServiceImage>();
        serviceImage.Property(item => item.ImageUrl).HasMaxLength(255);

        var reservation = modelBuilder.Entity<Reservation>();
        reservation.Property(item => item.ReceiverGender).HasConversion<string>().HasMaxLength(20);
        reservation.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        reservation.Property(item => item.ReceiverMobile).IsUnicode(false);
        reservation.HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        reservation.HasOne(item => item.AssignedStaff)
            .WithMany()
            .HasForeignKey(item => item.AssignedStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        var reservationItem = modelBuilder.Entity<ReservationItem>();
        reservationItem.HasOne(item => item.Reservation)
            .WithMany(item => item.ReservationItems)
            .HasForeignKey(item => item.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);
        reservationItem.HasOne(item => item.Service)
            .WithMany()
            .HasForeignKey(item => item.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        var contact = modelBuilder.Entity<Contact>();
        contact.Property(item => item.FullName).HasMaxLength(100);
        contact.Property(item => item.Gender).HasConversion<string>().HasMaxLength(20);
        contact.Property(item => item.Email).HasMaxLength(100);
        contact.Property(item => item.Mobile).HasMaxLength(10).IsUnicode(false);
        contact.Property(item => item.Address).HasMaxLength(255);
        contact.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        contact.HasIndex(item => item.Email).IsUnique();

        var contactHistory = modelBuilder.Entity<ContactHistory>();
        contactHistory.Property(item => item.FullName).HasMaxLength(100);
        contactHistory.Property(item => item.Gender).HasConversion<string>().HasMaxLength(20);
        contactHistory.Property(item => item.Email).HasMaxLength(100);
        contactHistory.Property(item => item.Mobile).HasMaxLength(10).IsUnicode(false);
        contactHistory.Property(item => item.Address).HasMaxLength(255);
        contactHistory.HasOne(item => item.Contact)
            .WithMany(item => item.Histories)
            .HasForeignKey(item => item.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
        contactHistory.HasOne(item => item.UpdatedBy)
            .WithMany()
            .HasForeignKey(item => item.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        var feedback = modelBuilder.Entity<Feedback>();
        feedback.Property(item => item.FullName).HasMaxLength(100);
        feedback.Property(item => item.Gender).HasConversion<string>().HasMaxLength(20);
        feedback.Property(item => item.Email).HasMaxLength(100);
        feedback.Property(item => item.Mobile).HasMaxLength(10).IsUnicode(false);
        feedback.Property(item => item.Content).HasMaxLength(1000);
        feedback.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
        feedback.HasOne(item => item.Service)
            .WithMany()
            .HasForeignKey(item => item.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        var feedbackImage = modelBuilder.Entity<FeedbackImage>();
        feedbackImage.Property(item => item.ImageUrl).HasMaxLength(255);
        feedbackImage.HasOne(item => item.Feedback)
            .WithMany(item => item.FeedbackImages)
            .HasForeignKey(item => item.FeedbackId)
            .OnDelete(DeleteBehavior.Cascade);

        var medicalExam = modelBuilder.Entity<MedicalExamination>();
        medicalExam.HasOne(item => item.Reservation)
            .WithMany()
            .HasForeignKey(item => item.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
        medicalExam.HasOne(item => item.Doctor)
            .WithMany()
            .HasForeignKey(item => item.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        var prescription = modelBuilder.Entity<Prescription>();
        prescription.Property(item => item.Diagnosis).HasMaxLength(500);
        prescription.Property(item => item.Note).HasMaxLength(500);
        prescription.HasOne(item => item.MedicalExamination)
            .WithMany(item => item.Prescriptions)
            .HasForeignKey(item => item.ExaminationId)
            .OnDelete(DeleteBehavior.Cascade);

        var prescriptionItem = modelBuilder.Entity<PrescriptionItem>();
        prescriptionItem.Property(item => item.MedicineName).HasMaxLength(150);
        prescriptionItem.Property(item => item.Dosage).HasMaxLength(100);
        prescriptionItem.Property(item => item.Usage).HasMaxLength(255);
        prescriptionItem.HasOne(item => item.Prescription)
            .WithMany(item => item.PrescriptionItems)
            .HasForeignKey(item => item.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Post>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Slider>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Service>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Reservation>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Contact>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ContactHistory>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Feedback>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<MedicalExamination>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }

            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedDate = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<Prescription>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
