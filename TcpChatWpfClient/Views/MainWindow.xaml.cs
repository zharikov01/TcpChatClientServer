using System.Windows;
using TcpChatWpfClient.ViewModels;

namespace TcpChatWpfClient.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow() {
        InitializeComponent();

        // привязка данных окна к классу AppViewModel,
        // реализация связи между View и ViewModel
        var viewModel = new AppViewModel(this);
        DataContext = viewModel;

        // при закрытии окна закрываем подключение к серверу
        Closing += (_, _) => viewModel.Disconnect();
    } // MainWindow
} // class MainWindow
