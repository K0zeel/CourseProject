using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KPWrestlingScoreboard.Windows
{
    public partial class FlagWindow : Window
    {
        public FlagWindow()
        {
            InitializeComponent();
            LoadFlagImage();
        }

        private void LoadFlagImage()
        {
            try
            {
                // Пытаемся загрузить изображение из ресурсов
                var uri = new Uri("pack://application:,,,/Resources/flag.png", UriKind.Absolute);
                flagImage.Source = new BitmapImage(uri);
            }
            catch
            {
                // Если файл не найден, пытаемся загрузить из папки Resources рядом с exe
                try
                {
                    string exePath = AppDomain.CurrentDomain.BaseDirectory;
                    string flagPath = Path.Combine(exePath, "Resources", "flag.png");
                    
                    if (File.Exists(flagPath))
                    {
                        flagImage.Source = new BitmapImage(new Uri(flagPath, UriKind.Absolute));
                    }
                }
                catch
                {
                    // Изображение не загружено - окно будет без флага
                }
            }
        }
    }
}
