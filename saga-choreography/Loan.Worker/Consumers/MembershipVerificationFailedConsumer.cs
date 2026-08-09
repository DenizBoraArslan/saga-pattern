using Contracts.Events;
using Loan.Worker.Data;
using Loan.Worker.Enums;
using MassTransit;

namespace Loan.Worker.Consumers;
public class MembershipVerificationFailedConsumer : IConsumer<MembershipVerificationFailedEvent>
{
    private readonly ILogger<MembershipVerificationFailedConsumer> _logger;
    private readonly LoanDbContext _loanDbContext;

    public MembershipVerificationFailedConsumer(ILogger<MembershipVerificationFailedConsumer> logger, LoanDbContext loanDbContext)
    {
        _logger = logger;
        _loanDbContext = loanDbContext;
    }
    public async Task Consume(ConsumeContext<MembershipVerificationFailedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Membership verification failed for LoanId: {LoanId}, MemberId: {MemberId}, ReasonId: {ReasonId}", msg.LoanId, msg.MemberId, msg.ReasonId);

        var loan = new Data.Loan(msg.LoanId, msg.MemberId, msg.BookId, Status.Cancelled, DateTime.UtcNow, msg.ReasonId);

        await _loanDbContext.AddAsync(loan);
        await _loanDbContext.SaveChangesAsync();
            
        await context.Publish(new LoanCanceledEvent(msg.LoanId, msg.ReasonId));
    }
}

