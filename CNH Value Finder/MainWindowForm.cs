namespace CNH_Value_Finder
{
    public partial class MainWindowForm : Form
    {
        public MainWindowForm()
        {
            InitializeComponent();
        }

        private void runButton_Click(object sender, EventArgs e)
        {

        }

        private void valueFinderUsageTextLabel_Click(object sender, EventArgs e)
        {

        }

        private void functionOptionsTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (functionOptionsTabControl.SelectedTab == valueFinderTabPage)
            {
                numDiceValueFinderTextBox.Text = numDiceTextBox.Text;
                numDiceTextBox.Text = "";
                numDiceTextBox.Visible = false;
                numDiceTextBox.Enabled = false;
                numDiceLabel.Visible = false;
            } else
            {
                if (numDiceValueFinderTextBox.Text != "")
                {
                    numDiceTextBox.Text = numDiceValueFinderTextBox.Text;
                }
                numDiceValueFinderTextBox.Text = "";
                numDiceTextBox.Visible = true;
                numDiceTextBox.Enabled = true;
                numDiceLabel.Visible = true;
            }
        }
    }
}
