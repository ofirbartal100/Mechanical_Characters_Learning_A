using MechanicalCharacters_Learning_A.Utils;
using MechanicalCharacters_Learning_A.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class SecondStageViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Random _probabilityRandom = new Random();
        private int _submitCounter = 0;
        private ObservableCollection<CurvePairControl> _threeCurvePairControls;
        private List<CurvesPair> _threeCurvePairs;

        public SecondStageViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            OKButtonClickCommand = new DelegateCommand(OKButtonClick);

            ReloadCurvePairs();
        }

        public ICommand OKButtonClickCommand { get; set; }

        public int SubmitCounter
        {
            get { return _submitCounter; }
            set { SetProperty(ref _submitCounter, value); }
        }

        public ObservableCollection<CurvePairControl> ThreeCurvePairControls
        {
            get { return _threeCurvePairControls; }
            set { SetProperty(ref _threeCurvePairControls, value); }
        }

        public List<CurvesPair> ThreeCurvePairs
        {
            get { return _threeCurvePairs; }
            set { SetProperty(ref _threeCurvePairs, value); }
        }

        private CurvesPair[] GenerateThreeRandomCurves()
        {
            var randomCurves = new[] { new Curve(), new Curve(), new Curve() };
            List<Task> tasks = new List<Task>
            {
                F(randomCurves, 0),
                F(randomCurves, 1),
                F(randomCurves, 2)
            };
            Task.WaitAll(tasks.ToArray());

            CurvesPair[] randomPairs = new CurvesPair[]
            {
                SQP.Instance.FindPairWithinDistanceBounds(randomCurves[0],0,0.3),
                SQP.Instance.FindPairWithinDistanceBounds(randomCurves[1],0,0.75),
                SQP.Instance.FindPairWithinDistanceBounds(randomCurves[2],0,9999999)
            };
            return randomPairs;

            Task F(Curve[] a, int b)
            {
                return Task.Factory.StartNew(() =>
                {
                    a[b] = CurveGenerator.GenerateRandomCurve();
                });
            }
        }

        private void OKButtonClick()
        {
            //update CurvePairs
            ThreeCurvePairs = new List<CurvesPair>(
                ThreeCurvePairControls
                .Select(control => ((CurvePairControlViewModel)control.DataContext).Pair)
                .ToArray()
            );

            List<CurvesPair> _simillarCurvesList, _dissimillarCurvesList;
            SelectionIntoSimilarAndDissimilar(out _simillarCurvesList, out _dissimillarCurvesList);

            SQP.Instance.Optimize(_simillarCurvesList, _dissimillarCurvesList);
            _eventAggregator.GetEvent<NewWeightsEvent>().Publish();

            ReloadCurvePairs();

            SubmitCounter++;
        }

        /// <summary>
        /// generate new curve pairs
        /// </summary>
        private void ReloadCurvePairs()
        {
            CurvesPair[] randomPairs = GenerateThreeRandomCurves();
            ThreeCurvePairs = new List<CurvesPair>(randomPairs);

            //update the pairs controls
            ThreeCurvePairControls = new ObservableCollection<CurvePairControl>();
            foreach (var curvePair in randomPairs)
            {
                var control = new CurvePairControl();
                ((CurvePairControlViewModel)control.DataContext).SetCurvePaire(curvePair);
                ThreeCurvePairControls.Add(control);
            }
        }

        private void SelectionIntoSimilarAndDissimilar(out System.Collections.Generic.List<CurvesPair> _simillarCurvesList, out System.Collections.Generic.List<CurvesPair> _dissimillarCurvesList)
        {
            _simillarCurvesList = ThreeCurvePairs.Where(pair =>
            {
                bool isTakeProbablySimilar = false;
                if (pair.SimilarityType == CurveSimilarity.ProbablySimilar)
                {
                    isTakeProbablySimilar |= (_probabilityRandom.NextDouble() < 0.75);
                }
                else if (pair.SimilarityType == CurveSimilarity.ProbablyDissimilar)
                {
                    isTakeProbablySimilar |= (_probabilityRandom.NextDouble() < 0.25);
                }
                return pair.SimilarityType == CurveSimilarity.Similar || isTakeProbablySimilar;
            }).ToList();
            _dissimillarCurvesList = ThreeCurvePairs.Where(pair =>
            {
                bool isTakeProbablySimilar = false;
                if (pair.SimilarityType == CurveSimilarity.ProbablyDissimilar)
                {
                    isTakeProbablySimilar |= (_probabilityRandom.NextDouble() < 0.75);
                }
                else if (pair.SimilarityType == CurveSimilarity.ProbablySimilar)
                {
                    isTakeProbablySimilar |= (_probabilityRandom.NextDouble() < 0.25);
                }
                return pair.SimilarityType == CurveSimilarity.Dissimilar || isTakeProbablySimilar;
            }).ToList();
        }
    }
}