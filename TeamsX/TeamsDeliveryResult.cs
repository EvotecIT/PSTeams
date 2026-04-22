namespace TeamsX;

public sealed class TeamsDeliveryResult {
    public TeamsDeliveryMethod DeliveryMethod { get; set; }
    public Uri TargetUri { get; set; } = null!;
    public bool IsSuccessStatusCode { get; set; }
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }
}
