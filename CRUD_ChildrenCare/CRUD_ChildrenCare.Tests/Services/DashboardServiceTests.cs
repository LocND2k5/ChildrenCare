using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Dashboards;
using CRUD_ChildrenCare.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace CRUD_ChildrenCare.Tests.Services;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardDataAsync_AggregatesCountsAndRevenueAccurately()
    {
        await using var factory = new TestWebApplicationFactory();

        await factory.ExecuteDbContextAsync(async context =>
        {
            var category = new Setting
            {
                Type = SettingType.ServiceCategory,
                Name = "Pediatric Specialty",
                Value = "pediatric-specialty",
                Status = SettingStatus.Active
            };
            context.Settings.Add(category);
            await context.SaveChangesAsync();

            var service = new Service
            {
                Title = "Specialist Exam",
                CategoryId = category.Id,
                ListPrice = 400000,
                SalePrice = 350000,
                AvailableQuantity = 10,
                Status = ServiceStatus.Active
            };
            context.Services.Add(service);
            await context.SaveChangesAsync();

            var successReservation = new Reservation
            {
                ReceiverFullName = "Child A",
                ReceiverEmail = "childA@example.com",
                ReceiverMobile = "0900000001",
                Status = ReservationStatus.Success,
                ReservedDate = DateTime.UtcNow,
                CheckupTime = DateTime.UtcNow,
                TotalCost = 350000
            };
            successReservation.ReservationItems.Add(new ReservationItem
            {
                ServiceId = service.Id,
                UnitPrice = 350000,
                Quantity = 1,
                NumberOfPerson = 1,
                TotalCost = 350000
            });

            var submittedReservation = new Reservation
            {
                ReceiverFullName = "Child B",
                ReceiverEmail = "childB@example.com",
                ReceiverMobile = "0900000002",
                Status = ReservationStatus.Submitted,
                ReservedDate = DateTime.UtcNow,
                CheckupTime = DateTime.UtcNow,
                TotalCost = 700000
            };

            context.Reservations.AddRange(successReservation, submittedReservation);
            await context.SaveChangesAsync();
        });

        using var scope = factory.Services.CreateScope();
        var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();

        var data = await dashboardService.GetDashboardDataAsync(
            DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, false);

        Assert.NotNull(data);
        Assert.Equal(1, data.SuccessReservations);
        Assert.Equal(1, data.SubmittedReservations);
        Assert.Equal(2, data.TotalReservations);
        Assert.Equal(350000, data.TotalRevenue);
        Assert.Single(data.RevenueByCategory);
        Assert.Equal("Pediatric Specialty", data.RevenueByCategory.First().CategoryName);
    }
}
