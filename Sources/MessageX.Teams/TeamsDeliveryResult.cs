namespace MessageX.Teams;

public sealed class TeamsDeliveryResult : MessageDeliveryResult {
    public TeamsDeliveryResult()
        : base(MessageProviders.Teams) {
    }

    public bool IsSuccessStatusCode {
        get => IsSuccess;
        set => IsSuccess = value;
    }

    public TeamsDeliveryMethod DeliveryMethod { get; set; }
    public string Target { get; set; } = string.Empty;
    public string? ResponseBody { get; set; }
}
