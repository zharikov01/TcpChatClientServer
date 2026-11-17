/*
 * Задание на 06.11.2026 - сервер для чата на протоколе TCP (из классной работы)
 * При первом запуске разрешить доступ к сети для этого приложения
 * Антивирус ошибочно срабатывает, сообщая о заражении этого приложения
 */


using TcpChatServer;
using TcpChatServer.Models;

var server = new ServerObject();
var listenTask = new Task(server.Listen);

Console.Title = "Задание на 06.11.2026 - cервер для чата по протоколу TCP";

// размер окна консоли можно задать только в Windows
if (OperatingSystem.IsWindows()) {
    Console.SetWindowSize(60, 25);
} // if

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
