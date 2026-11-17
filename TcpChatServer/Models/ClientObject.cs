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

    private string _userName = "";
    private readonly TcpClient _tcpClient;
    private readonly ServerObject _server;


    // конструктор выполняет внедрение зависимостей, получает
    // готовые объекты
    public ClientObject(TcpClient tcpTcpClient, ServerObject serverObject) {
        // генерация уникального Id для клиентского подключения
        Id = Guid.NewGuid().ToString();

        _tcpClient = tcpTcpClient;
        _server = serverObject;

        // сетевой поток получаем сразу: клиент добавляется в коллекцию
        // соединений и может получать сообщения других участников
        Stream = _tcpClient.GetStream();

        // добавить себя в коллекцию соединений
        _server.AddConnection(this);
    } // ClientObject


    // работа клиента в чате
    protected internal void Process() {
        try {
            // первое сообщение от клиента - имя клиента в чате
            _userName = GetMessage();

            // сообщить всем о входе пользователя в чат, вывести это же
            // сообщение в консоль сервера
            var message = $"{_userName} вошел в чат";
            _server.BroadcastMessage(message, this.Id);
            Console.WriteLine(message);

            // циклическая часть работы с клиентом
            // получаем сообщения от клиентов
            while (true) {
                try {
                    message = GetMessage();

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
                    break;
                } // try-catch
            } // while
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        } finally {
            // удалить соединение из коллекции, закрыть подключение
            _server.RemoveConnection(this.Id);
            Close();
        } // try-catch-finally
    } // Process


    // получение входящего сообщения и вывод его в строку
    private string GetMessage() {
        var data = new byte[256];
        using var msgBytes = new MemoryStream();

        do {
            // Stream.Read() блокирующий вызов, возвращает количество
            // фактически прочитанных байтов
            int bytes = Stream.Read(data, 0, data.Length);

            // 0 байтов - клиент закрыл соединение
            if (bytes == 0) {
                throw new IOException("Клиент закрыл соединение");
            } // if

            // в сообщение добавляем только прочитанные байты, а не весь буфер
            msgBytes.Write(data, 0, bytes);
        } while (Stream.DataAvailable);

        return Encoding.UTF8.GetString(msgBytes.ToArray());
    } // GetMessage


    // закрытие подключения
    protected internal void Close() {
        Stream.Close();
        _tcpClient.Close();
    } // Close
} // class ClientObject
