using Password_manager.Shared;
using Password_manager.Templates;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Password_manager.Services;

namespace Password_manager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<SqliteConnectionFactory>();
            builder.Services.AddSingleton<RequestHandler>();
            builder.Services.AddSingleton<RestServiceHelper>();
            builder.Services.AddSingleton<RestService>();
            builder.Services.AddTransient<AddNewDataView>();
            return builder.Build();
        }
    }
}
