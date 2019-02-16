using System;
using Plataforma.Console.Sftp;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Sftp sftp = new Sftp();
            sftp.host = "12.3.456.7";
            sftp.keyFilelocation = @"C:\SFTP\arquivo Chave";
            sftp.DownloadAll( @"/Arquivos/Files", @"/ArquivosLidos/Files", @"C:\SFTP\download\");
            sftp.uploadFile(@"C:\SFTP\upload\arquivo.txt", @"/Arquivos/UploadFiles/");
        }
    }
}
