using System;
using System.Threading.Tasks;
using FluentAssertions;
using IntegrationTesting.Data.Dapper;
using IntegrationTesting.Data.DTOs;
using Xunit;

namespace IntegrationTesting.Data.Tests.Dapper;

public class DapperBusinessEventRepositoryShould : IClassFixture<DapperDatabaseContainer>
{
    private readonly DapperDatabaseContainer _dapperDatabase;

    public DapperBusinessEventRepositoryShould(DapperDatabaseContainer dapperDatabase) => _dapperDatabase = dapperDatabase;

    [Fact]
    public async Task PersistAndGetsRelatedBusinessEvents()
    {
        var repository = new DapperBusinessEventRepository(_dapperDatabase.DomainLoginConnectionString);
        var businessEntityId = Guid.NewGuid();
        var firstBusinessEvent = new BusinessEvent(
            Guid.NewGuid(),
            businessEntityId,
            "TestEvent1",
            "{ \"Key\": \"Value\" }",
            new DateTime(1997, 07, 23, 12, 13, 14));
        var secondBusinessEvent = new BusinessEvent(
            Guid.NewGuid(),
            businessEntityId,
            "TestEvent2",
            "{ \"Key\": \"Value\" }",
            new DateTime(1997, 07, 23, 12, 13, 15));

        await repository.PersistBusinessEvent(firstBusinessEvent);
        await repository.PersistBusinessEvent(secondBusinessEvent);
        var retreivedEvents = await repository.GetBusinessEventsByBusinessEntityId(businessEntityId);

        retreivedEvents.Should().HaveCount(2)
            .And.ContainEquivalentOf(firstBusinessEvent)
            .And.ContainEquivalentOf(secondBusinessEvent);
    }

    [Fact]
    public async Task PersistAndNotGetUnrelatedBusinessEvents()
    {
        var repository = new DapperBusinessEventRepository(_dapperDatabase.DomainLoginConnectionString);
        var businessEntityId = Guid.NewGuid();
        var relatedBueinssEvent = new BusinessEvent(
            Guid.NewGuid(),
            businessEntityId,
            "TestEventRelated",
            "{ \"Key\": \"Value\" }",
            new DateTime(1997, 07, 23, 12, 13, 14));
        var unrelatedBueinssEvent = new BusinessEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TestEventUnrelated",
            "{ \"Key\": \"Value\" }",
            new DateTime(1997, 07, 23, 12, 13, 15));

        await repository.PersistBusinessEvent(relatedBueinssEvent);
        await repository.PersistBusinessEvent(unrelatedBueinssEvent);
        var retreivedEvents = await repository.GetBusinessEventsByBusinessEntityId(businessEntityId);

        retreivedEvents.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(relatedBueinssEvent);
    }
}
