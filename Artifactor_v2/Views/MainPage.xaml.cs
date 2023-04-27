using Artifactor_v2.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Artifactor_v2.Views;


public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get; 
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
        
        
    }

    /*[ObservableProperty]
    public string TesterName;*/
    

    

}
