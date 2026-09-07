namespace CNH_Value_Finder
{
    public partial class MainWindowForm : Form
    {
        private int typeDice = 6; // How many sides the dice have
        private int numDice = 1; // How many dice there are
        private int successThreshold = 4; // The lowest possible die face that counts as a Success
        private int difficulty = 0; // The difficulty of the simulated Attempt
        private int valueFinderNumDice = 0; // How many dice are used in the simulated Attempt
        private double successChance = 0; // The overall simulated Attempt's success chance
        private int diceSum = 10; // The sum of the dice to simulate

        // Standard constructor
        public MainWindowForm()
        {
            InitializeComponent();
        }

        // What happens when the 'Run' button is used
        private void runButton_Click(object sender, EventArgs e)
        {

        }

        // Ensures correct functionality when certain tab pages are selected
        private void functionOptionsTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* Notes for the Value Finder tab page:
             *  The 'Number of Dice' text box needs to "move" into the tab page
             *   This is accomplished via copying the common version's settings and resetting and hiding it
             *  The 'Run' button can *only* be enabled if exactly one of the three options is empty
             */
            if (functionOptionsTabControl.SelectedTab == valueFinderTabPage)
            {
                // Copy settings
                numDiceValueFinderTextBox.Text = numDiceTextBox.Text;
                // Reset
                numDiceTextBox.Text = "";
                // Hide
                numDiceTextBox.Visible = false;
                numDiceTextBox.Enabled = false;
                numDiceLabel.Visible = false;
                // Maybe enable 'Run' button, under the right conditions
                runButton.Enabled = false;
                if ((difficulty == 0 & valueFinderNumDice != 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice == 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice != 0 & successChance == 0))
                {
                    runButton.Enabled = true;
                }
            }
            /* When switching off of the Value Finder tab page:
             *  The 'Number of Dice' text box needs to "move" out of the tab page
             *   This is accomplished by copying the settings and resetting the tab version, then showing the common version
             *  The 'Run' button is re-enabled (no other pages need input save for Dice Finder, which has a default value)
             */
            else
            {
                // Copy settings
                if (!string.IsNullOrEmpty(numDiceValueFinderTextBox.Text))
                {
                    numDiceTextBox.Text = numDiceValueFinderTextBox.Text;
                }
                // Reset
                numDiceValueFinderTextBox.Text = "";
                // Show
                numDiceTextBox.Visible = true;
                numDiceTextBox.Enabled = true;
                numDiceLabel.Visible = true;
                // Re-enable 'Run' button
                runButton.Enabled = true;
            }
        }

        // Validation checks for each input text box
        //------------------------------------------

        private void typeDiceTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of d6
                if (string.IsNullOrEmpty(typeDiceTextBox.Text))
                {
                    x = 6;
                }
                // Since we only use the number, strip the 'd' if it exists
                else if (Char.ToLower(typeDiceTextBox.Text[0]) == 'd')
                {
                    x = Int32.Parse(typeDiceTextBox.Text.Substring(1));
                }
                else
                {
                    x = Int32.Parse(typeDiceTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                }
                // Assign variables and enable everything since we passed validation
                // NOTE - Can only edit the Success Threshold textbox once we have a dice type (needed for error-checking)
                typeDice = x;
                successThresholdTextBox.Enabled = true;
                runButton.Enabled = true;
                errorProvider.SetError(typeDiceTextBox, "");
            }
            catch (Exception ex)
            {
                successThresholdTextBox.Enabled = false;
                runButton.Enabled = false;
                errorProvider.SetError(typeDiceTextBox, "Invalid value - Enter an integer greater than 0 optionally preceeded by a 'd'");
            }
        }

        private void numDiceTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of 1 die
                if (string.IsNullOrEmpty(numDiceTextBox.Text))
                {
                    x = 1;
                }
                else
                {
                    x = Int32.Parse(numDiceTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                }
                // Assign variables and enable everything since we passed validation
                numDice = x;
                runButton.Enabled = true;
                errorProvider.SetError(numDiceTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                errorProvider.SetError(numDiceTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void successThresholdTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of the smallest number in the upper half of the dice type
                //  Ex: '4' for 'd6', '5' for 'd8', '11' for 'd20'
                if (string.IsNullOrEmpty(successThresholdTextBox.Text))
                {
                    if (typeDice % 2 == 0)
                    {
                        x = (typeDice / 2) + 1;
                    }
                    else
                    {
                        x = (int)Math.Ceiling((double)typeDice / 2);
                    }
                }
                else
                {
                    x = Int32.Parse(successThresholdTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                }
                else if (x > typeDice)
                {
                    x = typeDice;
                }
                // Assign variables and enable everything since we passed validation
                successThreshold = x;
                runButton.Enabled = true;
                errorProvider.SetError(successThresholdTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                // Customize error message since "Enter an integer between 1 and 1" sounds clunky
                String errorString;
                if (typeDice == 1)
                {
                    errorString = "Invalid value - Must be 1 since you a d1 dice type is being used";
                }
                else
                {
                    errorString = $"Invalid value - Enter an integer between 1 and {typeDice}, inclusive";
                }
                errorProvider.SetError(successThresholdTextBox, errorString);
            }
        }

        private void difficultyValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(difficultyValueFinderTextBox.Text))
                {
                    x = 0;
                }
                else
                {
                    x = Int32.Parse(difficultyValueFinderTextBox.Text);
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x < 1)
                    {
                        x = 1;
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                difficulty = x;
                runButton.Enabled = false;
                if ((difficulty == 0 & valueFinderNumDice != 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice == 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice != 0 & successChance == 0))
                {
                    runButton.Enabled = true;
                }
                errorProvider.SetError(difficultyValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                errorProvider.SetError(difficultyValueFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void numDiceValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(numDiceValueFinderTextBox.Text))
                {
                    x = 0;
                }
                else
                {
                    x = Int32.Parse(numDiceValueFinderTextBox.Text);
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x < 1)
                    {
                        x = 1;
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                valueFinderNumDice = x;
                runButton.Enabled = false;
                if ((difficulty == 0 & valueFinderNumDice != 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice == 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice != 0 & successChance == 0))
                {
                    runButton.Enabled = true;
                }
                errorProvider.SetError(numDiceValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                errorProvider.SetError(numDiceValueFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void successChanceValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                double x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(successChanceValueFinderTextBox.Text))
                {
                    x = 0;
                }
                // If user input has a '%' symbol at the end, strip it and treat the value as a percent (divide by 100)
                else if (successChanceValueFinderTextBox.Text[successChanceValueFinderTextBox.Text.Length - 1] == '%')
                {
                    x = Double.Parse(successChanceValueFinderTextBox.Text.Substring(0, successChanceValueFinderTextBox.Text.Length - 1));
                    x = x / 100;
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x <= 0)
                    {
                        x = 0.01;
                    }
                    else if (x >= 1)
                    {
                        x = 0.99;
                    }
                }
                else
                {
                    x = Double.Parse(successChanceValueFinderTextBox.Text);
                    // If user input doesn't have a '%' at the end, assume values >= 1 are percentage form and treat them as such (divide by 100)
                    if (x >= 1)
                    {
                        x = x / 100;
                    }
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x <= 0)
                    {
                        x = 0.01;
                    }
                    else if (x >= 1)
                    {
                        x = 0.99;
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                successChance = x;
                runButton.Enabled = false;
                if ((difficulty == 0 & valueFinderNumDice != 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice == 0 & successChance != 0) ||
                    (difficulty != 0 & valueFinderNumDice != 0 & successChance == 0))
                {
                    runButton.Enabled = true;
                }
                errorProvider.SetError(successChanceValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                errorProvider.SetError(successChanceValueFinderTextBox, "Invalid value - Enter a number between 0 and 100, exclusive, optionally followed by a '%'");
            }
        }

        private void sumDiceFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of a dice sum of 1
                if (string.IsNullOrEmpty(sumDiceFinderTextBox.Text))
                {
                    x = 1;
                }
                else
                {
                    x = Int32.Parse(sumDiceFinderTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                }
                // Assign variables and enable everything since we passed validation
                diceSum = x;
                runButton.Enabled = true;
                errorProvider.SetError(sumDiceFinderTextBox, "");
            }
            catch (Exception ex)
            {
                runButton.Enabled = false;
                errorProvider.SetError(sumDiceFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }
    }
}
