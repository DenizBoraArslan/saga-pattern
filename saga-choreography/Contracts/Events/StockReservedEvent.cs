namespace Contracts.Events;

public record StockReservedEvent(Guid LoanId, Guid BookId,Guid MemberId);

