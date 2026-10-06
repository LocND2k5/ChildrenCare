using CRUD_ChildrenCare.Areas.Admin.ViewModels.Customers;
using CRUD_ChildrenCare.Data;
using CRUD_ChildrenCare.Models;
using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Validation;
using Microsoft.EntityFrameworkCore;

namespace CRUD_ChildrenCare.Services.Contacts;

public sealed class ContactService(ApplicationDbContext dbContext) : IContactService
{
    public async Task<Contact> SyncContactFromReservationAsync(Reservation reservation, CancellationToken ct = default)
    {
        var email = reservation.ReceiverEmail.Trim();
        var contact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower(), ct);

        var targetStatus = reservation.Status == ReservationStatus.Success ? ContactStatus.Customer : ContactStatus.Potential;

        if (contact == null)
        {
            contact = new Contact
            {
                FullName = AccountValidation.NormalizeFullName(reservation.ReceiverFullName),
                Gender = reservation.ReceiverGender,
                Email = email,
                Mobile = reservation.ReceiverMobile,
                Address = reservation.ReceiverAddress,
                Status = targetStatus
            };
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync(ct);

            dbContext.ContactHistories.Add(new ContactHistory
            {
                ContactId = contact.Id,
                FullName = contact.FullName,
                Gender = contact.Gender,
                Email = contact.Email,
                Mobile = contact.Mobile,
                Address = contact.Address,
                UpdatedDate = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(ct);
        }
        else
        {
            bool changed = contact.FullName != reservation.ReceiverFullName
                || contact.Gender != reservation.ReceiverGender
                || contact.Mobile != reservation.ReceiverMobile
                || contact.Address != reservation.ReceiverAddress;

            contact.FullName = AccountValidation.NormalizeFullName(reservation.ReceiverFullName);
            contact.Gender = reservation.ReceiverGender;
            contact.Mobile = reservation.ReceiverMobile;
            if (!string.IsNullOrWhiteSpace(reservation.ReceiverAddress))
            {
                contact.Address = reservation.ReceiverAddress;
            }

            if (reservation.Status == ReservationStatus.Success)
            {
                contact.Status = ContactStatus.Customer;
            }
            else if (contact.Status == ContactStatus.Contact)
            {
                contact.Status = ContactStatus.Potential;
            }

            if (changed)
            {
                dbContext.ContactHistories.Add(new ContactHistory
                {
                    ContactId = contact.Id,
                    FullName = contact.FullName,
                    Gender = contact.Gender,
                    Email = contact.Email,
                    Mobile = contact.Mobile,
                    Address = contact.Address,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync(ct);
        }

        return contact;
    }

    public async Task<Contact> SyncContactFromFeedbackAsync(string fullName, Gender gender, string email, string mobile, CancellationToken ct = default)
    {
        var cleanEmail = email.Trim();
        var contact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Email.ToLower() == cleanEmail.ToLower(), ct);

        if (contact == null)
        {
            contact = new Contact
            {
                FullName = AccountValidation.NormalizeFullName(fullName),
                Gender = gender,
                Email = cleanEmail,
                Mobile = mobile,
                Status = ContactStatus.Contact
            };
            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync(ct);

            dbContext.ContactHistories.Add(new ContactHistory
            {
                ContactId = contact.Id,
                FullName = contact.FullName,
                Gender = contact.Gender,
                Email = contact.Email,
                Mobile = contact.Mobile,
                Address = contact.Address,
                UpdatedDate = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(ct);
        }
        else
        {
            bool changed = contact.FullName != fullName || contact.Gender != gender || contact.Mobile != mobile;
            contact.FullName = AccountValidation.NormalizeFullName(fullName);
            contact.Gender = gender;
            contact.Mobile = mobile;

            if (changed)
            {
                dbContext.ContactHistories.Add(new ContactHistory
                {
                    ContactId = contact.Id,
                    FullName = contact.FullName,
                    Gender = contact.Gender,
                    Email = contact.Email,
                    Mobile = contact.Mobile,
                    Address = contact.Address,
                    UpdatedDate = DateTime.UtcNow
                });
            }

            await dbContext.SaveChangesAsync(ct);
        }

        return contact;
    }

    public async Task PromoteToCustomerIfSuccessAsync(string email, CancellationToken ct = default)
    {
        var cleanEmail = email.Trim().ToLower();
        var contact = await dbContext.Contacts.FirstOrDefaultAsync(c => c.Email.ToLower() == cleanEmail, ct);
        if (contact != null && contact.Status != ContactStatus.Customer)
        {
            contact.Status = ContactStatus.Customer;
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task<(IReadOnlyList<Contact> Contacts, int TotalItems, int TotalPages)> GetContactsAsync(
        string? search, ContactStatus? status, string? sort, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Contacts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(c => c.FullName.Contains(s) || c.Email.Contains(s) || c.Mobile.Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        query = sort switch
        {
            "FullName" => query.OrderBy(c => c.FullName),
            "FullNameDesc" => query.OrderByDescending(c => c.FullName),
            "Email" => query.OrderBy(c => c.Email),
            "EmailDesc" => query.OrderByDescending(c => c.Email),
            "Mobile" => query.OrderBy(c => c.Mobile),
            "MobileDesc" => query.OrderByDescending(c => c.Mobile),
            "Status" => query.OrderBy(c => c.Status),
            "StatusDesc" => query.OrderByDescending(c => c.Status),
            _ => query.OrderByDescending(c => c.CreatedDate)
        };

        var totalItems = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (pageIndex < 1) pageIndex = 1;
        if (pageIndex > totalPages && totalPages > 0) pageIndex = totalPages;

        var contacts = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (contacts, totalItems, totalPages);
    }

    public async Task<Contact?> GetContactByIdAsync(int id, CancellationToken ct = default)
    {
        return await dbContext.Contacts
            .Include(c => c.Histories.OrderByDescending(h => h.UpdatedDate))
                .ThenInclude(h => h.UpdatedBy)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<bool> CreateCustomerAsync(CreateCustomerViewModel model, int? createdByUserId, CancellationToken ct = default)
    {
        if (await EmailExistsAsync(model.Email, null, ct))
        {
            return false;
        }

        var contact = new Contact
        {
            FullName = AccountValidation.NormalizeFullName(model.FullName),
            Gender = model.Gender,
            Email = model.Email.Trim(),
            Mobile = model.Mobile.Trim(),
            Address = model.Address?.Trim(),
            Status = ContactStatus.Contact
        };

        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync(ct);

        dbContext.ContactHistories.Add(new ContactHistory
        {
            ContactId = contact.Id,
            FullName = contact.FullName,
            Gender = contact.Gender,
            Email = contact.Email,
            Mobile = contact.Mobile,
            Address = contact.Address,
            UpdatedById = createdByUserId,
            UpdatedDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateCustomerAsync(EditCustomerViewModel model, int? updatedByUserId, CancellationToken ct = default)
    {
        var contact = await dbContext.Contacts.FindAsync([model.Id], ct);
        if (contact == null)
        {
            return false;
        }

        contact.FullName = AccountValidation.NormalizeFullName(model.FullName);
        contact.Gender = model.Gender;
        contact.Mobile = model.Mobile.Trim();
        contact.Address = model.Address?.Trim();

        dbContext.ContactHistories.Add(new ContactHistory
        {
            ContactId = contact.Id,
            FullName = contact.FullName,
            Gender = contact.Gender,
            Email = contact.Email,
            Mobile = contact.Mobile,
            Address = contact.Address,
            UpdatedById = updatedByUserId,
            UpdatedDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await dbContext.Contacts
            .AnyAsync(c => c.Email.ToLower() == normalized && (!excludeId.HasValue || c.Id != excludeId.Value), ct);
    }
}
