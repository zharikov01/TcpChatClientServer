using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using TcpChatWpfClient.Infrastructure;
using TcpChatWpfClient.Views;

namespace TcpChatWpfClient.ViewModels;

// Модель представления главного окна: подключение к серверу чата,
// отправка и прием сообщений
public partial class AppViewModel : INotifyPropertyChanged
{
    // окно, связанное с моделью представления
    public MainWindow HostWindow { get; }

    // конечная точка сервера чата
    public string Host { get; } = "127.0.0.1";
    public int Port { get; } = 8888;

    // подключение к серверу
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private string _userName = "";

    public bool IsConnected => _stream is not null;


    #region свойства для привязки

    // строка, введенная в TextBox: сообщение или команда клиента
    private string _message = "";
    public string Message {
        get => _message;
        set { _message = value; OnPropertyChanged(); }
    } // Message

    // все сообщения сервера и команд клиента, выводятся в TextBlock
    private string _chatLog = "";
    public string ChatLog {
        get => _chatLog;
        private set { _chatLog = value; OnPropertyChanged(); }
    } // ChatLog

    // состояние подключения
    private string _status = "";
    public string Status {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    } // Status

    #endregion


    // Отправить введенную строку (кнопка "Отправить" или Enter)
    public RelayCommand SendCommand { get; }


    public AppViewModel(MainWindow hostWindow) {
        HostWindow = hostWindow;

        SendCommand = new(async _ => await SendAsync(), _ => !string.IsNullOrWhiteSpace(Message));

        Status = "Нет подключения к серверу";
        WriteLine("Добро пожаловать в TCP-чат!");
        WriteLine("Введите ваше имя для чата и нажмите Enter. Список команд: @help");
    } // AppViewModel


    // Обработка введенной строки
    private async Task SendAsync() {
        var text = Message.Trim();
        Message = "";
        HostWindow.TbxMessage.Focus();

        if (text.Length == 0) {
            return;
        } // if

        try {
            // команды клиента выполняются на клиенте, на сервер не передаются
            if (text.StartsWith('@')) {
                ExecuteClientCommand(text);
                return;
            } // if

            // пока нет подключения, введенная строка - имя пользователя в чате
            if (!IsConnected) {
                await ConnectAsync(text);
                return;
            } // if

            await WriteToServerAsync(text);
            WriteLine($"Вы: {text}");
        } catch (Exception ex) {
            WriteLine($"Ошибка: {ex.Message}");
            Disconnect();
        } // try-catch
    } // SendAsync


    // Подключение к серверу, первое сообщение серверу - имя клиента в чате
    private async Task ConnectAsync(string userName) {
        try {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(Host, Port);
            _stream = _tcpClient.GetStream();
        } catch (SocketException ex) {
            Disconnect();
            WriteLine($"Сервер {Host}:{Port} недоступен: {ex.Message}");
            WriteLine("Запустите сервер и введите имя еще раз");
            return;
        } // try-catch

        _userName = userName;
        await WriteToServerAsync(_userName);

        Status = $"{_userName} в чате, сервер {Host}:{Port}";
        WriteLine($"Добро пожаловать, {_userName}!");

        // прием сообщений от сервера - асинхронно, интерфейс не блокируется
        _ = ReceiveMessagesAsync();
    } // ConnectAsync


    // Отправка строки серверу в кодировке UTF-8
    private async Task WriteToServerAsync(string text) {
        byte[] data = Encoding.UTF8.GetBytes(text);
        await _stream!.WriteAsync(data);
    } // WriteToServerAsync


    // Прием сообщений от сервера до разрыва подключения.
    // После await выполнение продолжается в потоке интерфейса,
    // поэтому выводить сообщения в TextBlock можно напрямую
    private async Task ReceiveMessagesAsync() {
        var stream = _stream!;
        var buffer = new byte[256];

        try {
            while (true) {
                // сообщение может прийти несколькими порциями - собираем все
                // байты и только потом декодируем строку
                using var message = new MemoryStream();
                do {
                    int bytes = await stream.ReadAsync(buffer);

                    // 0 байтов - сервер закрыл соединение
                    if (bytes == 0) {
                        throw new IOException("Сервер закрыл соединение");
                    } // if

                    message.Write(buffer, 0, bytes);
                } while (stream.DataAvailable);

                WriteLine(Encoding.UTF8.GetString(message.ToArray()).TrimEnd('\0'));
            } // while
        } catch {
            // если подключение закрыл сам клиент, сообщать не о чем
            if (ReferenceEquals(stream, _stream)) {
                WriteLine("Подключение к серверу прервано!");
                Disconnect();
                WriteLine("Для повторного подключения введите имя");
            } // if
        } // try-catch
    } // ReceiveMessagesAsync


    // Закрытие подключения к серверу
    public void Disconnect() {
        _stream?.Close();
        _tcpClient?.Close();
        _stream = null;
        _tcpClient = null;

        Status = "Нет подключения к серверу";
    } // Disconnect


    // Вывод строки в TextBlock с прокруткой к последней строке
    public void WriteLine(string text) {
        ChatLog += $"{(ChatLog.Length == 0 ? "" : "\n")}[{DateTime.Now:HH:mm:ss}] {text}";
        HostWindow.SvwChat.ScrollToBottom();
    } // WriteLine


    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
} // class AppViewModel
