using FakeItEasy;
using FluentAssertions;
using Fabillio.Common.Helpers.Extensions;
using Fabillio.Common.Helpers.Implementations;

namespace Fabillio.Common.Helpers.Tests.Extensions;

public class SamvirkStandardDateTimeExtensionsTests
{
    private DateTimeProvider _dateTimeProvider;

    [SetUp]
    public void Setup()
    {
        _dateTimeProvider = A.Fake<DateTimeProvider>();
        DateTimeProvider.Current = _dateTimeProvider;
    }

    [TearDown]
    public void TearDown()
    {
        DateTimeProvider.ResetToDefault();
    }

    [Test]
    // members elder than from 1995-01-01 are not qualified
    [TestCase("1994-12-31", "2020-01-01", false)]
    [TestCase("1995-05-01", "2020-01-01", true)]
    [TestCase("1995-01-01", "2020-01-01", true)]
    // members from Jan 1st the year they turn 16 are qualified
    [TestCase("2004-03-01", "2020-01-01", true)]
    // members younger than 16 are not qualified
    [TestCase("2005-03-01", "2020-01-01", false)]
    public void IsBirthdateQualifiedForYoungMembership_DependingOnBirthdayAndToday_QualifiesMemberCorrectly(
        DateTime birthday,
        DateTime now,
        bool expected
    )
    {
        // Arrange
        SetupNow(now);

        // Act
        bool actual = birthday.IsBirthdateQualifiedForYoungMembership();

        // Assert
        actual.Should().Be(expected);
    }

    public void SetupNow(DateTime now)
    {
        A.CallTo(() => _dateTimeProvider.UtcNow).Returns(now);
    }
}
