namespace MessageX.Tests;

public class MessageNotificationTests {
    [Fact]
    public void ConstructorFreezesProductFieldsAndCopiesFacts() {
        var facts = new List<MessageNotificationFact> { new("Service", "Monitoring") };
        var notification = new MessageNotification(
            MessageNotificationSeverity.Critical,
            "Monitor failed", "The probe timed out", facts, "inc-42",
            new Uri("https://example.test/reports/inc-42"),
            "Critical: Monitor failed. The probe timed out. Incident inc-42.");
        facts.Clear();

        Assert.Equal(MessageNotificationSeverity.Critical, notification.Severity);
        Assert.Equal("Monitor failed", notification.Title);
        Assert.Equal("The probe timed out", notification.Explanation);
        Assert.Equal("Monitoring", Assert.Single(notification.Facts).Value);
        Assert.Equal("inc-42", notification.IncidentId);
        Assert.Equal("https://example.test/reports/inc-42", notification.ReportUrl.AbsoluteUri);
        Assert.Contains("inc-42", notification.PlainTextFallback);
    }

    [Fact]
    public void ConstructorRejectsInsecureReportUrl() {
        Assert.Throws<ArgumentException>(() => new MessageNotification(
            MessageNotificationSeverity.Warning, "Alert", "Details", Array.Empty<MessageNotificationFact>(),
            "inc-1", new Uri("http://example.test/report"), "Alert details"));
    }

    [Fact]
    public void ConstructorRejectsMissingFallback() {
        Assert.Throws<ArgumentException>(() => new MessageNotification(
            MessageNotificationSeverity.Warning, "Alert", "Details", Array.Empty<MessageNotificationFact>(),
            "inc-1", new Uri("https://example.test/report"), ""));
    }
}
