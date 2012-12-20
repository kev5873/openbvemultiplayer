using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace OpenBve.OldCode
{

    class Multiplayer
    {
        string host = "127.0.0.1";
        int port = 4567;
        TcpClient client = null;
        bool connected = false;
        public double myPosition = 0;
        int[] players = new int[3];

        public void connect()
        {
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = 0;
            }
            try
            {
                client = new TcpClient(host, port);
                client.SendBufferSize = 1024;
                connected = true;
            }
            catch (Exception)
            {
                Game.AddDebugMessage("Could not connect to " + host, 15.0);
            }
        }

        public void disconnect()
        {
            if (connected)
            {
                client.Close();
                connected = false;
            }
        }

        public void refreshData()
        {
            while (connected)
            {
                System.Threading.Thread.Sleep(4000);
                // MULTIPLAYER TESTING
                // Translate the passed message into ASCII and store it as a Byte array.
                myPosition = (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].FrontAxlePosition + 0.5 * TrainManager.PlayerTrain.Cars[0].Length);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(myPosition.ToString());
                NetworkStream stream = client.GetStream();
                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                string[] playerData = responseData.Split(':');

                Game.AddDebugMessage(responseData, 5.0);
                if (responseData == "Error: Server Full")
                {
                    disconnect();
                }
            }
        }
    }
}
