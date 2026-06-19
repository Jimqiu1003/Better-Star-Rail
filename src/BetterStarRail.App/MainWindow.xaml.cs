using System.Windows;
using BetterStarRail.App.ViewModels;

namespace BetterStarRail.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
