using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Flurl.Http;
using Ookii.Dialogs.Wpf;
using static System.IO.Compression.ZipFile;
using MessageBox = System.Windows.MessageBox;

namespace osucollector_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private String _cursor = "0";
        private String _osuFolder;

        public MainWindow()
        {
            InitializeComponent();

            var outputter = new TextBoxOutputter(LogsText);
            Console.SetOut(outputter);
            if (Process.GetProcessesByName("osu!").Length > 0)
            {
                MessageBox.Show("osu! is running, stopping proccess");
                Process.GetProcessesByName("osu!")[0].Kill();
            }

            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                 "\\osu!\\Songs"))
            {
                _osuFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                             "\\osu!\\Songs";
                DirText.Text = _osuFolder;
            }
        }

        private void Click_Path(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Choose osu Songs folder";
            dialog.UseDescriptionForTitle = true;
            dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            dialog.ShowDialog();
            if (dialog.SelectedPath != null)
            {
                DirText.Text = dialog.SelectedPath;
                _osuFolder = dialog.SelectedPath;
            }
        }
        private String replace(String s)
        {
            return s
                .Replace(":", "_")
                .Replace("?", "")
                .Replace("/", "")
                .Replace("\"", "")
                .Replace(".", "");
        }
        private async void Download_Maps(object sender, RoutedEventArgs e)
        {
            var id = CollectorId.Text;
            var response = await GetBeatmap(id);
            int count = 0;

            if (response != null)
            {
                foreach (var map in response.beatmaps)
                {
                    await Task.Delay(50);

                    count++;
                    if (count > 50)
                    {
                        count = 0;
                        Console.WriteLine($@"Requesting another cursor");
                        response = await GetBeatmap(id);
                        continue;
                    }


                    ScrollView.ScrollToBottom();

                    String url = $"https://beatconnect.io/b/{map.beatmapset.id}";
                    String mapPath = $"{_osuFolder}\\{map.beatmapset.id}.zip";


                    String newPath = Path.Combine(_osuFolder,
                        $"{map.beatmapset.id} {replace(map.beatmapset.artist_unicode)} - {replace(map.beatmapset.title_unicode)}");

                    if (!Directory.Exists(newPath))
                    {
                        Console.WriteLine($@"{map.beatmapset.title_unicode} - Downloading map");
                        Directory.CreateDirectory(newPath);


                        await url.DownloadFileAsync(_osuFolder, $"{map.beatmapset.id}.zip");
                        Console.WriteLine($@"{map.beatmapset.title_unicode} - Importing");
                        ExtractToDirectory(mapPath, newPath);

                        File.Delete(mapPath);
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($@"{map.beatmapset.title_unicode} - Already downloaded");
                    }
                }
            }
        }

        private async Task<dynamic> GetBeatmap(String id)
        {
            dynamic response;
            try
            {
                response = await $"https://osucollector.com/api/collections/{id}/beatmapsv2?cursor={_cursor}"
                    .GetJsonAsync();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error: " + exception.Message);
                return null;
            }

            return response;
        }
    }
}