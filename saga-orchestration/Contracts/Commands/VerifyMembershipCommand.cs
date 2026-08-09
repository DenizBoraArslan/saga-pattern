namespace Contracts.Commands;

public record VerifyMembershipCommand(Guid LoanId, Guid MemberId, Guid BookId);