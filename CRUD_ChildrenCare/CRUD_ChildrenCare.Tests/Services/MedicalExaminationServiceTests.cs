using CRUD_ChildrenCare.Areas.Admin.ViewModels.MedicalExaminations;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.MedicalExaminations;
using CRUD_ChildrenCare.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRUD_ChildrenCare.Tests.Services;

public sealed class MedicalExaminationServiceTests
{
    [Fact]
    public async Task CreateExaminationAsync_RequiresAtLeastOneMedication()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMedicalExaminationService>();

        var model = new CreateMedicalExaminationViewModel
        {
            ReservationId = 1,
            ExaminationDate = DateTime.UtcNow,
            Diagnosis = "Viêm họng cấp",
            Items = []
        };

        var (success, error) = await service.CreateExaminationAsync(model, 1);

        Assert.False(success);
        Assert.Equal("Đơn thuốc phải có ít nhất 1 dòng thuốc.", error);
    }

    [Fact]
    public async Task CreateExaminationAsync_CreatesExamPrescriptionAndPromotesContactToCustomer()
    {
        await using var factory = new TestWebApplicationFactory();

        int reservationId = 0;
        int doctorId = 0;

        await factory.ExecuteDbContextAsync(async context =>
        {
            var doctorRole = new Setting
            {
                Type = SettingType.UserRole,
                Name = "Doctor",
                Value = "Doctor",
                Status = SettingStatus.Active
            };
            context.Settings.Add(doctorRole);
            await context.SaveChangesAsync();

            var doctor = new User
            {
                FullName = "Dr. Test Doctor",
                Email = "testdoctor@example.com",
                NormalizedEmail = "TESTDOCTOR@EXAMPLE.COM",
                Mobile = "0987654321",
                RoleId = doctorRole.Id,
                PasswordHash = "hash",
                Status = UserStatus.Active
            };
            context.Users.Add(doctor);

            var contact = new Contact
            {
                FullName = "Parent Patient",
                Email = "patientparent@example.com",
                Mobile = "0911222333",
                Gender = Gender.Male,
                Status = ContactStatus.Potential
            };
            context.Contacts.Add(contact);

            var reservation = new Reservation
            {
                ReceiverFullName = "Parent Patient",
                ReceiverEmail = "patientparent@example.com",
                ReceiverMobile = "0911222333",
                ReceiverGender = Gender.Male,
                Status = ReservationStatus.Submitted,
                CheckupTime = DateTime.UtcNow.AddHours(2),
                TotalCost = 300000,
                AssignedStaff = doctor
            };
            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();

            reservationId = reservation.Id;
            doctorId = doctor.Id;
        });

        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMedicalExaminationService>();

        var model = new CreateMedicalExaminationViewModel
        {
            ReservationId = reservationId,
            DoctorId = doctorId,
            ExaminationDate = DateTime.UtcNow,
            Diagnosis = "Sốt phát ban thể nhẹ",
            Note = "Nghỉ ngơi, uống oresol",
            Items =
            [
                new PrescriptionItemInputViewModel
                {
                    MedicineName = "Hapacol 250mg",
                    Dosage = "1 gói/lần",
                    Quantity = 5,
                    Usage = "Khi sốt > 38.5 độ"
                }
            ]
        };

        var (success, error) = await service.CreateExaminationAsync(model, doctorId);

        Assert.True(success, error);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var res = await context.Reservations.SingleAsync(r => r.Id == reservationId);
            Assert.Equal(ReservationStatus.Success, res.Status);

            var con = await context.Contacts.SingleAsync(c => c.Email == "patientparent@example.com");
            Assert.Equal(ContactStatus.Customer, con.Status);

            var exam = await context.MedicalExaminations
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.PrescriptionItems)
                .SingleAsync(m => m.ReservationId == reservationId);

            Assert.Single(exam.Prescriptions);
            Assert.Single(exam.Prescriptions.First().PrescriptionItems);
            Assert.Equal("Hapacol 250mg", exam.Prescriptions.First().PrescriptionItems.First().MedicineName);
        });
    }
}
