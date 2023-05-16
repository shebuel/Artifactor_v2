using Artifactor_v2.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Artifactor_v2.Views;

public sealed partial class ChecklistPage : Page
{
    public ChecklistViewModel ViewModel
    {
        get;
    }

    public ChecklistPage()
    {
        ViewModel = App.GetService<ChecklistViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
    }

}
