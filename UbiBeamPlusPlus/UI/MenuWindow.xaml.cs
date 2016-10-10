using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UbiBeamPlusPlus.Model;
using System.IO;

namespace UbiBeamPlusPlus.UI {
    /// <summary>
    /// Interaction logic for Menu_Window.xaml
    /// </summary>
    public partial class MenuWindow : Window {

        private String FileName = "location.txt";

        public MenuWindow() {
            InitializeComponent();

            try {
                lblPath.Content = System.IO.File.ReadAllText(@FileName);
            } catch (Exception) {
                Console.WriteLine("Error while reading from " + FileName);
            }

        }

        private void Button_ChooseCards(object sender, RoutedEventArgs e) {
            var FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                lblPath.Content = FolderDialog.SelectedPath;
                try {
                    System.IO.File.WriteAllText(FileName, FolderDialog.SelectedPath);
                } catch (Exception) {
                    Console.WriteLine("Error while writing to " + FileName);
                }
            }
        }

        private void Button_StartGame(object sender, RoutedEventArgs e) {
            String path = (String)lblPath.Content;

            rdbPrivate.IsChecked = rdbPrivate.IsChecked == null ? false : rdbPrivate.IsChecked;
            Game.GameMode mode = (bool)rdbPrivate.IsChecked ? Game.GameMode.Private : Game.GameMode.Public;

            float xFactor = 1.0F;
            float yFactor = 1.0F;
            if (!char.IsDigit(XFactor.Text, XFactor.Text.Length - 1)) {
                xFactor = float.Parse(XFactor.Text);
            }
            if (!char.IsDigit(YFactor.Text, YFactor.Text.Length - 1)) {
                yFactor = float.Parse(YFactor.Text);
            }

            bool testMode = cbxTestMode.IsChecked == null ? false : (bool)cbxTestMode.IsChecked;

            this.Hide();

            new MainWindow(mode, path, xFactor, yFactor, testMode).Show();

            this.Close();
        }
    }
}
