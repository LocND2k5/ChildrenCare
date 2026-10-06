using CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;

namespace CRUD_ChildrenCare.Services.Contacts;

public interface IContactService
{
    Task<Contact> SyncContactFromReservationAsync(Reservation reservation, CancellationToken ct = default);
    Task<Contact> SyncContactFromFeedbackAsync(string fullName, Gender gender, string email, string mobile, CancellationToken ct = default);
    Task PromoteToCustomerIfSuccessAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<Contact> Contacts, int TotalItems, int TotalPages)> GetContactsAsync(string? search, ContactStatus? status, string? sort, int pageIndex, int pageSize, CancellationToken ct = default);
    Task<Contact?> GetContactByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CreateCustomerAsync(CreateCustomerViewModel model, int? createdByUserId, CancellationToken ct = default);
    Task<bool> UpdateCustomerAsync(EditCustomerViewModel model, int? updatedByUserId, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default);
}
