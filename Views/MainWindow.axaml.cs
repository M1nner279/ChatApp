using Avalonia.Controls;
using ChatApp.ViewModels;

namespace ChatApp.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}