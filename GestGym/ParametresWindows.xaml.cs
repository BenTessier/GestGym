
using System.Windows;

#pragma warning disable WPF0001

namespace GestGym
{
    public partial class ParametresWindows : Window
    {
        private readonly Controller _controller;

        public ParametresWindows(Controller controller)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            InitializeComponent();

            // Charger les paramètres SQL actuels
            LoadSQLSettings();
        }

        private void LoadSQLSettings()
        {
            SQLServerNameTextBox.Text = Settings.Default.SQLServerName;
            SQLDBNameTextBox.Text = Settings.Default.SQLDBName;
            SQLAccountTextBox.Text = Settings.Default.SQLAccount;
            SQLPasswordBox.Password = Settings.Default.SQLPassword;
        }

        private void SystemThemeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.ThemeMode = ThemeMode.System;
            LightThemeRadio.IsChecked = false;
            DarkThemeRadio.IsChecked = false;
            SystemThemeRadio.IsChecked = true;
            _controller.SaveSettings();
        }

        private void LightThemeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.ThemeMode = ThemeMode.Light;
            LightThemeRadio.IsChecked = true;
            DarkThemeRadio.IsChecked = false;
            SystemThemeRadio.IsChecked = false;
            _controller.SaveSettings();
        }

        private void DarkThemeRadioButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.ThemeMode = ThemeMode.Dark;
            LightThemeRadio.IsChecked = false;
            DarkThemeRadio.IsChecked = true;
            SystemThemeRadio.IsChecked = false;
            _controller.SaveSettings();
        }

        private void SaveSQLSettings_Click(object sender, RoutedEventArgs e)
        {
            // Valider que les champs ne sont pas vides
            if (string.IsNullOrWhiteSpace(SQLServerNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SQLDBNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SQLAccountTextBox.Text) ||
                string.IsNullOrWhiteSpace(SQLPasswordBox.Password))
            {
                MessageBox.Show("Tous les champs de connexion à la base de données sont obligatoires.",
                                "Validation des paramètres", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sauvegarder les paramètres
            Settings.Default.SQLServerName = SQLServerNameTextBox.Text;
            Settings.Default.SQLDBName = SQLDBNameTextBox.Text;
            Settings.Default.SQLAccount = SQLAccountTextBox.Text;
            Settings.Default.SQLPassword = SQLPasswordBox.Password;
            Settings.Default.Save();

            MessageBox.Show("Les paramètres de connexion ont été enregistrés. Les changements prendront effet au prochain redémarrage de l'application.",
                          "Paramètres enregistrés", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TestSQLConnection_Click(object sender, RoutedEventArgs e)
        {
            // Créer une chaîne de connexion temporaire pour le test
            string connectionString = $"Data Source={SQLServerNameTextBox.Text};Initial Catalog={SQLDBNameTextBox.Text};User ID={SQLAccountTextBox.Text};Password={SQLPasswordBox.Password};TrustServerCertificate=True;";

            // Tester la connexion
            if (_controller.TestDatabaseConnection(connectionString))
            {
                MessageBox.Show("La connexion à la base de données a réussi.", "Test de connexion", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
