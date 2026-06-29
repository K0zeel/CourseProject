using System.Windows;

namespace DPWrestlingScoreboard.Windows
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
            ApplyWrestlerName(redWrestlerTextBlock, redWrestler);
            ApplyWrestlerName(blueWrestlerTextBlock, blueWrestler);
        }

        private static void ApplyWrestlerName(System.Windows.Controls.TextBlock block, string name)
        {
            block.Text = name;
            block.ToolTip = name.Length > 24 ? name : null;
            block.FontSize = name.Length switch
            {
                > 42 => 26,
                > 32 => 34,
                > 24 => 40,
                _ => 48
            };
        }

        public void UpdateWeightCategory(string weight)
        {
            weightTextBlock.Text = weight;
        }

        public void UpdateStage(string stage)
        {
            stageTextBlock.Text = stage;
            stageTextBlock.FontSize = stage.Length switch
            {
                > 16 => 32,
                > 12 => 40,
                _ => 48
            };
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
