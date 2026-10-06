namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Dashboard;

public sealed class DashboardViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool IsManagerView { get; set; }

    // Reservation KPI
    public int TotalReservations { get; set; }
    public int SuccessReservations { get; set; }
    public int SubmittedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int CartReservations { get; set; }

    // Revenue KPI
    public decimal TotalRevenue { get; set; }
    public List<CategoryRevenueItem> RevenueByCategory { get; set; } = [];

    // Customers & Users KPI
    public int NewUsersCount { get; set; }
    public int TotalContacts { get; set; }
    public int ContactCount { get; set; }
    public int PotentialCount { get; set; }
    public int CustomerCount { get; set; }

    // Feedback KPI
    public double AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
    public List<ServiceRatingItem> RatingByService { get; set; } = [];

    // Daily Trend
    public List<DailyTrendItem> DailyTrends { get; set; } = [];
}

public sealed record CategoryRevenueItem(string CategoryName, decimal Revenue, int ReservationCount);
public sealed record ServiceRatingItem(string ServiceName, double AverageRating, int FeedbackCount);
public sealed record DailyTrendItem(string Date, int TotalCount, int SuccessCount, decimal Revenue);
