using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tcp
{
	class file_server
	{
		/// <summary>
		/// The PORT
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		const int BUFSIZE = 1000;

        /// <summary>
        /// Initializes a new instance of the <see cref="file_server"/> class.
        /// Opretter en socket.
        /// Venter på en connect fra en klient.
        /// Modtager filnavn
        /// Finder filstørrelsen
        /// Kalder metoden sendFile
        /// Lukker socketen og programmet
        /// </summary>
        /// 
        
       
        private TcpListener serverSocket;
        private TcpClient clientSocket;
        IPAddress localAddr = IPAddress.Parse("10.0.0.1");
        
        


        private file_server ()
		{
            serverSocket = new TcpListener(localAddr,PORT);
            serverSocket.Start();
           
            Console.WriteLine(" >> Server Started");
            Console.WriteLine(" >> Accept connection from client");
			Console.WriteLine(" ---------------------------------");
			Console.WriteLine("");

            while (true)
            {
                try
                {

					NetworkStream io = EstablishConnection();
               
                    String file = null;
                    file = LIB.readTextTCP(io);
                    Console.WriteLine(" >> Client asking for file - " +file);

					if(new FileInfo(file).Exists)
					{
						ValidFile(file,io);
					}
					   else
					{
						InvalidFile(file,io);
					}               
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

		private NetworkStream EstablishConnection()
        {
            Console.WriteLine(" >> Waiting for client");
            clientSocket = serverSocket.AcceptTcpClient();
            Console.WriteLine(" >> Client Connected");
            return clientSocket.GetStream();
        }

        private void ValidFile(string file, NetworkStream io)
        {
            Console.WriteLine($" >> Valid file");
            LIB.writeTextTCP(io, $"Valid");

            long fileSize = new FileInfo(file).Length;
            Console.WriteLine($" >> File Size is: {fileSize}");
            LIB.writeTextTCP(io, $"{fileSize}");

            sendFile(file, fileSize, io);
        }

        private void InvalidFile(string file, NetworkStream io)
        {
            Console.WriteLine($" >> Invalid file {file}");
            LIB.writeTextTCP(io, $"Invalid");
            ShutDownConnection(io);
        }

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// The filename.
		/// </param>
		/// <param name='fileSize'>
		/// The filesize.
		/// </param>
		/// <param name='io'>
		/// Network stream for writing to the client.
		/// </param>
		private void sendFile (String fileName, long fileSize, NetworkStream io)
		{
			Console.WriteLine($" >> Sending file: {fileName}");

			Byte[] fileBytes = File.ReadAllBytes(fileName);

			int offset = 0;
			for (int i = (int)fileSize; i != 0;)
			{
				if(i > BUFSIZE)
				{
					io.Write(fileBytes, offset, BUFSIZE);
					offset += BUFSIZE;
					i -= BUFSIZE;
				}
				else 
				{
					io.Write(fileBytes, offset,i);
					i -= i;
				}
            }
			ShutDownConnection(io);
        }
		private void ShutDownConnection(NetworkStream io)
		{
			clientSocket.Close();
			io.Close();
            Console.WriteLine(" >> Connection shut down");
			Console.WriteLine(" ---------------------------------");
            Console.WriteLine("");
		}
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			file_server fs = new file_server();
		}
	}
}
