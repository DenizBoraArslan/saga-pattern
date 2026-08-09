using Contracts.Events;
using Loan.Worker.Data;
using Loan.Worker.Enums;
using MassTransit;

namespace Loan.Worker.Consumers;
public class StockReservationFailedConsumer : IConsumer<StockReservationFailedEvent>
{
    private readonly ILogger<StockReservationFailedConsumer> _logger;
    private readonly LoanDbContext _loanDbContext;
    public StockReservationFailedConsumer(ILogger<StockReservationFailedConsumer> logger, LoanDbContext loanDbContext)
    {
        _logger = logger;
        _loanDbContext = loanDbContext;
    }
    public async Task Consume(ConsumeContext<StockReservationFailedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Stock reservation failed for LoanId: {LoanId}, MemberId: {MemberId},BookId: {BookId} ,Reason: {Reason}", msg.LoanId, msg.MemberId, msg.BookId, msg.Reason);

        var loan = new Data.Loan(msg.LoanId, msg.MemberId, msg.BookId, Status.Cancelled, DateTime.UtcNow, msg.Reason);
        await _loanDbContext.AddAsync(loan);
        await _loanDbContext.SaveChangesAsync();

        await context.Publish(new LoanCanceledEvent(msg.LoanId, msg.Reason));
    }
}

