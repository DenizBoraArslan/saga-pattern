using Catalog.Worker.Data;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Worker.Consumers;
public class MembershipVerifiedConsumer : IConsumer<MembershipVerifiedEvent>
{
    private readonly ILogger<MembershipVerifiedEvent> _logger;
    private readonly CatalogDbContext _catalogDbContext;


    public MembershipVerifiedConsumer(ILogger<MembershipVerifiedEvent> logger, CatalogDbContext catalogDbContext)
    {
        _logger = logger;
        _catalogDbContext = catalogDbContext;
    }
    public async Task Consume(ConsumeContext<MembershipVerifiedEvent> context)
    {
        var message = context.Message;

        var book = await _catalogDbContext.Books.FirstOrDefaultAsync(b => b.Id == message.BookId);

        if (book is null)
        {
            _logger.LogWarning("Book not found : {BookId}", message.BookId);
            await context.Publish(new StockReservationFailedEvent(message.LoanId, message.MemberId, message.BookId, "Book not found"));
            return;
        }
        if (book.StockQuantity > 0)
        {
            book.StockQuantity -= 1;
            await _catalogDbContext.SaveChangesAsync();

            _logger.LogInformation("Stock reserved for BookId: {BookId}, LoanId: {LoanId}", message.BookId, message.LoanId);
            await context.Publish(new StockReservedEvent(message.LoanId, message.BookId, message.MemberId));
        }
        else
        {
            _logger.LogWarning("Stock reservation failed for BookId: {BookId}, LoanId: {LoanId} due to insufficient stock", message.BookId, message.LoanId);
            await context.Publish(new StockReservationFailedEvent(message.LoanId, message.MemberId, message.BookId, "Insufficient stock"));
        }
    }
}

