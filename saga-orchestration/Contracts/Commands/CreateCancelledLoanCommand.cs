namespace Contracts.Commands;

public record CreateCancelledLoanCommand(Guid LoanId, Guid MemberId, Guid BookId, string Reason);