using FluentAssertions;
using NUnit.Framework;
using Fabillio.Common.Events.Tests.TestModels;

namespace Fabillio.Common.Events.Tests;

public class EventExtensionsTests
{
    [Test]
    public void GetEventTopic_AttributeSpecifiedWithTopic_ReturnsTopicSpecified()
    {
        // Arrange
        var testEvent = new TestEventWithTopic();

        // Act
        var actual = testEvent.GetEventTopic();

        // Assert
        actual.Should().BeEquivalentTo("test-topic");
    }

    [Test]
    public void GetEventTopic_AttributeSpecifiedWithoutTopic_GeneratesTopicFromClassName()
    {
        // Arrange
        var testEvent = new TestEventWithoutTopic();

        // Act
        var actual = testEvent.GetEventTopic();

        // Assert
        actual.Should().BeEquivalentTo("test-event-without-topic");
    }

    [Test]
    public void GetEventTopic_AttributeNotSpecified_GeneratesTopicFromClassName()
    {
        // Arrange
        var testEvent = new TestEventWithoutAttribute();

        // Act
        var actual = testEvent.GetEventTopic();

        // Assert
        actual.Should().BeEquivalentTo("test-event-without-attribute");
    }
}
