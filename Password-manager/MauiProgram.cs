using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Password_manager.Services;
using Password_manager.Shared;
using Password_manager.Templates;
using Password_manager.ViewModels;
using Syncfusion.Maui.Core.Hosting;

namespace Password_manager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Config.SyncfusionKey);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionCore()
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
            builder.Services.AddHttpClient<RestService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/");
            });
            builder.Services.AddTransient<AddNewDataView>();
            builder.Services.AddTransient<PasswordVaultView>();
            builder.Services.AddTransient<NoteVaultView>();
            builder.Services.AddTransient<PopupRegisterView>();
            builder.Services.AddTransient<PopupLoginView>();
            builder.Services.AddTransient<PopupAddNoteView>();
            builder.Services.AddTransient<PopupEditNoteView>();
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<LoginPageViewModel>();
            builder.Services.AddTransient<RegisterPageViewModel>();

            return builder.Build();
        }
    }
}