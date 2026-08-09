namespace Contracts.Events;

public record MembershipVerificationFailedEvent(Guid LoanId, Guid BookId, Guid MemberId, string ReasonId);


