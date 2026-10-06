using CRUD_ChildrenCare.Areas.Admin.ViewModels.Dashboard;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Services.Dashboards;

public sealed class DashboardService(ApplicationDbContext dbContext) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? fromDate, DateTime? toDate, bool isManager, CancellationToken ct = default)
    {
        var end = (toDate ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
        var start = (fromDate ?? DateTime.UtcNow.AddDays(-6)).Date;

        if (start > end)
        {
            start = end.Date.AddDays(-6);
        }

        var model = new DashboardViewModel
        {
            FromDate = start,
            ToDate = end.Date,
            IsManagerView = isManager
        };

        // Reservations in range
        var reservationsQuery = dbContext.Reservations
            .Where(r => r.ReservedDate >= start && r.ReservedDate <= end);

        var reservationsList = await reservationsQuery.ToListAsync(ct);

        model.TotalReservations = reservationsList.Count(r => r.Status != ReservationStatus.Cart);
        model.SuccessReservations = reservationsList.Count(r => r.Status == ReservationStatus.Success);
        model.SubmittedReservations = reservationsList.Count(r => r.Status == ReservationStatus.Submitted);
        model.CancelledReservations = reservationsList.Count(r => r.Status == ReservationStatus.Cancelled);
        model.CartReservations = await dbContext.Reservations.CountAsync(r => r.Status == ReservationStatus.Cart, ct);

        // Revenue
        model.TotalRevenue = reservationsList
            .Where(r => r.Status == ReservationStatus.Success)
            .Sum(r => r.TotalCost);

        // Revenue by category
        var itemsInRange = await dbContext.ReservationItems
            .Include(i => i.Service)
                .ThenInclude(s => s.Category)
            .Include(i => i.Reservation)
            .Where(i => i.Reservation.Status == ReservationStatus.Success
                        && i.Reservation.ReservedDate >= start
                        && i.Reservation.ReservedDate <= end)
            .ToListAsync(ct);

        model.RevenueByCategory = itemsInRange
            .GroupBy(i => i.Service?.Category?.Name ?? "Uncategorized")
            .Select(g => new CategoryRevenueItem(
                g.Key,
                g.Sum(x => x.TotalCost),
                g.Select(x => x.ReservationId).Distinct().Count()))
            .OrderByDescending(x => x.Revenue)
            .ToList();

        // Users & Contacts KPI
        model.NewUsersCount = await dbContext.Users
            .CountAsync(u => u.CreatedDate >= start && u.CreatedDate <= end, ct);

        model.TotalContacts = await dbContext.Contacts.CountAsync(ct);
        model.ContactCount = await dbContext.Contacts.CountAsync(c => c.Status == ContactStatus.Contact, ct);
        model.PotentialCount = await dbContext.Contacts.CountAsync(c => c.Status == ContactStatus.Potential, ct);
        model.CustomerCount = await dbContext.Contacts.CountAsync(c => c.Status == ContactStatus.Customer, ct);

        // Feedbacks KPI
        var feedbacks = await dbContext.Feedbacks
            .Include(f => f.Service)
            .Where(f => f.Status == FeedbackStatus.Published)
            .ToListAsync(ct);

        model.TotalFeedbacks = feedbacks.Count;
        model.AverageRating = feedbacks.Count > 0
            ? Math.Round(feedbacks.Average(f => f.RatedStar), 1)
            : 0;

        model.RatingByService = feedbacks
            .GroupBy(f => f.Service?.Title ?? "General Care & System")
            .Select(g => new ServiceRatingItem(
                g.Key,
                Math.Round(g.Average(x => x.RatedStar), 1),
                g.Count()))
            .OrderByDescending(x => x.AverageRating)
            .ToList();

        // Daily trend (from start to end)
        var days = (int)(end.Date - start.Date).TotalDays + 1;
        if (days is > 0 and <= 60)
        {
            for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
            {
                var nextD = d.AddDays(1);
                var dayReservations = reservationsList.Where(r => r.ReservedDate >= d && r.ReservedDate < nextD).ToList();
                var total = dayReservations.Count(r => r.Status != ReservationStatus.Cart);
                var success = dayReservations.Count(r => r.Status == ReservationStatus.Success);
                var rev = dayReservations.Where(r => r.Status == ReservationStatus.Success).Sum(r => r.TotalCost);

                model.DailyTrends.Add(new DailyTrendItem(
                    d.ToString("dd/MM"),
                    total,
                    success,
                    rev));
            }
        }

        return model;
    }
}
