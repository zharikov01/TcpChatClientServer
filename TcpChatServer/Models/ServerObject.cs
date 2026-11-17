using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpChatServer.Models;


// Серверный объект для работы в TCP-чате
public class ServerObject
{
    // сервер для прослушивания сети
    private TcpListener? _tcpListener;

    // подключения участников чата
    private readonly List<ClientObject> _clients = [];

    // клиенты обслуживаются в разных потоках, доступ к коллекции
    // подключений синхронизируем
    private readonly object _locker = new();


    // добавить запись о клиенте в коллекцию сведений о подключениях клиентов
    protected internal void AddConnection(ClientObject clientObject) {
        lock (_locker) {
            _clients.Add(clientObject);
        } // lock
    } // AddConnection


    // удалить закрытое соединение клиента
    protected internal void RemoveConnection(string id) {
        lock (_locker) {
            // найти закрытое подключение по Id
            ClientObject? client = _clients.FirstOrDefault(c => c.Id == id);
            if (client != null) _clients.Remove(client);
        } // lock
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
        _tcpListener?.Stop();

        lock (_locker) {
            _clients.ForEach(c => c.Close());
            _clients.Clear();
        } // lock
    } // Disconnect


    // передача сообщения всем клиентам, кроме отправителя
    protected internal void BroadcastMessage(string message, string senderId) {
        byte[] data = Encoding.UTF8.GetBytes(message);

        lock (_locker) {
            foreach (var client in _clients.Where(c => c.Id != senderId)) {
                try {
                    client.Stream.Write(data, 0, data.Length);
                } catch {
                    // клиент отключился - его соединение будет
                    // удалено в потоке обслуживания этого клиента
                } // try-catch
            } // foreach
        } // lock
    } // BroadcastMessage
} // class ServerObject
