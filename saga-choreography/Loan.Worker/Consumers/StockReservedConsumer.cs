using Contracts.Events;
using Loan.Worker.Data;
using Loan.Worker.Enums;
using MassTransit;

namespace Loan.Worker.Consumers;
public class StockReservedConsumer : IConsumer<StockReservedEvent>
{
    private readonly ILogger<StockReservedConsumer> _logger;
    private readonly LoanDbContext _loanDbContext;

    public StockReservedConsumer(ILogger<StockReservedConsumer> logger, LoanDbContext loanDbContext)
    {
        _logger = logger;
        _loanDbContext = loanDbContext;
    }

    public async Task Consume(ConsumeContext<StockReservedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Stock reserved for LoanId: {LoanId}, BookId: {BookId}", msg.LoanId, msg.BookId);

        var loan = new Data.Loan(msg.LoanId, msg.MemberId, msg.BookId, Status.Completed, DateTime.UtcNow, null);
        _loanDbContext.Add(loan);
        await _loanDbContext.SaveChangesAsync();

        _logger.LogInformation("Loan Completed :{LoanId}",msg.LoanId);
        await context.Publish(new LoanCompletedEvent(msg.LoanId));
    }
}

