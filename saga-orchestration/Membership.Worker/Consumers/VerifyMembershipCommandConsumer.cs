using Contracts.Commands;
using Contracts.Events;
using MassTransit;
using Membership.Worker.Data;

namespace Membership.Worker.Consumers;
public class VerifyMembershipCommandConsumer : IConsumer<VerifyMembershipCommand>
{
    private readonly ILogger<VerifyMembershipCommandConsumer> _logger;
    private readonly MembershipDbContext _dbContext;
    public VerifyMembershipCommandConsumer(ILogger<VerifyMembershipCommandConsumer> logger, MembershipDbContext membershipDbContext)
    {
        _logger = logger;
        _dbContext = membershipDbContext;
    }

    public async Task Consume(ConsumeContext<VerifyMembershipCommand> context)
    {
        var msg = context.Message;
        var member = _dbContext.Members.FirstOrDefault(m => m.Id == msg.MemberId);

        if (member is null)
        {
            _logger.LogWarning("Member with ID {MemberId} not found.", msg.MemberId);
            await context.Publish(new MembershipVerificationFailedEvent(msg.LoanId, msg.MemberId, msg.BookId, "Member Not Found"));
            return;
        }
        if (member.IsEligible && member.OverdueBooksCount == 0)
        {
            _logger.LogInformation("Member Is Eligible");
            await context.Publish(new MembershipVerifiedEvent(msg.LoanId, msg.MemberId, msg.BookId));
            return;
        }
        else
        {
            _logger.LogWarning("Member {MemberId} is not eligible or has overdue books.", msg.MemberId);
            await context.Publish(new MembershipVerificationFailedEvent(msg.LoanId, msg.MemberId, msg.BookId, "Member Not Eligible or Has Overdue Books"));
        }
    }
}

