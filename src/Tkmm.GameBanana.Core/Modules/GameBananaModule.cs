using Microsoft.Extensions.DependencyInjection;
using Tkmm.Abstractions.IO;
using Tkmm.GameBanana.Core.Parsers;

namespace Tkmm.GameBanana.Core.Modules;

public static class GameBananaModule
{
    public static IServiceCollection AddGamebananaFeatures(this IServiceCollection services)
    {
        services.AddSingleton<IModReader, GameBananaModReader>();
        return services;
    }
}