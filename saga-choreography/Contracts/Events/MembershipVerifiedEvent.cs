namespace Contracts.Events;

public record MembershipVerifiedEvent(Guid LoanId, Guid MemberId, Guid BookId);