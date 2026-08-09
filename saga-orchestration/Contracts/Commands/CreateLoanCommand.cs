namespace Contracts.Commands;

public record CreateLoanCommand(Guid LoanId, Guid MemberId, Guid BookId);