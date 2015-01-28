using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace OpenBve.OldCode
{

    class PlayerObject
    {
        public int userID = 0;
        public double position = 0;
        public bool isItMe = false;

        public PlayerObject(string uid, string pos, string me)
        {
            userID = Convert.ToInt32(uid);
            position = Convert.ToDouble(pos);
            if (me == "M")
                isItMe = true;

        }
    }

    class Multiplayer
    {
        TcpClient client = null;
        bool connected = false;
        public double myPosition = 0;
        public double myID = 0;
        public List<PlayerObject> players = new List<PlayerObject>();
        public int totalPlayers = 0;
        bool firstResponse = true;

        public void connect(string host, int port)
        {
            try
            {
                client = new TcpClient(host, port);
                client.SendBufferSize = 1024;
                connected = true;
                Game.AddDebugMessage("Connected to " + host, 15.0);
            }
            catch (Exception)
            {
                Game.AddDebugMessage("Could not connect to " + host, 15.0);
            }
        }

        public void disconnect()
        {
            myPosition = 0;
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(myPosition.ToString());
            NetworkStream stream = client.GetStream();
            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);
            client.Close();
            connected = false;
            Environment.Exit(0);
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

                if (firstResponse)
                {
                    string[] returnObjects = responseData.Split(';');
                    for (int i = 0; i < returnObjects.Length; i++)
                    {
                        string[] playerData = returnObjects[i].Split(':');
                        
                        if (playerData[0] == "")
                        {
                            // Do nothing
                        }
                        else
                        {
                            players.Add(new PlayerObject(playerData[0], playerData[1], playerData[2]));
                        }
                    }
                    totalPlayers = returnObjects.Length;
                    firstResponse = !firstResponse;
                }
                else
                {
                    string[] returnObjects = responseData.Split(';');
                    if (totalPlayers < returnObjects.Length)
                    {
                        players.Clear();
                        for (int i = 0; i < returnObjects.Length; i++)
                        {
                            string[] playerData = returnObjects[i].Split(':');
                            if (playerData[0] == "")
                            {
                                // Do nothing
                            }
                            else
                            {
                                players.Add(new PlayerObject(playerData[0], playerData[1], playerData[2]));
                            }
                        }
                        totalPlayers = returnObjects.Length;
                    }
                    else // Update player positions
                    {
                        for (int i = 0; i < returnObjects.Length; i++)
                        {
                            string[] playerData = returnObjects[i].Split(':');
                            if (playerData[0] == "")
                            {
                                // Do nothing
                            }
                            else
                            {
                                PlayerObject thatPlayer = players.Find(
                                    delegate(PlayerObject theP)
                                    {
                                        return theP.userID == Convert.ToInt32(playerData[0]);
                                    }
                                );
                                thatPlayer.position = Convert.ToDouble(playerData[1]);
                            }
                        }
                    }

                    double largestPosition = 1;
                    Int32 highestIndex = 1;
                    for (int i = 0; i < players.Count; i++)
                    {
                        if (!players[i].isItMe)
                        {
                            Game.AddDebugMessage(Convert.ToString(players[i].position), 5.0);
                            if (players[i].position > myPosition)
                            {
                                moveTrain(players[i].position, 0);
                                highestIndex = i;
                                largestPosition = players[i].position;
                            }
                            if (players[highestIndex].position < 1)
                            {
                                moveTrain(1000000, 0);
                            }
                        }
                    }

                }
                if (responseData == "Error: Server Full")
                {
                    disconnect();
                }
            }
        }

        private void moveTrain(double position,Int32 index)
        {
            double amtMove;
            amtMove = position - Double.Parse(TrainManager.Trains[index].Cars[0].RearAxle.Follower.TrackPosition.ToString());
            for (int i = 1; i < TrainManager.Trains.Length; i++)
            {
                for (int j = 0; j < TrainManager.Trains[index].Cars.Length; j++)
                {
                    TrainManager.MoveCar(TrainManager.Trains[index], j, amtMove, 0.01);
                }
            }
        }

    }
}
