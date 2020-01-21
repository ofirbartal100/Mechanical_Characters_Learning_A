using MechanicalCharacters_Learning_A.ViewModels;
using System.Windows.Controls;

namespace MechanicalCharacters_Learning_A.Views
{
    /// <summary>
    /// Interaction logic for FirstStage
    /// </summary>
    public partial class FirstStage : UserControl
    {
        public FirstStage()
        {
            InitializeComponent();
        }

        private void SimilarCurvesTextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var stackPanel = ((StackPanel)(textbox.Parent));
            var idxOfCurve = stackPanel.Children.IndexOf(textbox);
            var idxOfCurvePair = int.Parse(stackPanel.Tag.ToString());
            ((FirstStageViewModel)DataContext).UpdateSimilarCurves(idxOfCurvePair, idxOfCurve, textbox.Text);
        }

        private void DissimilarCurvesTextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            var stackPanel = ((StackPanel)(textbox.Parent));
            var idxOfCurve = stackPanel.Children.IndexOf(textbox);
            var idxOfCurvePair = int.Parse(stackPanel.Tag.ToString());
            ((FirstStageViewModel)DataContext).UpdateDissimilarCurves(idxOfCurvePair, idxOfCurve, textbox.Text);
        }
    }
}