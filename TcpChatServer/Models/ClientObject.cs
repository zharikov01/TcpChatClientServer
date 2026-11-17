using System.Net.Sockets;
using System.Text;

namespace TcpChatServer.Models;

// Работа сервера с клиентом чата - объект этого класса создается 
// для каждого подключенного клиента
public class ClientObject
{
    // уникальный идентификатор клиента, формируется в конструкторе
    protected internal string Id { get; private set; }
    
    // сетевой поток для обмена данными 
    protected internal NetworkStream Stream { get; private set; }

    private string    _userName;
    private TcpClient _tcpClient;
    private ServerObject _server;


    // конструктор выполняет внедрение зависимостей, получает
    // готовые объекты
    public ClientObject(TcpClient tcpTcpClient, ServerObject serverObject) {
        // генерация уникального Id для клиентского подключения
        Id = Guid.NewGuid().ToString();

        _tcpClient = tcpTcpClient;
        _server = serverObject;

        // добавить себя в коллекцию соединений
        _server.AddConnection(this); 
    } // ClientObject


    // работа клиента в чате
    protected internal void Process() {
        try {
            // первое сообщение от клиента - имя клиента в чате
            Stream = _tcpClient.GetStream();
            _userName = GetMessage();

            // сообщить всем о входе пользователя в чат, вывести это же 
            // сообщение в консоль сервера
            var message = $"{_userName} вошел в чат";
            _server.BroadcastMessage(message, this.Id);
            Console.WriteLine(message);

            // TODO: можно реализовать команды клиента, например, отправить
            // TODO: список пользователей клиенту...

            // циклическая часть работы с клиентом
            // получаем сообщения от клиентов
            while (true) {
                try {
                    message = GetMessage();

                    // тут можно распарсить сообщение от клиента и обработать
                    // команды серверу

                    // эхо-сообщение в консоль
                    message = $"{_userName}: {message}";
                    Console.WriteLine(message);

                    // отправить сообщение всем участникам чата, кроме текущего пользователя
                    _server.BroadcastMessage(message, this.Id);
                } catch {
                    // любую проблему при работе с клиентом трактуем как 
                    // отключение клиента
                    message = $"{_userName} покинул чат";
                    Console.WriteLine(message);
                    _server.BroadcastMessage(message, this.Id);
                    _server.RemoveConnection(this.Id);
                    break;
                } // try-catch
            } // while
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            _server.RemoveConnection(this.Id);
        } // try-catch
    } // Process


    // получение входящего сообщения и вывод его в строку
    private string GetMessage() {
        var data = new byte[256];
        var msgBytes = new List<byte>();
        
		do {
            // Stream.Read() блокирующий вызов
            Stream.Read(data, 0, data.Length);
            msgBytes.AddRange(data);
        } while (Stream.DataAvailable);

        return Encoding.UTF8.GetString([.. msgBytes]);
    } // GetMessage


    // закрытие подключения
    protected internal void Close() {
        Stream?.Close();
        _tcpClient.Close();
    } // Close
} // class ClientObject
