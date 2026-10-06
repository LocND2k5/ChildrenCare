using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;

namespace CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;

public sealed class CustomerListViewModel
{
    public IReadOnlyList<Contact> Customers { get; set; } = [];
    public string? SearchQuery { get; set; }
    public ContactStatus? Status { get; set; }
    public string? Sort { get; set; }
    public int PageIndex { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
    public bool CanManage { get; set; }
}
