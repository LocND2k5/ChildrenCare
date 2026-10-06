using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Tests.Data;

public sealed class ApplicationDbContextTests
{
    [Fact]
    public async Task SaveChanges_RejectsDuplicateNormalizedEmail()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var role = CreateRole("Customer");
        fixture.Context.Settings.Add(role);
        await fixture.Context.SaveChangesAsync();

        fixture.Context.Users.AddRange(
            CreateUser("first@example.com", "DUPLICATE@EXAMPLE.COM", role.Id),
            CreateUser("duplicate@example.com", "DUPLICATE@EXAMPLE.COM", role.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_RejectsDuplicateSettingTypeAndName()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        fixture.Context.Settings.AddRange(
            CreateCategory("General", "general"),
            CreateCategory("general", "general-duplicate"));

        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task RoleMenu_RejectsDuplicateAssignment()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var role = CreateRole("Admin");
        var menu = new Setting
        {
            Type = SettingType.AdminMenu,
            Name = "Users",
            Value = "/Admin/Users",
            Status = SettingStatus.Active
        };
        fixture.Context.Settings.AddRange(role, menu);
        await fixture.Context.SaveChangesAsync();

        fixture.Context.RoleMenus.Add(new RoleMenu { RoleId = role.Id, MenuId = menu.Id });
        await fixture.Context.SaveChangesAsync();
        fixture.Context.ChangeTracker.Clear();
        fixture.Context.RoleMenus.Add(new RoleMenu { RoleId = role.Id, MenuId = menu.Id });

        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task RoleDelete_IsRestrictedWhenUserExists()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var role = CreateRole("Doctor");
        fixture.Context.Settings.Add(role);
        await fixture.Context.SaveChangesAsync();
        fixture.Context.Users.Add(CreateUser("doctor@example.com", "DOCTOR@EXAMPLE.COM", role.Id));
        await fixture.Context.SaveChangesAsync();

        fixture.Context.ChangeTracker.Clear();
        var storedRole = await fixture.Context.Settings.SingleAsync(item => item.Id == role.Id);
        fixture.Context.Settings.Remove(storedRole);

        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task Contact_RejectsDuplicateEmail()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        fixture.Context.Contacts.Add(new Contact
        {
            FullName = "First Contact",
            Gender = Gender.Male,
            Email = "contact@example.com",
            Mobile = "0123456789",
            Status = ContactStatus.Contact
        });
        await fixture.Context.SaveChangesAsync();

        fixture.Context.Contacts.Add(new Contact
        {
            FullName = "Second Contact",
            Gender = Gender.Female,
            Email = "contact@example.com",
            Mobile = "0987654321",
            Status = ContactStatus.Potential
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => fixture.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task MedicalExamination_SavesPrescriptionAndItemsSuccessfully()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var role = CreateRole("Doctor");
        fixture.Context.Settings.Add(role);
        await fixture.Context.SaveChangesAsync();

        var doctor = CreateUser("doctor@example.com", "DOCTOR@EXAMPLE.COM", role.Id);
        fixture.Context.Users.Add(doctor);

        var reservation = new Reservation
        {
            ReceiverFullName = "Child Patient",
            ReceiverGender = Gender.Male,
            ReceiverEmail = "parent@example.com",
            ReceiverMobile = "0912345678",
            CheckupTime = DateTime.UtcNow,
            Status = ReservationStatus.Success,
            TotalCost = 500000
        };
        fixture.Context.Reservations.Add(reservation);
        await fixture.Context.SaveChangesAsync();

        var exam = new MedicalExamination
        {
            ReservationId = reservation.Id,
            DoctorId = doctor.Id,
            ExaminationDate = DateTime.UtcNow
        };

        var rx = new Prescription
        {
            Diagnosis = "Mild pharyngitis",
            Note = "Drink warm fluids"
        };
        rx.PrescriptionItems.Add(new PrescriptionItem
        {
            MedicineName = "Paracetamol",
            Dosage = "1 pack / 6 hours",
            Quantity = 4
        });
        exam.Prescriptions.Add(rx);

        fixture.Context.MedicalExaminations.Add(exam);
        await fixture.Context.SaveChangesAsync();

        var stored = await fixture.Context.MedicalExaminations
            .Include(m => m.Prescriptions)
                .ThenInclude(p => p.PrescriptionItems)
            .SingleAsync(m => m.Id == exam.Id);

        Assert.Single(stored.Prescriptions);
        Assert.Single(stored.Prescriptions.First().PrescriptionItems);
        Assert.Equal("Paracetamol", stored.Prescriptions.First().PrescriptionItems.First().MedicineName);
    }

    private static Setting CreateRole(string value) => new()
    {
        Type = SettingType.UserRole,
        Name = value,
        Value = value,
        Status = SettingStatus.Active
    };

    private static Setting CreateCategory(string name, string value) => new()
    {
        Type = SettingType.PostCategory,
        Name = name,
        Value = value,
        Status = SettingStatus.Active
    };

    private static User CreateUser(string email, string normalizedEmail, int roleId) => new()
    {
        FullName = "Test User",
        Gender = Gender.Other,
        Email = email,
        NormalizedEmail = normalizedEmail,
        Mobile = "0123456789",
        PasswordHash = "hashed-password",
        RoleId = roleId,
        Status = UserStatus.Active
    };

    private sealed class DatabaseFixture : IAsyncDisposable
    {
        private DatabaseFixture(SqliteConnection connection, ApplicationDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        private SqliteConnection Connection { get; }
        public ApplicationDbContext Context { get; }

        public static async Task<DatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();
            return new DatabaseFixture(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
