using Nostalix_Servidor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nostalix_Servidor.Services
{
    internal class Transferencia
    {
        static IPEndPoint ipEndServidor;
        static Socket socketServidor;
        public static string enderecoIp = Utilidades.GetLocalIPv4();
        public static int port = Utilidades.GetLocalPort();
        public static string pastaBiblioteca = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + @"\";
        public static Action<string>? onLog = null;
        public static void SetLog(Action<string> onLogging)
        {
            onLog = onLogging;
        }
        public static void IniciarServidor()
        {   
            try
            {
                ipEndServidor = new IPEndPoint(IPAddress.Parse(enderecoIp), port);
                socketServidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                socketServidor.Bind(ipEndServidor);
            }catch(Exception ex)
            {
                onLog?.Invoke("Não foi possivel iniciar o servidor.");
            }

            try
            {
                socketServidor.Listen(100);//100 conexões
                onLog?.Invoke("Servidor conectado e recebendo requisições.");

                Socket clienteSock = socketServidor.Accept();

                clienteSock.ReceiveBufferSize = 16384; //16kb
                //pega informaç~coes
                byte[] dados = new byte[1024 * 50000]; //50mb
                int tamanhoBytesRecebidos = clienteSock.Receive(dados, dados.Length, 0);
                int tamanhoNomeArquivo = BitConverter.ToInt32(dados, 0);
                string nomeArquivo = Encoding.UTF8.GetString(dados, 4, tamanhoNomeArquivo);

                //gravar os dados
                BinaryWriter bWriter = new BinaryWriter(File.Open(pastaBiblioteca + nomeArquivo, FileMode.Append));
                bWriter.Write(dados, 4 + tamanhoNomeArquivo, tamanhoBytesRecebidos - 4 - tamanhoNomeArquivo);
                while(tamanhoBytesRecebidos > 0)
                {
                    tamanhoBytesRecebidos = clienteSock.Receive(dados, dados.Length, 0);
                    if(tamanhoBytesRecebidos == 0)
                    {
                        //fecha se já recebeu tudo.
                        bWriter.Close();
                    }
                    else
                    {
                        bWriter.Write(dados, 0, tamanhoBytesRecebidos);
                    }

                    onLog?.Invoke($"Arquivo recebido [{nomeArquivo}]");
                    bWriter.Close();
                    clienteSock.Close();

                }
            }catch(SocketException ex) {
                onLog?.Invoke("Erro ao receber arquivo.");
            }
            finally
            {
                socketServidor.Close();
                socketServidor.Dispose();
                IniciarServidor();
            }
        }
    }
}
