/*
 * Сервер для чата на протоколе TCP
 * При первом запуске разрешить доступ к сети для этого приложения
 * Антивирус ошибочно срабатывает, сообщая о заражении этого приложения
 */


using TcpChatServer;
using TcpChatServer.Models;

var server = new ServerObject();
var listenTask = new Task(server.Listen);

Console.Title = "Задание на 06.11.2026 - cервер для чата по протоколу TCP";
Console.SetWindowSize(42, 25);

try {
    listenTask.Start();

    Console.ReadKey(true);
}
catch (Exception ex) {
    Console.WriteLine(ex.Message);
}
finally {
    server.Disconnect();
} // try-catch-finally
