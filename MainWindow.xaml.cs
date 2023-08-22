using Microsoft.Win32;
using Nostalix_Servidor.Services;
using Nostalix_Servidor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;
using Result = System.Windows.Forms.DialogResult;

namespace Nostalix_Servidor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static string host = Utilidades.GetFullHost();
        private static bool isServidorParado = true;
        private static bool isCoping = false;
        private Task tarefa;

        public MainWindow()
        {
            InitializeComponent();
            ipInfo.Text = host;
           
        }

        private void IniciarServidor_Click(object sender, RoutedEventArgs e)
        {
            if(isServidorParado)
            {
                changeCorSituacao("Started");
                isServidorParado = false;
                try
                {
                    tarefa = Task.Factory.StartNew(() =>
                    {
                        Transferencia.SetLog((msg) =>
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                DisplayLog(msg, Log.Info);
                            });
                        });
                        Transferencia.IniciarServidor();

                    });
                    DisplayLog("o servidor foi iniciado.", Log.Info);
                }catch(Exception ex)
                {
                    DisplayLog("Erro ao conectar.", Log.Error);
                }
                
            }
            else
            {
                DisplayLog("o servidor já está em execução.", Log.Error);
            }

        }


        private void PararServidor_Click(object sender, RoutedEventArgs e)
        {
            if (!isServidorParado)
            {
                changeCorSituacao("Stoped");
                isServidorParado = true;
                try
                {
                    var currentExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(currentExecutablePath);
                    Application.Current.Shutdown();
                }
                catch(Exception ex)
                {
                    DisplayLog("o não será reiniciado.", Log.Error);
                }
                DisplayLog("o servidor foi finalizado.", Log.Info);
            }
            else
            {
                DisplayLog("o servidor já está parado.", Log.Error);
            }
        }


        private void changeCorSituacao(string key)
        {
            if (isServidorParado)
            {
                situacao.Text = "RODANDO";
            }
            else
            {
                situacao.Text = "PARADO";
            }
            // Encontrar o recurso
            LinearGradientBrush? brushColor = FindResource(key) as LinearGradientBrush;

            if (brushColor != null)
            {
                // Usar o recurso em algum lugar
                situacaoDisplay.Background = brushColor;
            }
            
        }
        enum Log
        {
           Error,
           Success,
           Info,
           Default
        }

        void DisplayLog(string msg, Log logType = Log.Default)
        {
            string logKey = "";
            switch (logType)
            {
                case Log.Error:
                    logKey = "logError";
                    break; 
                case Log.Success:
                    logKey = "logSuccess";
                    break;
                case Log.Info:
                    logKey = "logInfo";
                    break;
                default:
                    logKey = "logInfo";
                    break;

            }
            var item = new ListBoxItem();
            item.Content = $"[{DateTime.Now}] - {msg}";
            item.Foreground = FindResource(logKey) as SolidColorBrush;
            logLista.Items.Add(item);
        }

        [STAThread]
        private void Copiar_Click(object sender, RoutedEventArgs e)
        {
            if (!isCoping)
            {
                //https://social.msdn.microsoft.com/Forums/pt-BR/e9bb69d8-7df8-489f-87b1-afd9b5d7fbe9/o-thread-atual-deve-ser-definido-no-modo-sta-single-thread-apartment-antes-que-chamadas-ole-possam?forum=504
                Thread copy = new Thread(new ThreadStart(CopyHost));
                copy.SetApartmentState(ApartmentState.STA);
                copy.IsBackground = true;
                copy.Start();
                isCoping = true;
            }
            else
            {
                DisplayLog("copie apos 3 segundos.");
            }
        }

        [STAThread]
        private void CopyHost()
        {
            //https://stackoverflow.com/questions/4253088/updating-gui-wpf-using-a-different-thread
            Clipboard.SetText(host);
            Dispatcher.Invoke(() => {
                copiaInfo.Visibility = Visibility.Visible;
            });
            Thread.Sleep(3000);
            Dispatcher.Invoke(() => {
                copiaInfo.Visibility = Visibility.Collapsed;
            });

            isCoping = false;
        }

        private void SelecionarLocal_Click(object sender, RoutedEventArgs e)
        {
            /*
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Episódios (*.mp4)|*.mp4|Todos arquivos (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            
            if (openFileDialog.ShowDialog() == true)
            {

                MessageBox.Show(openFileDialog.FileName);
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            if (saveFileDialog.ShowDialog() == true)
            {
                MessageBox.Show(saveFileDialog.FileName);
            }
            */
            using var dialog = new FolderBrowserDialog();
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            Result result = dialog.ShowDialog();
            if (result == Result.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                MessageBox.Show($"Pasta selecionada: {dialog.SelectedPath}");
                Transferencia.pastaBiblioteca = dialog.SelectedPath + @"\";
                DisplayLog($"pasta selecionada: {Transferencia.pastaBiblioteca}", Log.Info);
            }

        }
    }
}
