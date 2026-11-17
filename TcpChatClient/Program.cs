/*
 * Клиент для чата на протоколе TCP
 */

using System.Net.Sockets;
using System.Text;


string userName = "guest";
string host = "127.0.0.1"; // "192.168.1.113"
int port = 8888;

Console.Title = "Занятие 30.11.2026 - клиент для чата по протоколу TCP";
Console.Write("\n\n\n\nВаше имя для чата: ");
userName = Console.ReadLine()!;

// экземпляр клиента для работы с протоколом TCP
using var tcpClient = new TcpClient();

try {
    // подключение к серверу 
    tcpClient.Connect(host, port);

    // сетевой поток для общения с сетью
    using var stream = tcpClient.GetStream();

    // первое сообщение для сервера - отправка имени клиента
    byte[] data = Encoding.UTF8.GetBytes(userName);
    stream.Write(data, 0, data.Length);

    // запуск приема сообщений от сервера в отдельном потоке
    Task receiveTask = new(() => ReceiveMessage(stream));
    receiveTask.Start();

    Console.WriteLine($"Добро пожаловать, {userName}");

    // главный цикл работы клиента чата - ввод
    // и отправка сообщений - в главном потоке
    SendMessages(stream);
}
catch (Exception ex) {
    Console.WriteLine(ex.Message);
}

return;


// получение сообщений - выполняем в отдельном потоке исполнения
void ReceiveMessage(NetworkStream stream) {
    while (true) {
        // прием очередного сообщения и вывод его в консоль
        try {
            var data = new byte[256];
            var sbr = new StringBuilder();
            do {
                int bytes = stream.Read(data, 0, data.Length);
                sbr.Append(Encoding.UTF8.GetString(data, 0, bytes));
            } while (stream.DataAvailable);

            // !! дополнительную обработку принятых сообщений
            // !! сервера можно реализовать тут !!

            // просто вывод в консоль сообщения
            lock (typeof(Console)) {
                var fg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(sbr);
                Console.ForegroundColor = fg;
            } // lock
        }
        catch {
            Console.WriteLine("Подключение прервано!");
            break;
        } // try-catch
    } // while
} // ReceiveMessage


// отправка сообщений
void SendMessages(NetworkStream stream) {
    while (true) {
        Console.Write("Ваше сообщение: ");

        // ввести сообщение, не передавать пустую строку серверу
        var message = Console.ReadLine()!;
        if (string.IsNullOrEmpty(message)) {
            continue;
        } // if

        // !!! вот тут можно добавить обработку команд, передаваемых серверу !!!
        // примеры
        // @time, @logout, @ren newUserName, @to имяПользователя текстСообщения,....

        // сформировать и вывести массив байтов сообщения
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    } // while
} // SendMessage
