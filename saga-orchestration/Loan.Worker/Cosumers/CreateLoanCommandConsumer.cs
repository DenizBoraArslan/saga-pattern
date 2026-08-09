using Contracts.Commands;
using Contracts.Events;
using Loan.Worker.Data;
using MassTransit;

namespace Loan.Worker.Cosumers;
public class CreateLoanCommandConsumer : IConsumer<CreateLoanCommand>
{
    private readonly ILogger<CreateLoanCommandConsumer> _logger;
    private readonly LoanDbContext _loanDbContext;
    public CreateLoanCommandConsumer(ILogger<CreateLoanCommandConsumer> logger, LoanDbContext loanDbContext)
    {
        _logger = logger;
        _loanDbContext = loanDbContext;
    }
    public async Task Consume(ConsumeContext<CreateLoanCommand> context)
    {
        var msg = context.Message;

        var loan = new Data.Loan()
        {
            Id = msg.LoanId,
            BookId = msg.BookId,
            MemberId = msg.MemberId,
            Status = Enums.Status.Completed,
            CreatedAt = DateTime.UtcNow
        };

        await _loanDbContext.Loan.AddAsync(loan);
        await _loanDbContext.SaveChangesAsync();

        await context.Publish(new LoanCreatedEvent(msg.LoanId));
    }
}
