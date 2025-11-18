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
            builder.Services.AddTransient<RequestHandler>();
            builder.Services.AddTransient<RestServiceHelper>();
            builder.Services.AddTransient<RestService>();
            builder.Services.AddTransient<AddNewDataView>();
            builder.Services.AddTransient<PasswordVaultView>();
            builder.Services.AddTransient<NoteVaultView>();
            builder.Services.AddTransient<PopupRegisterView>();
            builder.Services.AddTransient<PopupLoginView>();
            return builder.Build();
        }
    }
}
