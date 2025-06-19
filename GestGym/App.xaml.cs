using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace GestGym;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [Experimental("WPF0001")]
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Démarrer avec la fenêtre de login au lieu de MainWindow
        var loginWindow = new LoginWindow();
        MainWindow = loginWindow;
        loginWindow.Show();
    }
}