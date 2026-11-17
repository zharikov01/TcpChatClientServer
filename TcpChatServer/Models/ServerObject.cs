using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChatServer.Models;

    
// Серверный объект для работы в TCP-чате
public class ServerObject
{
    // сервер для прослушивания сети
    private static TcpListener _tcpListener; 
    
    // подключения участников чата
    private List<ClientObject> _clients = [];

    
    // добавить запись о клиенте в коллекцию сведений о подключениях клиентов 
    protected internal void AddConnection(ClientObject clientObject) =>
        _clients.Add(clientObject);

    // удалить закрытое соединение клиента
    protected internal void RemoveConnection(string id) {
        // найти закрытое подключение по Id
        ClientObject client = _clients.FirstOrDefault(c => c.Id == id);
        if (client != null) _clients.Remove(client);
    } // RemoveConnection


    // слушатель входящих подключений
    protected internal void Listen() {
        try {
            // запуск прослушки сети
            _tcpListener = new TcpListener(IPAddress.Any, 8888);
            _tcpListener.Start();
            Console.WriteLine("Чат-сервер стартовал, ожидаю подключений...");

            while (true) {
                // ожидание очередного клиентского запроса
                TcpClient tcpClient = _tcpListener.AcceptTcpClient();

                // запуск обработки очередного клиентского запроса
                // в отдельном потоке исполнения
                ClientObject clientObject = new (tcpClient, this);
                Task clientTask = new (clientObject.Process);
                clientTask.Start();
            } // while
        }  catch (Exception ex) {
            Console.WriteLine(ex.Message);
            Disconnect();
        } // try-catch
    } // Listen


    // отключение всех клиентов
    protected internal void Disconnect() {
        _tcpListener.Stop();
        _clients.ForEach(c => c.Close());
    } // Disconnect


    // передача сообщения всем клиентам, кроме отправителя
    protected internal void BroadcastMessage(string message, string senderId) {
        byte[] data = Encoding.UTF8.GetBytes(message);
        _clients
            .Where(c => c.Id != senderId)
            .ToList()
            .ForEach(c => c.Stream.Write(data, 0, data.Length));
    } // BroadcastMessage
} // class ServerObject
