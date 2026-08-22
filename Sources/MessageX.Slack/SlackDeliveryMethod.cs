namespace MessageX.Slack;

/// <summary>Supported Slack notification transports.</summary>
public enum SlackDeliveryMethod {
    /// <summary>A fixed-destination Slack incoming webhook.</summary>
    IncomingWebhook,
    /// <summary>An authenticated Slack Web API call.</summary>
    WebApi
}
