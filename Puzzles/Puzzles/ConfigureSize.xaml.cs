using System;
using System.Windows;

namespace Puzzles
{
    /// <summary>
    /// Interaction logic for ConfigureSize.xaml
    /// </summary>
    public partial class ConfigureSize : Window
    {
        MainWindow window;
        public ConfigureSize(MainWindow window)
        {
            InitializeComponent();

            this.window = window;
        }

        public void ShowFile(string path)
        {
            file.Text = path;
        }

        public void ShowCatalog(string path)
        {
            catalog.Text = path;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            ChooseFile chooseFile = new ChooseFile(this, true);
            chooseFile.ShowDialog();
        }

        private void OpenCatalog_Click(object sender, RoutedEventArgs e)
        {
            ChooseFile chooseFile = new ChooseFile(this);
            chooseFile.ShowDialog();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (Int32.Parse(width.Text) < 10 || Int32.Parse(height.Text) < 10)
                    throw new Exception("Параметри нижче допустимого значення 10");
                if (catalog.Text == null)
                    throw new Exception("Ви не обрали папку для зберігання");
                if (file.Text == null)
                    throw new Exception("Ви не обрали файл");

                window.CutImage(Int32.Parse(width.Text), Int32.Parse(height.Text), file.Text, catalog.Text);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
    }
}