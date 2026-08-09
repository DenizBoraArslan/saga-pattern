namespace Membership.Worker.Data;

public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsEligible { get; set; }
    public int OverdueBooksCount { get; set; }
}