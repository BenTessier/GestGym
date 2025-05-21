using System.Windows;
using System.Windows.Threading;

namespace GestGym;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Démarrer avec la fenêtre de login au lieu de MainWindow
        var loginWindow = new LoginWindow();
        MainWindow = loginWindow;
        loginWindow.Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Une erreur inattendue s'est produite: {e.Exception.Message}",
            "Erreur non gérée", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Empêche l'application de se fermer brutalement
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            MessageBox.Show($"Une erreur critique s'est produite: {ex.Message}",
                "Erreur critique", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Nettoyer les ressources si nécessaire
        base.OnExit(e);
    }
}