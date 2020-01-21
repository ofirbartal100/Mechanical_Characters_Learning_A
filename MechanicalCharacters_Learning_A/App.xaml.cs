using System;
using MechanicalCharacters_Learning_A.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using MechanicalCharacters_Learning_A.Utils;

namespace MechanicalCharacters_Learning_A
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register(typeof(Object), typeof(StartingPage), "StartingPage");
            containerRegistry.Register(typeof(Object), typeof(StartStage), "StartStage");
            containerRegistry.Register(typeof(Object), typeof(FirstStage), "FirstStage");
            containerRegistry.Register(typeof(Object), typeof(SecondStage), "SecondStage");
        }
    }
}
