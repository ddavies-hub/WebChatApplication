using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Connection
{
   public class Connection
    {
        private System.Net.Sockets.TcpClient _client { get; set; }
        private System.Net.Sockets.NetworkStream _stream { get; set; }
        private System.IO.StreamReader _reader { get; set; }
        private System.IO.StreamWriter _writer { get; set; }
        private string _user { get; set; }
        private string _channel { get; set; }

        public Connection(string username, string channel) 
        {
            string host = "chat.freenode.net";
            int port = 6667;
            _user = username;
            _channel = channel;
            _client = new System.Net.Sockets.TcpClient(host, port);
            _stream = _client.GetStream();
            _reader = new System.IO.StreamReader(_stream);
            _writer = new System.IO.StreamWriter(_stream);
        }

        public List<string> InitialConnection()
        {
            SendCredentials();
            return GetMessages();
        }

        private void SendCredentials()
        {
            _writer.WriteLine("USER " + _user + " user user user user :user@user.com");
            _writer.WriteLine("NICK " + _user);
            _writer.Flush();
        }

        private void SendJoin(string channel)
        {
            _channel = channel;
            _writer.WriteLine("JOIN " + channel);
            _writer.Flush();
        }

        public bool HasMessage() 
        {
            return _reader.EndOfStream;
        }

        public void SendMessage(string msg)
        {
            string msgFormat = String.Format("PRIVMSG {0} : {1}", _channel, msg); 
            _writer.WriteLine(msgFormat);
            _writer.Flush();
        }

        public List<string> GetNames()
        {
            _writer.WriteLine("NAMES " + _channel);
            _writer.Flush();
            System.Threading.Thread.Sleep(1500);
            return GetMessages();
        }

        public List<string> GetMessages()
        {
            List<string> messages = new List<string>();

            while (_stream.DataAvailable)
            {
                var line = _reader.ReadLine();
                if (line.Contains("PING"))
                {
                        var code =  line.Substring(line.IndexOf(":"), line.Length - line.IndexOf(":"));
                        _writer.WriteLine("PONG " + code);
                        _writer.Flush();
                }

                if (line.Contains("001"))
                {
                    if (!string.IsNullOrWhiteSpace(_channel))
                    {
                        SendJoin(_channel);
                    }   
                }

                Console.WriteLine(line);
                messages.Add(line);
            }
            return ParseMessages(messages);
        }

        /*
         * Parse the PRIVMSG returned from the server. 
         * Rather than looking at the string User!User@domain #channel PRIVMSG : <msg> and using multiple substrings and IndexOf
         * It is easier to reverse the string and get the index from the character :
        */
        public string ParsePRIVMSG(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            var reversed = new string(charArray);
            string msg = reversed.Substring(0, reversed.IndexOf(":"));
            var msgArray = msg.ToCharArray();
            Array.Reverse(msgArray);

            return new string(msgArray);
        }

        public List<string> ParseMessages(List<string> messages)
        {
            var results = new List<string>();

            var channelMessages = messages.Where(s => s.Contains("PRIVMSG") && !s.Contains("005")).ToList();
            var joinMessages = messages.Where(s => s.Contains("JOIN") && !s.Contains("005")).ToList();
            var partMessages = messages.Where(s => s.Contains("PART") && !s.Contains("005")).ToList();

            if (channelMessages.Any())
            {
                foreach (var chanMsg in channelMessages)
                {
                    string nickname = chanMsg.Substring(1, chanMsg.IndexOf('!') - 1);
                    string msg = ParsePRIVMSG(chanMsg);   
                    
                    results.Add(nickname + " : " + msg);
                }
            }
            if (joinMessages.Any())
            {
                foreach (var join in joinMessages)
                {
                    string nickname = join.Substring(1, join.IndexOf('!') - 1);
                    results.Add("<p class='join-channel'>• " + nickname + " has joined the chat room.</p>");
                }
            }

            if (partMessages.Any())
            {
                foreach (var part in partMessages)
                {
                    string nickname = part.Substring(1, part.IndexOf('!') - 1);
                    results.Add("<p class='join-channel'>• " + nickname + " has left the chat room.</p>");
                }
            }
            else if (!channelMessages.Any() && !joinMessages.Any() && !partMessages.Any())
            {
                results = messages;
            }

            return results;
        }

    }
}