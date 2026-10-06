using CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Contacts;
using CRUD_ChildrenCare.Tests.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRUD_ChildrenCare.Tests.Services;

public sealed class ContactServiceTests
{
    [Fact]
    public async Task SyncContactFromReservationAsync_CreatesNewPotentialContactAndHistory()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IContactService>();

        var reservation = new Reservation
        {
            ReceiverFullName = "Pham Thi Huong",
            ReceiverGender = Gender.Female,
            ReceiverEmail = "huongpham@example.com",
            ReceiverMobile = "0987111222",
            ReceiverAddress = "Hanoi",
            Status = ReservationStatus.Submitted
        };

        var contact = await service.SyncContactFromReservationAsync(reservation);

        Assert.NotNull(contact);
        Assert.Equal("huongpham@example.com", contact.Email);
        Assert.Equal(ContactStatus.Potential, contact.Status);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var stored = await context.Contacts.Include(c => c.Histories).SingleAsync(c => c.Email == "huongpham@example.com");
            Assert.Equal(ContactStatus.Potential, stored.Status);
            Assert.Single(stored.Histories);
            Assert.Equal("Pham Thi Huong", stored.Histories.First().FullName);
        });
    }

    [Fact]
    public async Task PromoteToCustomerIfSuccessAsync_UpgradesStatusToCustomer()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IContactService>();

        await service.SyncContactFromFeedbackAsync("Vo Hoang Yen", Gender.Female, "yenvo@example.com", "0901234567");

        await factory.ExecuteDbContextAsync(async context =>
        {
            var stored = await context.Contacts.SingleAsync(c => c.Email == "yenvo@example.com");
            Assert.Equal(ContactStatus.Contact, stored.Status);
        });

        await service.PromoteToCustomerIfSuccessAsync("yenvo@example.com");

        await factory.ExecuteDbContextAsync(async context =>
        {
            var stored = await context.Contacts.SingleAsync(c => c.Email == "yenvo@example.com");
            Assert.Equal(ContactStatus.Customer, stored.Status);
        });
    }

    [Fact]
    public async Task UpdateCustomerAsync_TracksChangesInContactHistory()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IContactService>();

        await service.CreateCustomerAsync(new CreateCustomerViewModel
        {
            FullName = "Dang Van Lam",
            Gender = Gender.Male,
            Email = "lamdang@example.com",
            Mobile = "0933444555",
            Address = "District 7"
        }, null);

        int contactId = 0;
        await factory.ExecuteDbContextAsync(async context =>
        {
            var c = await context.Contacts.SingleAsync(x => x.Email == "lamdang@example.com");
            contactId = c.Id;
        });

        await service.UpdateCustomerAsync(new EditCustomerViewModel
        {
            Id = contactId,
            FullName = "Dang Van Lam Updated",
            Gender = Gender.Male,
            Email = "lamdang@example.com",
            Mobile = "0933444999",
            Address = "District 1, HCMC"
        }, null);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var stored = await context.Contacts.Include(c => c.Histories).SingleAsync(c => c.Id == contactId);
            Assert.Equal("Dang Van Lam Updated", stored.FullName);
            Assert.Equal(2, stored.Histories.Count);
        });
    }
}
