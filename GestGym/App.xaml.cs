using System.Configuration;
using System.Data;
using System.Windows;

namespace GestGym
{
    
    /// Interaction logic for App.xaml
    
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ajouter un gestionnaire global d'exceptions non gérées
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Une erreur inattendue s'est produite: {e.Exception.Message}",
                "Erreur non gérée", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Empêche l'application de se fermer brutalement
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Une erreur critique s'est produite: {ex.Message}",
                    "Erreur critique", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Nettoyer les ressources si nécessaire
            base.OnExit(e);
        }
    }

}