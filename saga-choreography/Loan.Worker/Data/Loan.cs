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

    public Loan(Guid id, Guid memberId, Guid bookId, Status status, DateTime createdAt, string? canceledReason)
    {
        Id = id;
        MemberId = memberId;
        BookId = bookId;
        Status = status;
        CreatedAt = createdAt;
        CanceledReason = canceledReason;
    }
}
