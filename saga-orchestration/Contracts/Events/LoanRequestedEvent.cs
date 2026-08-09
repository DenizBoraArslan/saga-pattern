namespace Contracts.Events;

public record LoanRequestedEvent(Guid LoanId, Guid MemberId, Guid BookId, DateTime RequestedAt);