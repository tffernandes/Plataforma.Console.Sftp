using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;



namespace Plataforma.Console.Sftp
{
    public class Sftp
    {

        /// <summary>Local do arquivo de chave de conexão no padrão OpenSSH</summary>
        public string keyFilelocation;

        /// <summary>
        /// Usuário de conexão
        /// </summary>
        public string username = "User123";
        /// <summary>
        /// Ip do SFTP
        /// </summary>
        public string host = "12.3.456.7";
        /// <summary>
        /// Caso a autenticação seja por arquivo de chave deixar em branco
        /// </summary>
        public string psw = "password"; 

        /// <param name="localPath">Diretorio no computador local para receber os arquivos</param>
        /// <param name="hostDirectory">Diretorio no computador(SFTP) remoto onde estão os arquivos</param>
        /// <param name="readFileDestination">Direotrio no computador(SFTP) onde serão movidos arquivos recebidos</param>
        /// <summary>Realiza o download de todos os arquivos da pasta indicada.</summary>
        public void DownloadAll(string hostDirectory, string readFileDestination, string localPath = @"C:\Arquivos_SFTP\")
        {
            if (!readFileDestination.EndsWith(@"/")) readFileDestination = readFileDestination + @"/"; //Adiciona barra ao final do caminho
            if (!localPath.EndsWith(@"\")) localPath = localPath + @"\"; //Adiciona barra ao final do caminho

            PrivateKeyFile keyFile = new PrivateKeyFile(keyFilelocation);
            PrivateKeyFile[] keyFiles = new[] { keyFile };
            List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
            methods.Add(new PasswordAuthenticationMethod(username, psw));
            methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));
            var con = new ConnectionInfo(host, 22, username, methods.ToArray());

            try
            {
                using (var client = new SftpClient(con))
                {
                    client.Connect();

                    var files = client.ListDirectory(hostDirectory);

                    foreach (var file in files)
                    {
                        if (file.IsRegularFile) // somente arquivos ignora pastas
                        {
                            using (var fs = new FileStream(localPath + file.Name, FileMode.Create))
                            {
                                client.DownloadFile(file.FullName, fs);
                            }
                            client.RenameFile(file.FullName, readFileDestination + file.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.StackTrace.Contains("Renci.SshNet.Sftp.SftpSession.RequestRename")) throw new Exception("Falha ao Renomear arquivo no computador remoto.");
                else throw new Exception(ex.Message);
            }
        }

        /// <param name="localFile">caminho completo do arquivo no computador local.</param>
        /// <param name="hostDirectory">Diretorio no computador remoto (SFTP) onde será disponibilizado o arquivo</param>
        /// <summary>Realiza o download de todos os arquivos da pasta indicada.</summary>
        public void uploadFile(string localFile, string hostDirectory)
        {
            PrivateKeyFile keyFile = new PrivateKeyFile(keyFilelocation);
            PrivateKeyFile[] keyFiles = new[] { keyFile };
            List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
            methods.Add(new PasswordAuthenticationMethod(username, psw));
            methods.Add(new PrivateKeyAuthenticationMethod(username, keyFiles));

            var con = new ConnectionInfo(host, 22, username, methods.ToArray());
            try
            {

                using (var client = new SftpClient(con))
                {
                    client.Connect();

                    FileStream fs = File.OpenRead(localFile);
                    string filename = Path.GetFileName(localFile);

                    client.ChangeDirectory(hostDirectory);
                    if (client.Exists(hostDirectory + filename)) throw new Exception("Arquivo já existe no servidor destino");
                    client.UploadFile(fs, filename);

                }
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(@"Arquivo " + localFile + " não encontrado, verifique o caminho.", ex);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
