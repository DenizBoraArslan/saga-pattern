using Contracts.Events;
using MassTransit;
using Membership.Worker.Data;
using Microsoft.EntityFrameworkCore;

namespace Membership.Worker.Consumers
{
    public class LoanRequestedConsumer : IConsumer<LoanRequestEvent>
    {
        private readonly ILogger<LoanRequestedConsumer> _logger;
        private readonly MembershipDbContext _membershipDbContext;

        public LoanRequestedConsumer(ILogger<LoanRequestedConsumer> logger, MembershipDbContext membershipDbContext)
        {
            _logger = logger;
            _membershipDbContext = membershipDbContext;
        }

        public async Task Consume(ConsumeContext<LoanRequestEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Checking membership for MemberId: {MemberId}, LoanId: {LoanId}", msg.MemberId, msg.LoanId);

            var member = await _membershipDbContext.Members.FirstOrDefaultAsync(m => m.Id == msg.MemberId);

            if (member is null)
            {
                _logger.LogWarning("Member not found: {MemberId}", msg.MemberId);
                await context.Publish(new MembershipVerificationFailedEvent(msg.LoanId, msg.BookId,msg.MemberId, "Member not found"));
                return;
            }

            if (member.IsEligible && member.OverdueBooksCount == 0)
            {
                _logger.LogInformation("Member {MemberId} is eligible", msg.MemberId);
                await context.Publish(new MembershipVerifiedEvent(msg.LoanId, msg.MemberId,msg.BookId));
            }
            else
            {
                _logger.LogWarning("Member {MemberId} is NOT eligible", msg.MemberId);
                await context.Publish(new MembershipVerificationFailedEvent(msg.LoanId, msg.BookId,msg.MemberId, "Member has overdue books"));
            }

        }
    }
}
    