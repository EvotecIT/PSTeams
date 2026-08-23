namespace MessageX.Hosting.AspNetCore;

/// <summary>Exposes payload-free operational state for durable ingress.</summary>
public interface IMessageDurableIngressHealth {
    /// <summary>Returns one bounded snapshot without provider data or storage errors.</summary>
    MessageDurableIngressHealthSnapshot GetHealthSnapshot();
}
