using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DCP
{
    class Program
    {
        static DCPClient _client;
        static IFormatter _formatter;

        class Credentials : ServiceClientCredentials
        {

        }

        static Program()
        {
            _client = new DCPClient(new Credentials());
            _client.HttpClient.Timeout = TimeSpan.FromHours(1);
            _formatter = new BinaryFormatter();
        }

        static void Main(string[] args)
        {
            Help();
            while (true)
            {
                try
                {
                    Console.Write("DCP>");
                    string s = Console.ReadLine();

                    if (s == "dcp -h")
                    {
                        Help();
                        continue;
                    }

                    if (s == "dcp -d")
                    {
                        Download();
                        continue;
                    }

                    if (s.StartsWith("dcp -u") && s.Contains("="))
                    {
                        string file = s.Split('=')[1];
                        Upload(file);
                        continue;
                    }
                    Console.WriteLine("Incorrect command");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.InnerException?.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        static void Download()
        {
            Console.WriteLine("Download...");
            var base64 = _client.Values.GetAsync().Result;
            var buffer = Convert.FromBase64String(base64.ToString());
            using(MemoryStream ms = new MemoryStream(buffer))
            {
                var result = _formatter.Deserialize(ms) as Tuple<string, byte[]>;
                File.WriteAllBytes(result.Item1, result.Item2);
                Console.WriteLine("Done");
            }
        }

        static void Upload(string file)
        {
            Console.WriteLine("Upload...");
            using (MemoryStream ms = new MemoryStream())
            {
                _formatter.Serialize(ms, new Tuple<string, byte[]>(file.Split('\\').Last(), File.ReadAllBytes(file)));
                _client.Values.PostAsync(ms.GetBuffer()).Wait();
                Console.WriteLine("Done");
            }
        }

        static void Help()
        {
            string[] help = new string[]
            {
                "-h: help",
                "-u: upload file",
                "-d: download file",
                "usage: dcp -u=text.txt",
                ""
            };
            foreach (string s in help)
                Console.WriteLine(s);
        }
    }
}
