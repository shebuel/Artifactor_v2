using Artifactor_v2.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Artifactor_v2.Views;

public sealed partial class AboutPage : Page
{
    public AboutViewModel ViewModel
    {
        get;
    }

    public AboutPage()
    {
        ViewModel = App.GetService<AboutViewModel>();
        InitializeComponent();
    }
}
