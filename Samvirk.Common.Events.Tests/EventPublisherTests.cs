using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Raven.Client.Documents;
using Fabillio.Common.Events.Abstractions;
using Fabillio.Common.Events.Tests.TestModels;
using Fabillio.Common.Tests;

namespace Fabillio.Common.Events.Tests;

public class EventPublisherTests : RavenDbTests
{
    private DaprClient _daprClient;
    private ILogger<EventPublisher> _logger;

    [SetUp]
    public void Setup()
    {
        _daprClient = A.Fake<DaprClient>();
        A.CallTo(
                () =>
                    _daprClient.PublishEventAsync(
                        A<string>.Ignored,
                        "",
                        null,
                        CancellationToken.None
                    )
            )
            .Returns(Task.FromResult(0));
        _logger = A.Fake<ILogger<EventPublisher>>();
    }

    [Test]
    public async Task Publish_Always_PublishesDaprEvent()
    {
        // Arrange
        var testEvent = new TestEventWithoutAttribute();
        var documentStore = A.Fake<IDocumentStore>();
        var target = new EventPublisher(_daprClient, documentStore, _logger);

        // Act
        await target.PublishAsync(testEvent, CancellationToken.None);

        // Assert
        A.CallTo(
                () =>
                    _daprClient.PublishEventAsync(
                        EventPublisher.PubSub,
                        "test-event-without-attribute",
                        testEvent,
                        CancellationToken.None
                    )
            )
            .MustHaveHappened();
    }

    [Test]
    public async Task PublishWithOutbox_WhenDaprFails_SavesOutboxEvent()
    {
        // Arrange
        var testEvent = new TestEventWithoutAttribute();
        var documentStore = GetDocumentStore();
        var target = new EventPublisher(_daprClient, documentStore, _logger);
        A.CallTo(
                () =>
                    _daprClient.PublishEventAsync(
                        EventPublisher.PubSub,
                        "test-event-without-attribute",
                        testEvent,
                        CancellationToken.None
                    )
            )
            .Throws(() => new Exception("Some exception"));

        // Act
        await target.PublishWithOutbox(testEvent, CancellationToken.None);

        // Assert
        var documentSession = documentStore.OpenAsyncSession();
        var outboxEvents = await documentSession.Query<OutboxEvent>().ToListAsync();

        outboxEvents
            .Should()
            .HaveCount(1)
            .And.Contain(x => x.Published == null && x.EventId != Guid.Empty);
    }
}
