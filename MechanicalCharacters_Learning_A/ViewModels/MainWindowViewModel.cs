using MechanicalCharacters_Learning_A.Utils;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Computational Design Of MEchanical Characters - Training A";

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            var d = new Dictionary<string, string>
            {
                ["ContentRegion"] = "StartingPage"
            };
            regionManager.Navigate(d);

            Services.Tracker.Track(SQP.Instance);
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}