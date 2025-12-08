using System.Windows;

namespace KPWrestlingScoreboard.Windows
{
    public partial class ScoreboardWindow : Window
    {
        public ScoreboardWindow()
        {
            InitializeComponent();
        }

        public void UpdateScore(int redScore, int blueScore)
        {
            redScoreTextBlock.Text = redScore.ToString();
            blueScoreTextBlock.Text = blueScore.ToString();
        }

        public void UpdateTimer(string time)
        {
            timerTextBlock.Text = time;
        }

        public void UpdatePeriod(int period)
        {
            periodTextBlock.Text = period.ToString();
        }

        public void UpdateWrestlers(string redWrestler, string blueWrestler)
        {
            redWrestlerTextBlock.Text = redWrestler;
            blueWrestlerTextBlock.Text = blueWrestler;
        }

        public void UpdateWeightCategory(string weight)
        {
            weightTextBlock.Text = weight;
        }

        public void UpdateStage(string stage)
        {
            stageTextBlock.Text = stage;
        }

        public void UpdateStyle(string style)
        {
            styleTextBlock.Text = style;
        }

        public void ShowRedWinner(bool show)
        {
            redWinnerBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            blueWinnerBorder.Visibility = Visibility.Collapsed;
        }

        public void ShowBlueWinner(bool show)
        {
            blueWinnerBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            redWinnerBorder.Visibility = Visibility.Collapsed;
        }

        public void ShowBreak(bool show, int seconds = 30)
        {
            breakBorder.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            if (show)
            {
                breakStatusTextBlock.Text = $"ПЕРЕРЫВ {seconds} сек";
            }
        }

        public void HideWinners()
        {
            redWinnerBorder.Visibility = Visibility.Collapsed;
            blueWinnerBorder.Visibility = Visibility.Collapsed;
        }
    }
}
