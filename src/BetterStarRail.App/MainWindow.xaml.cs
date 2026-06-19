using System.Windows;
using BetterStarRail.App.ViewModels;

namespace BetterStarRail.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
