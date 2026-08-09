namespace Contracts.Events;

public record MembershipVerificationFailedEvent(Guid LoanId, Guid MemberId, Guid BookId, string Reason);