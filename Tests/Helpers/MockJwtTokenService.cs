using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Services;
using Microsoft.Extensions.Options;

namespace GiftOfTheGivers.ReliefApi.Tests.Helpers;

public static class MockJwtTokenService
{
    public static JwtTokenService CreateService()
    {
        var jwtOptions = new JwtOptions
        {
            Key = "test-secret-key-for-unit-testing-12345678",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = 60
        };

        var options = Options.Create(jwtOptions);
        return new JwtTokenService(options);
    }
}


