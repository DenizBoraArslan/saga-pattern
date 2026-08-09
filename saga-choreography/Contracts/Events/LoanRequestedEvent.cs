namespace Contracts.Events;
public record LoanRequestEvent(Guid LoanId, Guid MemberId, Guid BookId, DateTime RequestedAt);

