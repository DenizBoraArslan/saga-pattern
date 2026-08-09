using Catalog.Worker.Data;
using Contracts.Commands;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Worker.Consumers;
public class ReserveStockCommandConsumer : IConsumer<ReserveStockCommand>
{
    private readonly ILogger _logger;
    private readonly CatalogDbContext _dbContext;
    public ReserveStockCommandConsumer(ILogger<ReserveStockCommandConsumer> logger, CatalogDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    public async Task Consume(ConsumeContext<ReserveStockCommand> context)
    {
        var msg = context.Message;

        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == msg.BookId);

        if (book.StockQuantity > 0)
        {
            book.StockQuantity--;
            await _dbContext.SaveChangesAsync();
         
            _logger.LogInformation("Reserved stock for BookId: {BookId}, LoanId: {LoanId}, MemberId: {MemberId}", msg.BookId, msg.LoanId, msg.MemberId);
            await context.Publish(new StockReservedEvent(msg.LoanId,msg.MemberId,msg.BookId));
        }
        else
        {
            _logger.LogWarning("Failed to reserve stock for BookId: {BookId}, LoanId: {LoanId}, MemberId: {MemberId} - Out of stock", msg.BookId, msg.LoanId, msg.MemberId);
            await context.Publish(new StockReservationFailedEvent(msg.LoanId,msg.MemberId,msg.BookId, "Insufficient Stock"));
        }

    }
}

