using CRUD_ChildrenCare.Models.Enums;
using CRUD_ChildrenCare.Services.Feedbacks;
using CRUD_ChildrenCare.Tests.Integration;
using CRUD_ChildrenCare.ViewModels.Feedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRUD_ChildrenCare.Tests.Services;

public sealed class FeedbackServiceTests
{
    [Fact]
    public async Task CreateFeedbackAsync_CreatesFeedbackAndSyncsContact()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFeedbackService>();

        var model = new CreateFeedbackViewModel
        {
            FullName = "Nguyen Quoc Tuan",
            Gender = Gender.Male,
            Email = "tuannguyen@example.com",
            Mobile = "0944555666",
            RatedStar = 5,
            Content = "Dịch vụ tiêm chủng và bác sĩ tư vấn rất nhẹ nhàng, tận tình."
        };

        var feedback = await service.CreateFeedbackAsync(model);

        Assert.NotNull(feedback);
        Assert.Equal(5, feedback.RatedStar);
        Assert.Equal(FeedbackStatus.Published, feedback.Status);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var storedFeedback = await context.Feedbacks.SingleAsync(f => f.Email == "tuannguyen@example.com");
            Assert.Equal("Nguyen Quoc Tuan", storedFeedback.FullName);

            var contact = await context.Contacts.SingleAsync(c => c.Email == "tuannguyen@example.com");
            Assert.Equal(ContactStatus.Contact, contact.Status);
        });
    }

    [Fact]
    public async Task ToggleStatusAsync_ChangesStatusBetweenPublishedAndHidden()
    {
        await using var factory = new TestWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IFeedbackService>();

        var model = new CreateFeedbackViewModel
        {
            FullName = "Hoang Thu Thao",
            Gender = Gender.Female,
            Email = "thaothoang@example.com",
            Mobile = "0988777666",
            RatedStar = 4,
            Content = "Bác sĩ kiểm tra kỹ lưỡng, thời gian chờ hơi lâu một chút."
        };

        var feedback = await service.CreateFeedbackAsync(model);
        Assert.Equal(FeedbackStatus.Published, feedback.Status);

        await service.ToggleStatusAsync(feedback.Id);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var updated = await context.Feedbacks.SingleAsync(f => f.Id == feedback.Id);
            Assert.Equal(FeedbackStatus.Hidden, updated.Status);
        });

        await service.ToggleStatusAsync(feedback.Id);

        await factory.ExecuteDbContextAsync(async context =>
        {
            var updated = await context.Feedbacks.SingleAsync(f => f.Id == feedback.Id);
            Assert.Equal(FeedbackStatus.Published, updated.Status);
        });
    }
}
