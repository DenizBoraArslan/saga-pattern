using Contracts.Commands;
using Loan.Worker.Data;
using Loan.Worker.Enums;
using MassTransit;

namespace Loan.Worker.Cosumers;

public class CreateCancelledLoanCommandConsumer : IConsumer<CreateCancelledLoanCommand>
{
    private readonly ILogger<CreateCancelledLoanCommandConsumer> _logger;
    private readonly LoanDbContext _loanDbContext;

    public CreateCancelledLoanCommandConsumer(ILogger<CreateCancelledLoanCommandConsumer> logger, LoanDbContext loanDbContext)
    {
        _logger = logger;
        _loanDbContext = loanDbContext;
    }

    public async Task Consume(ConsumeContext<CreateCancelledLoanCommand> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Creating cancelled loan record for LoanId: {LoanId}, Reason: {Reason}", msg.LoanId, msg.Reason);

        var loan = new Data.Loan
        {
            Id = msg.LoanId,
            MemberId = msg.MemberId,
            BookId = msg.BookId,
            Status = Status.Cancelled,
            CreatedAt = DateTime.UtcNow,
            CanceledReason = msg.Reason
        };

        await _loanDbContext.Loan.AddAsync(loan);
        await _loanDbContext.SaveChangesAsync();
    }
}