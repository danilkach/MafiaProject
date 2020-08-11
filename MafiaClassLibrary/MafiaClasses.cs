using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MafiaClassLibrary
{
    public interface IAction
    {
        void NightAction(City c);
        void AfterAction(City c);
        List<Object> Container { get; set; }
    }
    public class Role
    {
        public Role() { }
        public Role(string name, bool singular)
        {
            Name = name;
            Singular = singular;
            RoleCode = string.Empty;
        }
        public Role(string name, bool singular, string roleCode)
        {
            Name = name;
            Singular = singular;
            RoleCode = roleCode;
        }
        public string AssembleRole()
        {

            //MethodInfo
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("System.Data.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.dll");
            parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            parameters.ReferencedAssemblies.Add("MafiaClassLibrary.dll");
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, RoleCode);
            if(results.Errors.HasErrors)
            {
                string errors = string.Empty;
                foreach(CompilerError error in results.Errors)
                    errors += string.Format("Error ({0}): {1} (строка {2})\r\n", error.ErrorNumber, error.ErrorText, error.Line);
                Console.Write(errors);
                return errors;
            }

            Assembly assembly = results.CompiledAssembly;
            var type = assembly.GetType("UserDefined.Actions");
            action = (IAction)Activator.CreateInstance(type);
            return string.Empty;
        }
        public void NightAction(City c)
        {
            action.NightAction(c);
        }
        public void AfterAction(City c)
        {
            action.AfterAction(c);
        }
        public string Name { get; set; }
        IAction action;
        public bool Singular;
        public string RoleCode;
    }
    public class Pack
    {
        public void SavePack(string filepath)
        {
            XmlDocument doc = new XmlDocument();
            doc.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);
            doc.AppendChild(doc.CreateElement("Roles"));
            foreach(Role r in Roles)
            {
                XmlElement role = doc.CreateElement("Role");
                role.SetAttribute("name", r.Name);
                role.SetAttribute("singular", r.Singular.ToString());
                role.AppendChild(doc.CreateElement("Code"));
                role.FirstChild.InnerText = r.RoleCode;
                doc.DocumentElement.AppendChild(role);
                Console.WriteLine(doc.InnerXml);
            }
            doc.Save(filepath);
        }
        public void LoadPack(string filepath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);
            foreach(XmlElement r in doc.DocumentElement.ChildNodes)
                Roles.Add(new Role(r.GetAttribute("name"), Convert.ToBoolean(r.GetAttribute("singular")), r.FirstChild.InnerText));
        }
        public List<Role> Roles = new List<Role>();
    }
    public struct City
    {
        public City(int capacity, string ip, int port)
        {
            Players = new List<Object>(capacity);
            host = new Host(ip, port);
        }
        public List<Object> Players;
        private Host host;
    }
    public class Host
    {
        public Host(string ip, int port)
        {
            IP = ip;
            Port = port;

            stream = new MemoryStream(new byte[1048576], 0, 1048576, true, true);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);

            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clients = new List<User>();
            Console.Title = "Сервер";
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(0);
            socket.BeginAccept(AcceptCallback, null);

            Console.WriteLine("Сервер запущен. Ожидание подключений...");
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            User newUser = new User();
            Thread thread = new Thread(HandleClient);
            newUser.Socket = socket.EndAccept(ar);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nНовое подключение...\n");
            Console.ResetColor();
            thread.Start(newUser);
            clients.Add(newUser);
            socket.BeginAccept(AcceptCallback, null);
        }
        private void HandleClient(object obj)
        {
            User client = (User)obj;
            
            while(true)
            {
                stream.Position = 0;
                try
                {
                    client.Socket.Receive(stream.GetBuffer());
                }
                catch
                {
                    client.Socket.Shutdown(SocketShutdown.Both);
                    client.Socket.Disconnect(true);
                    clients.Remove(client);
                    Console.WriteLine(string.Format("{0} ({1}) отключился", client.Name, client.ID.ToString()));
                    return;
                }
                int code = reader.ReadInt32();
                switch((PacketInfo)code)
                {
                    case PacketInfo.ConnectionInfo:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("Получение информации об игроке...");
                        string name = reader.ReadString();
                        string playerImageString;
                        byte[] playerImage = Encoding.UTF8.GetBytes(playerImageString = reader.ReadString());
                        clients.Find((cl) => cl.Socket == client.Socket).Name = name;
                        clients.Find((cl) => cl.Socket == client.Socket).PlayerImage = playerImage;
                        Guid newGuid = clients.Find((cl) => cl.Socket == client.Socket).ID = Guid.NewGuid();
                        writer.Write((int)PacketInfo.ConnectionInfo);
                        writer.Write(name);
                        writer.Write(playerImageString);
                        writer.Write(newGuid.ToByteArray());
                        Console.WriteLine("Подключен " + name + " с ID: " + newGuid.ToString());
                        Console.WriteLine("Информация об игроке получена");
                        Console.WriteLine("Рассылка информации об игроке...");
                        foreach (Client cli in clients)
                            if (cli != client)
                                cli.Socket.Send(stream.GetBuffer());
                        Console.WriteLine("Рассылка информации об игроке завершена");
                        Console.ResetColor();
                        break;
                    }
                    case PacketInfo.ServerMessage:
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        string message = reader.ReadString();
                        writer.Write(client.Name);
                        Console.WriteLine(string.Format("Рассылка сообщения - {0} ({1}): {2}",
                            client.Name, client.ID.ToString(), message));
                        foreach (Client cli in clients)
                            cli.Socket.Send(stream.GetBuffer());
                        Console.WriteLine("Рассылка завершена");
                        Console.ResetColor();
                        break;
                    }
                    case PacketInfo.OnlyPlayerMessage:
                    {
                        string targetUserID = reader.ReadString();
                        clients.Find((c) => c.ID.ToString() == targetUserID).Socket.Send(stream.GetBuffer());
                        break;
                    }
                }
            }
        }
        public void SendPacket(PacketInfo info, object obj, Guid clientID)
        {
            stream.Position = 0;
            switch (info)
            {
                case PacketInfo.RoleInfo:
                    {
                        Role role = (Role)obj;
                        writer.Write((int)PacketInfo.RoleInfo);
                        writer.Write(role.Name);
                        writer.Write(role.Singular);
                        writer.Write(role.RoleCode);
                        clients.Find((c) => c.ID == clientID).Socket.Send(stream.GetBuffer());
                        break;
                    }
            }
        }
        string IP;
        int Port;
        public List<User> clients;

        private Socket socket;
        private MemoryStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;
    }
    public enum PacketInfo { ConnectionInfo, ServerMessage, OnlyPlayerMessage, RoleInfo }
    public interface IPlayer
    {
        byte[] PlayerImage { get; set; }
        Role PlayerRole { get; set; }
        string Name { get; set; }
        Dictionary<string, bool> Flags { get; set; }
    }
    public abstract class Client : IPlayer
    {
        public Client()
        {
            stream = new MemoryStream(new byte[1048576], 0, 1048576, true, true);
            reader = new BinaryReader(stream);
            writer = new BinaryWriter(stream);
        }
        public void Connect(string ip, int port)
        {
            Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            Socket.ReceiveBufferSize = 1048576;
            Socket.Connect(ip, port);
            Flags = new Dictionary<string, bool>();
            Flags.Add("isAlive", true);
            Flags.Add("isAbleToVote", true);
        }
        public void SendPacket(PacketInfo info, object obj)
        {
            stream.Position = 0;
            switch(info)
            { 
                case PacketInfo.ConnectionInfo:
                {
                    Console.WriteLine("Отправка данных об игроке...");
                    writer.Write((int)PacketInfo.ConnectionInfo);
                    writer.Write(Name);
                    writer.Write(Encoding.UTF8.GetString(PlayerImage));
                    Task.Run(() => { while (true) ReceivePacket(); } );
                    Socket.Send(stream.GetBuffer());
                    Console.WriteLine("Данные об игроке отправлены");
                    break;
                }
                case PacketInfo.ServerMessage:
                {
                    Console.WriteLine("Сообщение");
                    writer.Write((int)PacketInfo.ServerMessage);
                    writer.Write((string)obj);
                    Socket.Send(stream.GetBuffer());
                    break;
                }
            }
        }
        public void ReceivePacket()
        {
            stream.Position = 0;
            try
            {
                Socket.Receive(stream.GetBuffer());
            }
            catch
            {
                return;
            }
            int code = reader.ReadInt32();
            switch((PacketInfo)code)
            {
                case PacketInfo.ConnectionInfo:
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Получение информации об игроке...");
                    User newUser = new User();
                    newUser.Name = reader.ReadString();
                    newUser.PlayerImage = Encoding.UTF8.GetBytes(reader.ReadString());
                    newUser.ID = new Guid(reader.ReadBytes(16));
                    if(DrawPlayer != null)
                        DrawPlayer(newUser);
                    Console.WriteLine(string.Format("Информация об игроке {0} ({1}) получена",
                        newUser.Name, newUser.ID.ToString()));
                    Console.ResetColor();
                    break;
                }
                case PacketInfo.ServerMessage:
                {
                    string message = reader.ReadString();
                    string sender = reader.ReadString();
                    Console.WriteLine(sender + ": " + message);
                    if(Message != null)
                        Message(sender + ": " + message);
                    break;
                }
                case PacketInfo.RoleInfo:
                {
                    Role role = new Role();
                    role.Name = reader.ReadString();
                    role.Singular = reader.ReadBoolean();
                    role.RoleCode = reader.ReadString();
                    PlayerRole = role;
                    PlayerRole.AssembleRole();
                    break;
                }
            }
        }
        public void RegisterDrawPlayerHandler(DrawPlayerHandler handler)
        {
            DrawPlayer = handler;
        }
        public void RegisterPickPlayerHandler(PickPlayerHandler handler)
        {
            PickPlayer = handler;
        }
        public void RegisterMessageHandler(MessageHandler handler)
        {
            Message = handler;
        }
        public Guid ID { get; set; }
        public byte[] PlayerImage { get; set; }
        public Role PlayerRole { get; set; }
        public string Name { get; set; }
        public Dictionary<string, bool> Flags { get; set; }

        public Socket Socket { get; set; }
        private MemoryStream stream;
        private BinaryReader reader;
        private BinaryWriter writer;

        public PickPlayerHandler PickPlayer;
        public DrawPlayerHandler DrawPlayer;
        public MessageHandler Message;
    }
    public class User : Client
    {
        public User()
        {
        }
        public void Vote()
        {
            
        }
        public List<User> players;
    }
    public sealed class Bot : User
    {

    }
    public delegate void DrawPlayerHandler(User user);
    public delegate Guid PickPlayerHandler();
    public delegate void MessageHandler(string message);
}
