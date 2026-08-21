namespace BusinessLogic.DTOs.Messages.Response;

public class ProducerRs
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? OtherName { get; set; }

    public string? Description { get; set; }

    public DateOnly? EstablishDate { get; set; }

    public string? Country { get; set; }

    public string Status { get; set; } = null!;
}
