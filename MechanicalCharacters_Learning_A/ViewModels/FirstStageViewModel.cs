using MechanicalCharacters_Learning_A.Utils;
using MechanicalCharacters_Learning_A.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class FirstStageViewModel : BindableBase
    {
        private List<CurveControl> _curveControlList;
        private List<Curve> _curvesList;

        private List<CurvesPair> _dissimillarCurvesList;

        private IEventAggregator _eventAggregator;

        private IRegionManager _regionManager;

        private List<CurvesPair> _simillarCurvesList;

        public FirstStageViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            CurvesList = new List<Curve>() {
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                new Curve(),
                };

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < CurvesList.Count; i++)
            {
                tasks.Add(F(CurvesList, i));
            }
            Task.WaitAll(tasks.ToArray());

            CurveControlList = new List<CurveControl>();
            foreach (var curve1 in CurvesList)
            {
                var control = new CurveControl();
                ((CurveControlViewModel)control.DataContext).SetCurve(curve1);
                CurveControlList.Add(control);
            }

            SimillarCurvesList = new List<CurvesPair>() { new CurvesPair(true), new CurvesPair(true), new CurvesPair(true), new CurvesPair(true), new CurvesPair(true) };
            DissimillarCurvesList = new List<CurvesPair>() { new CurvesPair(false), new CurvesPair(false), new CurvesPair(false), new CurvesPair(false), new CurvesPair(false) };
            ContinueButtonClickCommand = new DelegateCommand(ContinueButtonClick);

            Task F(List<Curve> a, int b)
            {
                return Task.Factory.StartNew(() =>
                {
                    a[b] = CurveGenerator.GenerateRandomCurve();
                });
            }
        }

        public ICommand ContinueButtonClickCommand { get; set; }

        public List<CurveControl> CurveControlList
        {
            get { return _curveControlList; }
            set { SetProperty(ref _curveControlList, value); }
        }

        public List<Curve> CurvesList
        {
            get { return _curvesList; }
            set { SetProperty(ref _curvesList, value); }
        }

        public List<CurvesPair> DissimillarCurvesList
        {
            get { return _dissimillarCurvesList; }
            set { SetProperty(ref _dissimillarCurvesList, value); }
        }

        public List<CurvesPair> SimillarCurvesList
        {
            get { return _simillarCurvesList; }
            set { SetProperty(ref _simillarCurvesList, value); }
        }

        public List<int> SlotNumbers { get; set; } = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        public void UpdateDissimilarCurves(int idxOfCurvePair, int idxOfCurve, string text)
        {
            //parse the string to int
            if (int.TryParse(text, out int curveIdx) && curveIdx >= 0 && curveIdx < CurvesList.Count)
            {
                if (idxOfCurve == 0) //index of textbox child
                    DissimillarCurvesList[idxOfCurvePair].CurveA = CurvesList[curveIdx];
                else if (idxOfCurve == 2) //index of textbox child
                    DissimillarCurvesList[idxOfCurvePair].CurveB = CurvesList[curveIdx];
            }
        }

        public void UpdateSimilarCurves(int idxOfCurvePair, int idxOfCurve, string text)
        {
            //parse the string to int
            if (int.TryParse(text, out int curveIdx) && curveIdx >= 0 && curveIdx < CurvesList.Count)
            {
                if (idxOfCurve == 0) //index of textbox child
                    SimillarCurvesList[idxOfCurvePair].CurveA = CurvesList[curveIdx];
                else if (idxOfCurve == 2) //index of textbox child
                    SimillarCurvesList[idxOfCurvePair].CurveB = CurvesList[curveIdx];
            }
        }

        private void ContinueButtonClick()
        {
            SimillarCurvesList.ForEach(pair => pair.CalculateFeatures());
            DissimillarCurvesList.ForEach(pair => pair.CalculateFeatures());
            SQP.Instance.Optimize(SimillarCurvesList, DissimillarCurvesList);
            _eventAggregator.GetEvent<NewWeightsEvent>().Publish();

            var d = new Dictionary<string, string>
            {
                ["Workspace"] = "SecondStage"
            };
            _regionManager.Navigate(d);
        }
    }
}