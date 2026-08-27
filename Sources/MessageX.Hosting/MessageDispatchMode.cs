namespace MessageX.Hosting;

/// <summary>Selects whether a registered handler runs through deferred ingress or before provider acknowledgement.</summary>
public enum MessageDispatchMode {
    /// <summary>Accepts the event into the configured queue or durable store before acknowledgement.</summary>
    Deferred,
    /// <summary>Runs the handler inline so it can produce the initial acknowledgement or consume transient capabilities.</summary>
    Synchronous
}
