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

    private String osuFolder;
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
      if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\osu!\\Songs"))
      {
        osuFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\osu!\\Songs";
        DirText.Text = osuFolder;
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
        osuFolder = dialog.SelectedPath;
      }
      
    }
      private async void Download_Maps(object sender, RoutedEventArgs e)
      {
        dynamic response;
        
        var id = CollectorId.Text;
        try
        {
          response = await $"https://osucollector.com/api/collections/{id}"
            .GetJsonAsync();
        }
        catch (Exception exception)
        {
          MessageBox.Show("Error: " + exception.Message);
          return;
        }


        foreach (var map in response.beatmapsets)
        {
          await Task.Delay(50);
          ScrollView.ScrollToBottom();
          
          String url = $"https://beatconnect.io/b/{map.id}";
          String mapPath = $"{osuFolder}\\{map.id}.zip";
          String newPath = $"{osuFolder}\\{map.id}\\";

          if (!Directory.Exists(newPath))
          {
            Console.WriteLine($"{map.id} - Downloading map");
            Directory.CreateDirectory(newPath);
            
            
            await url.DownloadFileAsync(osuFolder, $"{map.id}.zip");
            Console.WriteLine($"{map.id} - Importing");
            ExtractToDirectory(mapPath, newPath);
            
            File.Delete(mapPath);
            continue;
          }
          else
          {
            Console.WriteLine($"{map.id} - Already downloaded");
            
          }

        }

      }
    }
  }
