using MechanicalCharacters_Learning_A.Utils;
using MechanicalCharacters_Learning_A.Views;
using Prism.Mvvm;
using System.Collections.Generic;

namespace MechanicalCharacters_Learning_A.ViewModels
{
    public class CurvePairControlViewModel : BindableBase
    {
        private List<CurveControl> _curveControlList;

        private CurvesPair _pair;

        public CurvePairControlViewModel()
        {
        }

        public List<CurveControl> CurveControlList
        {
            get { return _curveControlList; }
            set { SetProperty(ref _curveControlList, value); }
        }

        public CurvesPair Pair
        {
            get { return _pair; }
            set { SetProperty(ref _pair, value); }
        }

        public void SetCurvePaire(CurvesPair curvesPair)
        {
            Pair = curvesPair;
            CurveControlList = new List<CurveControl>();
            var control = new CurveControl();
            ((CurveControlViewModel)control.DataContext).SetCurve(curvesPair.CurveA);
            CurveControlList.Add(control);

            var control2 = new CurveControl();
            ((CurveControlViewModel)control2.DataContext).SetCurve(curvesPair.CurveB);
            CurveControlList.Add(control2);
        }
    }
}