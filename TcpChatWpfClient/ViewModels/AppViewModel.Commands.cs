using System.Windows;

namespace TcpChatWpfClient.ViewModels;

// Команды клиента (выполняются на клиенте, не передаются на сервер)
public partial class AppViewModel
{
    // сведения о разработчике
    private const string Developer = "<ФИО>, группа <номер>";


    // Выбор команды клиента по имени
    private void ExecuteClientCommand(string command) {
        switch (command.ToLower()) {
            case "@help":  Help();  break;
            case "@about": About(); break;
            case "@clear": Clear(); break;
            case "@exit":  Exit();  break;

            default:
                WriteLine($"{command}: неизвестная команда, список команд: @help");
                break;
        } // switch
    } // ExecuteClientCommand


    // @help - вывод списка всех команд
    private void Help() => WriteLine("""
        Команды клиента (выполняются на клиенте, на сервер не передаются):
            @help   - вывод списка всех команд
            @about  - сведения о приложении и разработчике
            @clear  - очистка области вывода
            @exit   - завершение работы клиента, сервер продолжает работу
        Первая введенная строка - ваше имя в чате, следующие - сообщения участникам чата.
        """);


    // @about - сведения о приложении и разработчике
    private void About() => WriteLine($"""
        Клиент TCP-чата, WPF-приложение
        Задание на 06.11.2026
        Разработчик: {Developer}
        КА ШАГ, Донецк, 2026
        """);


    // @clear - очистка области вывода (TextBlock)
    private void Clear() => ChatLog = "";


    // @exit - завершение работы клиента без завершения работы сервера
    private void Exit() {
        Disconnect();
        Application.Current.Shutdown();
    } // Exit
} // class AppViewModel
