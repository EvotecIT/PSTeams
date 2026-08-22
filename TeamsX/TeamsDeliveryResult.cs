namespace TeamsX;

public sealed class TeamsDeliveryResult {
    public TeamsDeliveryMethod DeliveryMethod { get; set; }
    public string Target { get; set; } = string.Empty;
    public bool IsSuccessStatusCode { get; set; }
    public int? StatusCode { get; set; }
    public string? ResponseBody { get; set; }
}
