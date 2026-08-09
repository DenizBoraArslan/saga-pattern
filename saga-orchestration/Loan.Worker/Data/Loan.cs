using Loan.Worker.Enums;

namespace Loan.Worker.Data;
public class Loan
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public Guid BookId { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public String? CanceledReason { get; set; }
}

