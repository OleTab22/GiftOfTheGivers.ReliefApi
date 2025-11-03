using GiftOfTheGivers.ReliefApi.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ReliefDbContext CreateInMemoryContext(string databaseName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<ReliefDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ReliefDbContext(options);
    }
}


