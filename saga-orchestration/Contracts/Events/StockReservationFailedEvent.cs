namespace Contracts.Events;

public record StockReservationFailedEvent(Guid LoanId, Guid MemberId, Guid BookId, string Reason);