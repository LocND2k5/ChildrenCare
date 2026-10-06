using CRUD_ChildrenCare.Models;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;

public sealed class CustomerDetailsViewModel
{
    public Contact Customer { get; set; } = null!;
    public IReadOnlyList<ContactHistory> Histories { get; set; } = [];
    public IReadOnlyList<Reservation> Reservations { get; set; } = [];
    public bool CanManage { get; set; }
}
