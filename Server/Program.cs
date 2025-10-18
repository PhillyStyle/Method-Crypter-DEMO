using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Server
{
    class Program
    {
        static string key;
        static string iv;
        static List<TypeAndMethodEncryptionEntry> tnmee = new List<TypeAndMethodEncryptionEntry>();
        static List<TypeAndStringArrayEncryptionEntry> tnsaee = new List<TypeAndStringArrayEncryptionEntry>();

        static void Main(string[] args)
        {
            if (!LoadSettings())
            {
                Console.WriteLine("Error: No Settings found. Exiting...");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Console.WriteLine("Server started on port 9000...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected.");
                ThreadParams tp = new ThreadParams(client, key, iv, tnmee, tnsaee);
                ThreadPool.QueueUserWorkItem(HandleClient, tp);
            }
        }

        private static bool LoadSettings()
        {
            string fileName = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "server_settings.json"
            );

            if (File.Exists(fileName))
            {
                string jsonText = File.ReadAllText(fileName);
                JsonSyncSettings jss = JsonSerializer.Deserialize<JsonSyncSettings>(jsonText);

                if (jss != null)
                {
                    key = jss.jsonkey;
                    iv = jss.jsoniv;

                    if (jss.jsonTypeAndMethod != null)
                    {
                        foreach (var entry in jss.jsonTypeAndMethod)
                            tnmee.Add(entry);
                    }

                    if (jss.jsonTypeAndStringArray != null)
                    {
                        foreach (var entry in jss.jsonTypeAndStringArray)
                            tnsaee.Add(entry);
                    }
                    return true;
                }
            }
            return false;
        }

        public class JsonSyncSettings
        {
            public string jsonkey { get; set; }
            public string jsoniv { get; set; }
            public List<TypeAndMethodEncryptionEntry> jsonTypeAndMethod { get; set; }
            public List<TypeAndStringArrayEncryptionEntry> jsonTypeAndStringArray { get; set; }
        }

        public class TypeAndMethodEncryptionEntry
        {
            public string Type { get; set; }
            public string MethodName { get; set; }
        }

        public class TypeAndStringArrayEncryptionEntry
        {
            public string Type { get; set; }
            public string ArrayName { get; set; }
        }

        public class ThreadParams
        {
            public ThreadParams(TcpClient c, string k, string i, List<TypeAndMethodEncryptionEntry> t, List<TypeAndStringArrayEncryptionEntry> tns) { client = c; key = k; iv = i; tnmee = t; tnsaee = tns; }
            public TcpClient client;
            public string key;
            public string iv;
            public List<TypeAndMethodEncryptionEntry> tnmee;
            public List<TypeAndStringArrayEncryptionEntry> tnsaee;
        }

        static void HandleClient(object obj)
        {
            var tp = (ThreadParams)obj;
            var client = tp.client;
            var key = tp.key;
            var iv = tp.iv;
            var tnmee = tp.tnmee;
            var tnsaee = tp.tnsaee;
            var stream = client.GetStream();

            try
            {
                string username = null;
                string password = null;
                bool authenticated = false;

                foreach (var line in ReadCommands(stream))
                {
                    var tokens = Tokenize(line);

                    if (tokens.Count == 0)
                        continue;

                    string cmd = tokens[0].ToLowerInvariant();
                    tokens.RemoveAt(0);

                    if (!authenticated)
                    {
                        if (cmd == "login" && tokens.Count >= 2)
                        {
                            username = tokens[0];
                            password = tokens[1];

                            if (username == "myuser" && password == "mypass")
                            {
                                authenticated = true;
                                Console.WriteLine("Login successful.");
                                SendCommand(stream, "ok", "login");

                                // After login, send run command
                                SendCommand(stream, "run");
                            }
                            else
                            {
                                Console.WriteLine("Login failed.");
                                SendCommand(stream, "error", "invalid_credentials");
                                client.Close();
                                return;
                            }
                        }
                        else
                        {
                            SendCommand(stream, "error", "expected_login");
                            Console.WriteLine("Received: " + cmd);
                        }
                    }
                    else
                    {
                        if (cmd == "run")
                        {
                            if (tokens.Count == 1 && tokens[0].ToLowerInvariant() == "start")
                            {
                                Console.WriteLine("Client started run.");
                            }
                            else if (tokens.Count == 1 && tokens[0].ToLowerInvariant() == "end")
                            {
                                Console.WriteLine("Client ended run.");
                                client.Close();
                                return;
                            }
                        }
                        else if (cmd == "echo")
                        {
                            // echo command with arbitrary data
                            string echoed = string.Join(" ", tokens.GetRange(0, tokens.Count));
                            Console.WriteLine("Client says: " + echoed);
                        }
                        else if (cmd == "decrypt")
                        {
                            if (tokens.Count == 1 && tokens[0].ToLowerInvariant() == "run")
                            {
                                Console.WriteLine("Received decrypt run.");
                                SendCommand(stream, "decrypt", "run", "begin");
                                foreach (var t in tnmee)
                                    SendCommand(stream, "decrypt", "run", t.Type, t.MethodName, key, iv);
                                SendCommand(stream, "decrypt", "run", "end");

                                SendCommand(stream, "decrypt", "run_strings", "begin");
                                foreach (var t in tnsaee)
                                    SendCommand(stream, "decrypt", "run_strings", t.Type, t.ArrayName, key, iv);
                                SendCommand(stream, "decrypt", "run_strings", "end");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Client error: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        static IEnumerable<string> ReadCommands(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            StringBuilder sb = new StringBuilder();

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                string current = sb.ToString();
                int newlineIndex;
                while ((newlineIndex = current.IndexOf('\n')) >= 0)
                {
                    string command = current.Substring(0, newlineIndex).TrimEnd('\r');
                    yield return command;

                    current = current.Substring(newlineIndex + 1);
                }

                sb.Clear();
                sb.Append(current);
            }
        }

        static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            if (string.IsNullOrEmpty(input))
                return tokens;

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;

                    // Check for empty quoted string
                    if (!inQuotes && sb.Length == 0)
                    {
                        // "" detected, add empty token
                        tokens.Add("");
                    }

                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(c))
                {
                    // End of unquoted token
                    if (sb.Length > 0)
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        // ignore multiple spaces
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            // Add last token if any
            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }


        static void SendCommand(NetworkStream stream, params string[] parts)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                string arg = parts[i];

                if (i > 0) sb.Append(" "); // add space before all but first

                if (arg == "")
                {
                    sb.Append("\"\""); // empty string is sent as quoted empty
                }
                else if (arg.Contains(" "))
                {
                    sb.Append("\"").Append(arg).Append("\""); // quote strings with spaces
                }
                else
                {
                    sb.Append(arg); // plain token
                }
            }

            sb.Append("\n");
            byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Sent: " + sb.ToString().TrimEnd('\n'));
        }

    }

    public static class CommandReader
    {
        public static IEnumerable<string> ReadCommands(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            StringBuilder sb = new StringBuilder();

            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                string current = sb.ToString();
                int newlineIndex;
                while ((newlineIndex = current.IndexOf('\n')) >= 0)
                {
                    string command = current.Substring(0, newlineIndex).TrimEnd('\r');
                    if (!string.IsNullOrWhiteSpace(command))
                        yield return command;

                    current = current.Substring(newlineIndex + 1);
                }

                sb.Clear();
                sb.Append(current);
            }
        }
    }
}