using Microsoft.Extensions.DependencyInjection;
using Tkmm.Abstractions.IO;
using Tkmm.GameBanana.Core.Readers;

namespace Tkmm.GameBanana.Core.Extensions;

public static class GameBananaFeatureExtension
{
    public static IServiceCollection AddGamebananaFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IModReader, GameBananaModReader>();
        return services;
    }
}