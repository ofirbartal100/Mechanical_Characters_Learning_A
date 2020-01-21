using MechanicalCharacters_Learning_A.Utils;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Windows.Input;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class StartStageViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public StartStageViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            StartButtonClickCommand = new DelegateCommand(StartButtonClick);
        }

        public ICommand StartButtonClickCommand { get; set; }
        private void StartButtonClick()
        {
            var d = new Dictionary<string, string>
            {
                ["Workspace"] = "FirstStage"
            };
            _regionManager.Navigate(d);
        }
    }
}