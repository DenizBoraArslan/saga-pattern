namespace Contracts.Events;

public record StockReservedEvent(Guid LoanId, Guid MemberId, Guid BookId);