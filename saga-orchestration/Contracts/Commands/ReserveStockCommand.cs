namespace Contracts.Commands;

public record ReserveStockCommand(Guid LoanId, Guid MemberId, Guid BookId);