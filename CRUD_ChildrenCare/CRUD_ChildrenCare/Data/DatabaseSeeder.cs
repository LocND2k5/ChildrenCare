using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Email;
using CRUD_ChildrenCare.Services.Security;
using CRUD_ChildrenCare.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Data;

public sealed class DatabaseSeeder(
    ApplicationDbContext context,
    IPasswordHasher<User> passwordHasher,
    ISecureTokenService tokenService,
    IEmailSender emailSender,
    ILogger<DatabaseSeeder> logger)
{
    private const string InitialAdminEmail = "admin@childrencare.local";

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedSettingsAsync(cancellationToken);
        await SeedRoleMenusAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
        await SeedTestUsersAsync(cancellationToken);
        await SeedSampleServicesAsync(cancellationToken);
        await SeedPart5DataAsync(cancellationToken);

        // Auto-promote shinichi2542003@gmail.com and kkk@gmail.com
        var emailsToPromote = new[] { "shinichi2542003@gmail.com", "kkk@gmail.com" };
        foreach (var email in emailsToPromote)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (user != null)
            {
                var adminRoleId = await context.Settings.Where(s => s.Type == SettingType.UserRole && s.Value == SystemData.RoleValues.Admin).Select(s => s.Id).FirstOrDefaultAsync(cancellationToken);
                user.RoleId = adminRoleId;
                user.Status = UserStatus.Active; // Activate account so they don't have to verify email
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSettingsAsync(CancellationToken cancellationToken)
    {
        var existingKeys = await context.Settings
            .AsNoTracking()
            .Select(item => new { item.Type, item.Name })
            .ToListAsync(cancellationToken);
        var existing = existingKeys
            .Select(item => $"{item.Type}:{item.Name}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleValue in SystemData.RoleValues.All)
        {
            AddSettingIfMissing(
                existing,
                new SettingSeed(SettingType.UserRole, roleValue, roleValue, $"System role: {roleValue}"));
        }

        foreach (var seed in SystemData.AdminMenus.Concat(SystemData.SampleCategories))
        {
            AddSettingIfMissing(existing, seed);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private void AddSettingIfMissing(HashSet<string> existing, SettingSeed seed)
    {
        var key = $"{seed.Type}:{seed.Name}";
        if (!existing.Add(key))
        {
            return;
        }

        context.Settings.Add(new Setting
        {
            Type = seed.Type,
            Name = seed.Name,
            Value = seed.Value,
            Description = seed.Description,
            Status = SettingStatus.Active
        });
    }

    private async Task SeedRoleMenusAsync(CancellationToken cancellationToken)
    {
        var targetRoleValues = new[] { SystemData.RoleValues.Admin, SystemData.RoleValues.Manager, SystemData.RoleValues.Doctor, SystemData.RoleValues.Nurse };
        var roleIds = await context.Settings
            .Where(item => item.Type == SettingType.UserRole && targetRoleValues.Contains(item.Value))
            .Select(item => new { item.Value, item.Id })
            .ToListAsync(cancellationToken);

        var menuIds = await context.Settings
            .Where(item => item.Type == SettingType.AdminMenu)
            .ToListAsync(cancellationToken);

        foreach (var role in roleIds)
        {
            var existingMenuIds = await context.RoleMenus
                .Where(item => item.RoleId == role.Id)
                .Select(item => item.MenuId)
                .ToListAsync(cancellationToken);

            foreach (var menu in menuIds.Where(m => !existingMenuIds.Contains(m.Id)))
            {
                context.RoleMenus.Add(new RoleMenu { RoleId = role.Id, MenuId = menu.Id });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var normalizedEmail = AccountValidation.NormalizeEmail(InitialAdminEmail);
        if (await context.Users.AnyAsync(item => item.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return;
        }

        var adminRoleId = await context.Settings
            .Where(item => item.Type == SettingType.UserRole && item.Value == SystemData.RoleValues.Admin)
            .Select(item => item.Id)
            .SingleAsync(cancellationToken);
        var temporaryPassword = $"Aa1!{tokenService.CreateToken()[..12]}";
        var admin = new User
        {
            FullName = "System Administrator",
            Gender = Gender.Other,
            Email = InitialAdminEmail,
            NormalizedEmail = normalizedEmail,
            Mobile = "0000000000",
            RoleId = adminRoleId,
            Status = UserStatus.Active
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, temporaryPassword);
        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            new EmailMessage(
                admin.Email,
                "Children Care administrator account",
                $"Your temporary password is <strong>{temporaryPassword}</strong>. Change it after signing in."),
            cancellationToken);
        logger.LogInformation("Initial administrator account created for {Email}.", admin.Email);
    }

    private async Task SeedTestUsersAsync(CancellationToken cancellationToken)
    {
        var testAccounts = new[]
        {
            new { Email = "manager@childrencare.local", Name = "Service Manager", Role = SystemData.RoleValues.Manager },
            new { Email = "doctor@childrencare.local", Name = "Dr. Sarah Jenkins", Role = SystemData.RoleValues.Doctor },
            new { Email = "customer@childrencare.local", Name = "John Customer", Role = SystemData.RoleValues.Customer }
        };

        foreach (var acc in testAccounts)
        {
            var normalizedEmail = AccountValidation.NormalizeEmail(acc.Email);
            if (!await context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
            {
                var roleId = await context.Settings
                    .Where(s => s.Type == SettingType.UserRole && s.Value == acc.Role)
                    .Select(s => s.Id)
                    .SingleAsync(cancellationToken);

                var user = new User
                {
                    FullName = acc.Name,
                    Gender = Gender.Male,
                    Email = acc.Email,
                    NormalizedEmail = normalizedEmail,
                    Mobile = "0987654321",
                    RoleId = roleId,
                    Status = UserStatus.Active
                };
                user.PasswordHash = passwordHasher.HashPassword(user, "Password123!");
                context.Users.Add(user);
            }
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSampleServicesAsync(CancellationToken cancellationToken)
    {
        if (await context.Services.AnyAsync(cancellationToken))
        {
            return;
        }

        var categoryId = await context.Settings
            .Where(s => s.Type == SettingType.ServiceCategory)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryId == 0) return;

        var service1 = new Service
        {
            Title = "General Pediatric Checkup",
            Thumbnail = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=600",
            CategoryId = categoryId,
            BriefInfo = "Comprehensive physical examination and growth tracking for infants and children.",
            Description = "<h4>Pediatric Care Package</h4><p>Our general checkup includes vital sign monitoring, physical examination, growth milestones tracking, and personalized nutritional guidance.</p>",
            NumberOfPerson = 1,
            ListPrice = 500000,
            SalePrice = 450000,
            AvailableQuantity = 30,
            IsFeatured = true,
            Status = ServiceStatus.Active
        };

        var service2 = new Service
        {
            Title = "Pediatric Dental Examination",
            Thumbnail = "https://images.unsplash.com/photo-1588776814546-1ffcf47267a5?w=600",
            CategoryId = categoryId,
            BriefInfo = "Gentle dental checkup, cavity prevention, and teeth cleaning for children.",
            Description = "<h4>Pediatric Dentistry</h4><p>Ensuring healthy smiles from an early age. Includes oral hygiene assessment, gentle cleaning, and fluoridation.</p>",
            NumberOfPerson = 1,
            ListPrice = 350000,
            SalePrice = 300000,
            AvailableQuantity = 20,
            IsFeatured = true,
            Status = ServiceStatus.Active
        };

        var service3 = new Service
        {
            Title = "Child Vaccination & Immunization",
            Thumbnail = "https://images.unsplash.com/photo-1632053002928-1906a5829633?w=600",
            CategoryId = categoryId,
            BriefInfo = "Standard childhood vaccines administered by certified healthcare professionals.",
            Description = "<h4>Vaccination Service</h4><p>Keep your child protected against preventable diseases according to standard international immunization schedules.</p>",
            NumberOfPerson = 1,
            ListPrice = 650000,
            SalePrice = 600000,
            AvailableQuantity = 50,
            IsFeatured = false,
            Status = ServiceStatus.Active
        };

        context.Services.AddRange(service1, service2, service3);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedPart5DataAsync(CancellationToken cancellationToken)
    {
        if (await context.Contacts.AnyAsync(cancellationToken))
        {
            return;
        }

        var contact1 = new Contact
        {
            FullName = "Nguyen Thi Mai",
            Gender = Gender.Female,
            Email = "mainguyen@gmail.com",
            Mobile = "0912345678",
            Address = "123 Le Loi, District 1, HCMC",
            Status = ContactStatus.Customer,
            CreatedDate = DateTime.UtcNow.AddDays(-10),
            UpdatedDate = DateTime.UtcNow.AddDays(-2)
        };

        var contact2 = new Contact
        {
            FullName = "Tran Van Bao",
            Gender = Gender.Male,
            Email = "baotran@gmail.com",
            Mobile = "0987654321",
            Address = "456 Nguyen Trai, District 5, HCMC",
            Status = ContactStatus.Potential,
            CreatedDate = DateTime.UtcNow.AddDays(-5),
            UpdatedDate = DateTime.UtcNow.AddDays(-5)
        };

        var contact3 = new Contact
        {
            FullName = "Le Hoang Nam",
            Gender = Gender.Male,
            Email = "namle@gmail.com",
            Mobile = "0903123456",
            Address = "789 Hai Ba Trung, District 3, HCMC",
            Status = ContactStatus.Contact,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            UpdatedDate = DateTime.UtcNow.AddDays(-1)
        };

        context.Contacts.AddRange(contact1, contact2, contact3);
        await context.SaveChangesAsync(cancellationToken);

        // Seed histories
        context.ContactHistories.AddRange(
            new ContactHistory
            {
                ContactId = contact1.Id,
                FullName = "Nguyen Thi Mai",
                Gender = Gender.Female,
                Email = "mainguyen@gmail.com",
                Mobile = "0912345678",
                Address = "123 Le Loi, District 1, HCMC",
                UpdatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new ContactHistory
            {
                ContactId = contact2.Id,
                FullName = "Tran Van Bao",
                Gender = Gender.Male,
                Email = "baotran@gmail.com",
                Mobile = "0987654321",
                Address = "456 Nguyen Trai, District 5, HCMC",
                UpdatedDate = DateTime.UtcNow.AddDays(-5)
            },
            new ContactHistory
            {
                ContactId = contact3.Id,
                FullName = "Le Hoang Nam",
                Gender = Gender.Male,
                Email = "namle@gmail.com",
                Mobile = "0903123456",
                Address = "789 Hai Ba Trung, District 3, HCMC",
                UpdatedDate = DateTime.UtcNow.AddDays(-1)
            }
        );

        // Seed Feedbacks
        var firstService = await context.Services.FirstOrDefaultAsync(cancellationToken);
        var feedback1 = new Feedback
        {
            FullName = "Nguyen Thi Mai",
            Gender = Gender.Female,
            Email = "mainguyen@gmail.com",
            Mobile = "0912345678",
            ServiceId = firstService?.Id,
            RatedStar = 5,
            Content = "Dịch vụ khám tổng quát rất tận tâm và nhẹ nhàng với bé. Phòng khám sạch sẽ, bác sĩ tư vấn rất kỹ lưỡng!",
            Status = FeedbackStatus.Published,
            CreatedDate = DateTime.UtcNow.AddDays(-3)
        };

        var feedback2 = new Feedback
        {
            FullName = "Tran Van Bao",
            Gender = Gender.Male,
            Email = "baotran@gmail.com",
            Mobile = "0987654321",
            ServiceId = null,
            RatedStar = 5,
            Content = "Hệ thống đặt lịch rất tiện lợi, trung tâm hỗ trợ nhiệt tình và chu đáo.",
            Status = FeedbackStatus.Published,
            CreatedDate = DateTime.UtcNow.AddDays(-1)
        };

        context.Feedbacks.AddRange(feedback1, feedback2);

        // Seed a sample reservation with Medical Examination if doctor exists
        var doctor = await context.Users.FirstOrDefaultAsync(u => u.Email == "doctor@childrencare.local", cancellationToken);
        if (doctor != null && firstService != null)
        {
            var res = new Reservation
            {
                ReceiverFullName = "Nguyen Thi Mai",
                ReceiverGender = Gender.Female,
                ReceiverEmail = "mainguyen@gmail.com",
                ReceiverMobile = "0912345678",
                ReceiverAddress = "123 Le Loi, District 1, HCMC",
                CheckupTime = DateTime.UtcNow.AddDays(-2),
                ReservedDate = DateTime.UtcNow.AddDays(-3),
                Status = ReservationStatus.Success,
                AssignedStaffId = doctor.Id,
                TotalCost = firstService.SalePrice,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                UpdatedDate = DateTime.UtcNow.AddDays(-2)
            };
            res.ReservationItems.Add(new ReservationItem
            {
                ServiceId = firstService.Id,
                UnitPrice = firstService.SalePrice,
                Quantity = 1,
                NumberOfPerson = 1,
                TotalCost = firstService.SalePrice
            });
            context.Reservations.Add(res);
            await context.SaveChangesAsync(cancellationToken);

            var exam = new MedicalExamination
            {
                ReservationId = res.Id,
                ExaminationDate = DateTime.UtcNow.AddDays(-2),
                DoctorId = doctor.Id,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                UpdatedDate = DateTime.UtcNow.AddDays(-2)
            };

            var prescription = new Prescription
            {
                Diagnosis = "Viêm họng cấp thể nhẹ ở trẻ, có sốt nhẹ không biến chứng",
                Note = "Giữ ấm cổ họng cho bé, uống nhiều nước ấm, tái khám sau 3 ngày nếu còn sốt cao.",
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            };
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                MedicineName = "Paracetamol gói 150mg",
                Dosage = "1 gói/lần khi sốt trên 38.5 độ",
                Quantity = 6,
                Usage = "Pha với nước ấm, cách nhau ít nhất 4-6 tiếng"
            });
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                MedicineName = "Siro ho thảo dược Prospan",
                Dosage = "2.5ml/lần, ngày 3 lần",
                Quantity = 1,
                Usage = "Uống sau ăn sáng, trưa, tối"
            });
            prescription.PrescriptionItems.Add(new PrescriptionItem
            {
                MedicineName = "Vitamin C siro 100ml",
                Dosage = "5ml/ngày",
                Quantity = 1,
                Usage = "Uống vào buổi sáng sau ăn để tăng đề kháng"
            });

            exam.Prescriptions.Add(prescription);
            context.MedicalExaminations.Add(exam);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
