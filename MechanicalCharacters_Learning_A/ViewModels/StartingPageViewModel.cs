using MechanicalCharacters_Learning_A.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Windows.Input;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class StartingPageViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private string _aContent;

        private int _fontsize;

        public StartingPageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<NewWeightsEvent>().Subscribe(NewWeightsEventHandler);
            NewWeightsEventHandler();

            _regionManager = regionManager;
            var d = new Dictionary<string, string>
            {
                ["Workspace"] = "StartStage"
            };
            _regionManager.Navigate(d);

            Fontsize = 13;

            SaveSQPStateButtonClickCommand = new DelegateCommand(SaveSQPStateButtonClick);
        }

        public string AContent
        {
            get { return _aContent; }
            set { SetProperty(ref _aContent, value); }
        }
        public int Fontsize
        {
            get { return _fontsize; }
            set { SetProperty(ref _fontsize, value); }
        }

        public ICommand SaveSQPStateButtonClickCommand { get; set; }
        private void NewWeightsEventHandler()
        {
            var A = SQP.Instance.A;
            AContent = $"({A[0]:F4}, {A[1]:F4}, {A[2]:F4}, {A[3]:F4}, {A[4]:F4}, {A[5]:F4}, {A[6]:F4}, {A[7]:F4})";
        }

        private void SaveSQPStateButtonClick()
        {
            Services.Tracker.Persist(SQP.Instance);
        }
    }
}