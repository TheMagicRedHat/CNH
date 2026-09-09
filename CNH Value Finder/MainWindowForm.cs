using ExtendedNumerics;
using System.Numerics;

namespace CNH_Value_Finder
{
    public partial class MainWindowForm : Form
    {
        private int typeDice = 6; // How many sides the dice have
        private int defaultTypeDice = 6;
        private int numDice = 1; // How many dice there are
        private int defaultNumDice = 1;
        private int successThreshold = 4; // The lowest possible die face that counts as a Success
        private int defaultSuccessThreshold = 4;
        private int difficulty = 0; // The difficulty of the simulated Attempt
        private int defaultDifficulty = 0;
        private int valueFinderNumDice = 0; // How many dice are used in the simulated Attempt
        private int defaultValueFinderNumDice = 0;
        private double successChance = 0; // The overall simulated Attempt's success chance
        private double defaultSuccessChance = 0;
        private int diceSum = 10; // The sum of the dice to simulate
        private int defaultDiceSum = 10;

        // Standard constructor
        public MainWindowForm()
        {
            InitializeComponent();
        }

        /* Calculates the number of ways to choose k items from n total items where order doesn't matter
         * Implementation of the Combination (or Binomial Coefficient) mathematical function
         * @param n <int> The total number of items to 'choose' from
         * @param k <int> The number of items 'chosen'
         * @return <BigInteger> The number of ways to pick k items from n total items (order doesn't matter)
         */
        private static BigInteger Choose(BigInteger n, BigInteger k)
        {
            // For invalid combinations, return 0 immediately
            if (k > n || n < 0 || k < 0)
            {
                return 0;
            }
            if (k == 0 || k == n)
            {
                return 1;
            }
            // Otherwise calculate the number of combinations
            BigInteger returnValue = 1;
            // Can use symmetry of combinations around n-k for faster computation
            if (k > n - k)
            {
                k = n - k;
            }
            // Perform the combination math
            for (BigInteger i = 1; i <= k; i++)
            {
                returnValue *= n - k + i;
                returnValue /= i;
            }
            return returnValue;
        }

        /* Calculates an average value of a probability distribution array where each element
         *  is the probability of its own position
         * @param probabilities <double[]> The probability distribution array to find the average of
         * @return <double> The average value of the probability distribution array
         */
        private static double Average(double[] probabilities)
        {
            // Average value can be found by summing the products of values with their probabilities
            double average = 0.0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                average += probabilities[i] * i;
            }
            return average;
        }

        /* Calculates a standard deviation value of a probability distribution array where each element
         *  is the probability of its own position
         * @param probabilities <double[]> The probability distribution array to find the standard deviation of
         * @return <double> The standard deviation value of the probability distribution array
         */
        private static double StandardDeviation(double[] probabilities)
        {
            // Standard deviation value can be found by taking the square root of
            //  the sum of
            //   the products of an element's probability and
            //    the squares of
            //     the differences between each value and the average
            // NOTE - Roughly 68% of values are within 1 standard deviation
            //        Roughly 95% of values are within 2 standard deviations
            double average = Average(probabilities);
            double stdDeviation = 0.0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                stdDeviation += probabilities[i] * Math.Pow(i - average, 2);
            }
            return Math.Pow(stdDeviation, 0.5);
        }

        /* Generate an array of the probability distribution for the range of possible sums with the current number of dice
         * @return <double[]> The array of probabilities where the position of each value corresponds to a total sum
         */
        private double[] GenerateSumDistribution()
        {
            // Establish the base probability distribution (ignore Advantage/Disadvantage)
            double[] baseDistribution = new double[(typeDice * numDice) + 1];
            for (int sum = 0; sum < baseDistribution.Length; sum++)
            {
                // Initialize value to 0
                // The first set of values stay 0 since you can't roll a sum less than the number of dice used
                baseDistribution[sum] = 0.0;
                if (sum >= numDice)
                {
                    // Value is the number of combinations that yield the exact sum divided by the total number of possible sums
                    // NOTE - There is a fairly easy recursive implementation for finding the number of combinations for an exact sum
                    //        BUT, since this number grows *very* quickly, it is *far* better to use the derived formula
                    //        The only downside is that the formula is unintuitive and somewhat inscrutable
                    //        This is mostly due to the fact that each die can only contribute up to <largest face value> to the sum
                    BigInteger numerator = 0;
                    for (int i = 0; i <= Math.Floor((double)(sum - numDice) / typeDice); i++)
                    {
                        numerator += BigInteger.Pow(-1, i) * Choose(numDice, i) * Choose(sum - 1 - (typeDice * i), numDice - 1);
                    }
                    BigInteger denominator = BigInteger.Pow(typeDice, numDice);
                    BigRational probability = new BigRational(numerator, denominator);
                    baseDistribution[sum] = Math.Round((double)probability, 15);
                }
            }
            // If there's no Advantage/Disadvantage, then we're done
            if (NeutralRadioButton.Checked == true)
            {
                return baseDistribution;
            }
            // If there's Advantage/Disadvantage, we need a new distribution
            double[] finalDistribution = new double[(typeDice * numDice) + 1];
            // For Advantage, the new distribution for any given sum is:
            //  The probability of rolling that sum twice (base probability squared)
            //  PLUS
            //   Twice (one for each overall roll) the probability of rolling that sum
            //   MULTIPLIED BY
            //   The sum of the probabilities of all valid fewer dice sums (which would be replaced)
            if (AdvantageRadioButton.Checked == true)
            {
                for (int i = 0; i < finalDistribution.Length; i++)
                {
                    if (i >= numDice)
                    {
                        double value = (Math.Pow(baseDistribution[i], 2)) + (2 * baseDistribution[i] * baseDistribution[numDice..i].Sum());
                        finalDistribution[i] = Math.Round(value, 15);
                    }
                }
            }
            // For Disadvantage, the new distribution for any given sum is:
            //  The probability of rolling that sum twice (base probability squared)
            //  PLUS
            //   Twice (one for each overall roll) the probability of rolling that sum
            //   MULTIPLIED BY
            //   The sum of the probabilities of all valid greater dice sums (which would be replaced)
            else
            {
                for (int i = 0; i < finalDistribution.Length; i++)
                {
                    if (i >= numDice)
                    {
                        double value = (Math.Pow(baseDistribution[i], 2)) + (2 * baseDistribution[i] * baseDistribution[(i + 1)..].Sum());
                        finalDistribution[i] = Math.Round(value, 15);
                    }
                }
            }
            return finalDistribution;
        }

        /* Generate an array of the probability distribution for the range of possible Successes with the current number of dice
         * @return <double[]> The array of probabilities where the position of each value corresponds to that many successes
         */
        private double[] GenerateSuccessDistribution()
        {
            // Odds that any single die roll is a Success
            BigRational successProbability = new BigRational((typeDice + 1 - successThreshold), typeDice);
            // Establish the base probability distribution (ignore Advantage/Disadvantage)
            double[] baseDistribution = new double[numDice + 1];
            for (int i = 0; i < baseDistribution.Length; i++)
            {
                // Value is standard binomial probability formula for each amount of Successes
                BigRational value = new BigRational(Choose(numDice, i) * (BigRational.Pow(successProbability, i)) * (BigRational.Pow(1 - successProbability, numDice - i)));
                baseDistribution[i] = Math.Round((double)value, 15);
            }
            // If there's no Advantage/Disadvantage, then we're done
            if (NeutralRadioButton.Checked == true)
            {
                return baseDistribution;
            }
            // If there's Advantage/Disadvantage, we need a new distribution
            double[] finalDistribution = new double[numDice + 1];
            // For Advantage, the new distribution for any given amount of successes is:
            //  The probability of rolling that many successes twice (base probability squared)
            //  PLUS
            //   Twice (one for each Attempt roll) the probability of rolling that many successes
            //   MULTIPLIED BY
            //   The sum of the probabilities of all fewer numbers of successes (which would be replaced)
            if (AdvantageRadioButton.Checked == true)
            {
                for (int i = 0; i < finalDistribution.Length; i++)
                {
                    double value = (Math.Pow(baseDistribution[i], 2)) + (2 * baseDistribution[i] * baseDistribution[..i].Sum());
                    finalDistribution[i] = Math.Round(value, 15);
                }
            }
            // For Disadvantage, the new distribution for any given amount of successes is:
            //  The probability of rolling that many successes twice (base probability squared)
            //  PLUS
            //   Twice (one for each Attempt roll) the probability of rolling that many successes
            //   MULTIPLIED BY
            //   The sum of the probabilities of all greater numbers of successes (which would be replaced)
            else
            {
                for (int i = 0; i < finalDistribution.Length; i++)
                {
                    double value = (Math.Pow(baseDistribution[i], 2)) + (2 * baseDistribution[i] * baseDistribution[(i + 1)..].Sum());
                    finalDistribution[i] = Math.Round(value, 15);
                }
            }
            return finalDistribution;
        }

        // All of the code for simulating dice rolls
        private void RunDiceRoller()
        {
            Random rand = new Random();
            string sumOutputText1 = "-----\nRolls\n";
            string sumOutputText2 = "-----\nRolls\n";
            string successOutputText1 = "-----\nRolls\n";
            string successOutputText2 = "-----\nRolls\n";
            int sum1 = 0;
            int sum2 = 0;
            int successes1 = 0;
            int successes2 = 0;
            bool critSuccess = false;
            bool ogCritSuccess = false;
            bool critFail = false;
            bool ogCritFail = false;
            // Need to roll every dice an extra time when doing Advantage/Disadvantage
            //  So, multiply the number of loops by 2 when doing Advantage/Disadvantage
            int loopAmount = 1;
            if (NeutralRadioButton.Checked != true)
            {
                loopAmount = 2;
            }
            // For each die that needs to be rolled
            // NOTE - Need to loop through the whole thing again when doing Advantage/Disadvantage
            //        Since Advantage/Disadvantage apply to the roll as a whole, we *cannot* just roll
            //         each die twice and pick the better/worse result
            for (int i = 0; i < numDice * loopAmount; i++)
            {
                // Reset the crit flags when starting a new set of dice rolls
                // Also remembers if first set of rolls crit
                if (i == numDice)
                {
                    ogCritSuccess = critSuccess;
                    ogCritFail = critFail;
                    critSuccess = false;
                    critFail = false;
                }
                // Roll the die
                int roll;
                // When using one-sided dice, no need for random
                if (typeDice == 1)
                {
                    roll = 1;
                }
                // Otherwise "roll the die" by generating a random valid number
                else
                {
                    roll = rand.Next(1, typeDice + 1);
                }
                // Keep track of the first two dice rolled for crit purposes
                // NOTE - When using one-sided dice, there are no crits
                if (numDice >= 2 & typeDice > 1)
                {
                    // Check the first die of the overall roll
                    if (i % numDice == 0)
                    {
                        // Set a flag if it's the max or min possible value
                        if (roll == typeDice)
                        {
                            critSuccess = true;
                        }
                        else if (roll == 1)
                        {
                            critFail = true;
                        }
                    }
                    // Check the second die of the overall roll
                    else if (i % numDice == 1)
                    {
                        // If the first die was valid for a crit AND this die is too, keep the crit
                        // Otherwise, reset it to false
                        if (critSuccess & roll != typeDice)
                        {
                            critSuccess = false;
                        }
                        if (critFail & roll != 1)
                        {
                            critFail = false;
                        }
                    }
                }
                // Now add the roll to the output and increment sum and successes appropriately
                string rollText = $" Roll {(i % numDice) + 1}: {roll}\n";
                if (i < numDice)
                {
                    sumOutputText1 += rollText;
                    successOutputText1 += rollText;
                    sum1 += roll;
                    if (roll >= successThreshold)
                    {
                        successes1++;
                    }
                }
                else
                {
                    sumOutputText2 += rollText;
                    successOutputText2 += rollText;
                    sum2 += roll;
                    if (roll >= successThreshold)
                    {
                        successes2++;
                    }
                }
            }
            // Check which set of results to use based on Advantage State
            // Auto-assign the correct values to the outputText1 variables
            // Advantage
            if (AdvantageRadioButton.Checked == true)
            {
                bool sumUsesSecondSet = false;
                bool successesUsesSecondSet = false;
                // Since we assume first set of rolls by default, we only need to update the values if:
                //  The second set of rolls crit and the first didn't OR
                //  Both sets crit and the second set had more successes OR
                //  Neither set crit and the second set had more successes
                if ((critSuccess & !ogCritSuccess) || (critSuccess == ogCritSuccess & successes2 > successes1))
                {
                    successesUsesSecondSet = true;
                }
                // Since we assume first set of rolls by default, we only need to update the values if:
                //  The second set of rolls had a higher sum than the first
                if (sum2 > sum1)
                {
                    sumUsesSecondSet = true;
                }
                // Set the appropriate text
                // If there's a tie, try to match Sum and Successes together
                if (sumUsesSecondSet || (sum1 == sum2 & successesUsesSecondSet))
                {
                    sumOutputText1 = $"Total Sum: {sum2}\n{sumOutputText2}";
                }
                else
                {
                    sumOutputText1 = $"Total Sum: {sum1}\n{sumOutputText1}";
                }
                if (successesUsesSecondSet || (critSuccess == ogCritSuccess & successes1 == successes2 & sumUsesSecondSet))
                {
                    successOutputText1 = $"Total Successes: {successes2}\n{successOutputText2}";
                }
                else
                {
                    successOutputText1 = $"Total Successes: {successes1}\n{successOutputText1}";
                }
                // Optionally add a Crit notification to the beginning if there was one
                if ((successesUsesSecondSet & critSuccess) || (!successesUsesSecondSet & ogCritSuccess))
                {
                    successOutputText1 = $"Critical Success!\n{successOutputText1}";
                }
                else if ((successesUsesSecondSet & critFail) || (!successesUsesSecondSet & ogCritFail))
                {
                    successOutputText1 = $"Critical Fail!\n{successOutputText1}";
                }
            }
            // Disadvantage
            else if (DisadvantageRadioButton.Checked == true)
            {
                bool sumUsesSecondSet = false;
                bool successesUsesSecondSet = false;
                // Since we assume first set of rolls by default, we only need to update the values if:
                //  The second set of rolls crit and the first didn't OR
                //  Both sets crit and the second set had fewer successes OR
                //  Neither set crit and the second set had fewer successes
                if ((critFail & !ogCritFail) || (critFail == ogCritFail & successes2 < successes1))
                {
                    successesUsesSecondSet = true;
                }
                // Since we assume first set of rolls by default, we only need to update the values if:
                //  The second set of rolls had a lower sum than the first
                if (sum2 < sum1)
                {
                    sumUsesSecondSet = true;
                }
                // Set the appropriate text
                // If there's a tie, try to match Sum and Successes together
                if (sumUsesSecondSet || (sum1 == sum2 & successesUsesSecondSet))
                {
                    sumOutputText1 = sumOutputText1 = $"Total Sum: {sum2}\n{sumOutputText2}";
                }
                else
                {
                    sumOutputText1 = $"Total Sum: {sum1}\n{sumOutputText1}";
                }
                if (successesUsesSecondSet || (critSuccess == ogCritSuccess & successes1 == successes2 & sumUsesSecondSet))
                {
                    successOutputText1 = $"Total Successes: {successes2}\n{successOutputText2}";
                }
                else
                {
                    successOutputText1 = $"Total Successes: {successes1}\n{successOutputText1}";
                }
                // Optionally add a Crit notification to the beginning if there was one
                if ((successesUsesSecondSet & critSuccess) || (!successesUsesSecondSet & ogCritSuccess))
                {
                    successOutputText1 = $"Critical Success!\n{successOutputText1}";
                }
                else if ((successesUsesSecondSet & critFail) || (!successesUsesSecondSet & ogCritFail))
                {
                    successOutputText1 = $"Critical Fail!\n{successOutputText1}";
                }
            }
            // No Advantage/Disadvantage, so just go with first set of dice rolls
            else
            {
                sumOutputText1 = $"Total Sum: {sum1}\n{sumOutputText1}";
                successOutputText1 = $"Total Successes: {successes1}\n{successOutputText1}";
                // Optionally add a Crit notification to the beginning if there was one
                if (critSuccess)
                {
                    successOutputText1 = $"Critical Success!\n{successOutputText1}";
                }
                else if (critFail)
                {
                    successOutputText1 = $"Critical Fail!\n{successOutputText1}";
                }
            }
            // Display the results
            BodyTextSumDiceRollerLabel.Text = sumOutputText1;
            BodyTextSuccessDiceRollerLabel.Text = successOutputText1;
        }

        // All of the code for calculating and displaying all possible dice results
        private void RunProbabilityDisplays()
        {
            // Find sum average and standard deviation
            double[] sumDistribution = GenerateSumDistribution();
            double average = Average(sumDistribution);
            double stdDeviation = StandardDeviation(sumDistribution);
            // Convert the sum average info to text
            string sumOutputText = " Roughly 68% of all Sums will be within 1 Std. Dev.\n Roughly 95% of all Sums will be within 2 Std. Devs.\n";
            sumOutputText = $"{sumOutputText}Useful Stats\n Average Sum: {average.ToString("F2")}\n";
            // Find the range of values within 1 standard deviation and correct them if they're out-of-bounds
            double stdDeviationUp = average + stdDeviation;
            double stdDeviationDown = average - stdDeviation;
            if (stdDeviationUp > typeDice * numDice)
            {
                stdDeviationUp = typeDice * numDice;
            }
            if (stdDeviationDown < numDice)
            {
                stdDeviationDown = numDice;
            }
            // Convert the 1-standard-deviation info to text
            sumOutputText = $"{sumOutputText} Range of Sums Within 1 Std. Deviation: {stdDeviationDown.ToString("F2")} to {stdDeviationUp.ToString("F2")}\n";
            // Find the range of values within 2 standard deviations and correct them if they're out-of-bounds
            stdDeviationUp = average + (2 * stdDeviation);
            stdDeviationDown = average - (2 * stdDeviation);
            if (stdDeviationUp > typeDice * numDice)
            {
                stdDeviationUp = typeDice * numDice;
            }
            if (stdDeviationDown < numDice)
            {
                stdDeviationDown = numDice;
            }
            // Convert the 2-standard-deviations info to text
            sumOutputText = $"{sumOutputText} Range of Sums Within 2 Std. Deviations: {stdDeviationDown.ToString("F2")} to {stdDeviationUp.ToString("F2")}\n";
            // Convert the sum distribution to text
            sumOutputText = $"{sumOutputText}Results Distribution\n";
            for (int i = 0; i < sumDistribution.Length; i++)
            {
                if (i >= numDice)
                {
                    sumOutputText = $"{sumOutputText} Sum of {i}: {sumDistribution[i].ToString("P")}\n";
                }
            }
            // Repeat the process with successes now
            double[] successDistribution = GenerateSuccessDistribution();
            average = Average(successDistribution);
            stdDeviation = StandardDeviation(successDistribution);
            // Convert the success average info to text
            string successOutputText = " Roughly 68% of all Attempts will be within 1 Std. Dev.\n Roughly 95% of all Attempts will be within 2 Std. Devs.\n";
            successOutputText = $"{successOutputText}Useful Stats\n Average Num Successes: {average.ToString("F2")}\n";
            // Find the range of values within 1 standard deviation and correct them if they're out-of-bounds
            stdDeviationUp = average + stdDeviation;
            stdDeviationDown = average - stdDeviation;
            if (stdDeviationUp > numDice)
            {
                stdDeviationUp = numDice;
            }
            if (stdDeviationDown < 0)
            {
                stdDeviationDown = 0.0;
            }
            // Convert the 1-standard-deviation info to text
            successOutputText = $"{successOutputText} Range of Successes Within 1 Std. Deviation: {stdDeviationDown.ToString("F2")} to {stdDeviationUp.ToString("F2")}\n";
            // Find the range of values within 2 standard deviations and correct them if they're out-of-bounds
            stdDeviationUp = average + (2 * stdDeviation);
            stdDeviationDown = average - (2 * stdDeviation);
            if (stdDeviationUp > numDice)
            {
                stdDeviationUp = numDice;
            }
            if (stdDeviationDown < 0)
            {
                stdDeviationDown = 0.0;
            }
            // Convert the 2-standard-deviations info to text
            successOutputText = $"{successOutputText} Range of Successes Within 2 Std. Deviations: {stdDeviationDown.ToString("F2")} to {stdDeviationUp.ToString("F2")}\n";
            successOutputText = $"{successOutputText}Results Distribution\n";
            for (int i = 0; i < successDistribution.Length; i++)
            {
                if (i == 1)
                {
                    successOutputText = $"{successOutputText} 1 Success:    {successDistribution[i].ToString("P")}\n";
                }
                else
                {
                    successOutputText = $"{successOutputText} {i} Successes: {successDistribution[i].ToString("P")}\n";
                }
            }
            // Display the results
            BodyTextSumProbabilityDisplayLabel.Text = sumOutputText;
            BodyTextSuccessProbabilityDisplayLabel.Text = successOutputText;
        }

        // All of the code for calculating various desired CNH values
        private void RunValueFinder()
        {
            // First do error checking to make absolutely sure that calculations can be run
            // Of the three Value Finder fields (difficulty, numDice, successChance)
            //  *exactly* one of them *must* be empty and the other two *must* be filled
            string errorText = "Please leave exactly one of the above textboxes blank and provide valid inputs for the other two";
            if (difficulty == 0)
            {
                if (valueFinderNumDice == 0)
                {
                    BodyTextValueFinderLabel.Text = errorText;
                    return;
                }
                else
                {
                    if (successChance == 0)
                    {
                        BodyTextValueFinderLabel.Text = errorText;
                        return;
                    }
                }
            }
            else
            {
                if (valueFinderNumDice == 0)
                {
                    if (successChance == 0)
                    {
                        BodyTextValueFinderLabel.Text = errorText;
                        return;
                    }
                }
                else
                {
                    if (successChance != 0)
                    {
                        BodyTextValueFinderLabel.Text = errorText;
                        return;
                    }
                }
            }
            // Value finding begins here
            // Find the difficulty given overall success chance and number of dice
            if (difficulty == 0)
            {
                return;
            }
            // Find the number of dice needed for an overall success chance and a difficulty
            else if (valueFinderNumDice == 0)
            {
                return;
            }
            // Find the overall success chance given a number of dice and a difficulty
            else if (successChance == 0)
            {
                return;
            }
        }

        // All of the code for calculating various ways to reach a dice sum
        private void RunDiceFinder()
        {
            return;
        }

        // What happens when the 'Run' button is used
        private void RunButton_Click(object sender, EventArgs e)
        {
            if (FunctionOptionsTabControl.SelectedTab == DiceRollerTabPage)
            {
                RunDiceRoller();
            }
            else if (FunctionOptionsTabControl.SelectedTab == ProbabilityDisplaysTabPage)
            {
                RunProbabilityDisplays();
            }
            else if (FunctionOptionsTabControl.SelectedTab == ValueFinderTabPage)
            {
                RunValueFinder();
            }
            else if (FunctionOptionsTabControl.SelectedTab == DiceFinderTabPage)
            {
                RunDiceFinder();
            }
        }

        // Ensures correct functionality when certain tab pages are selected
        private void FunctionOptionsTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* Notes for the Value Finder tab page:
             *  The 'Number of Dice' text box needs to "move" into the tab page
             *   This is accomplished via copying the common version's settings and resetting and hiding it
             *  The 'Run' button can *only* be enabled if exactly one of the three options is empty
             */
            if (FunctionOptionsTabControl.SelectedTab == ValueFinderTabPage)
            {
                // Copy settings
                NumDiceValueFinderTextBox.Text = NumDiceTextBox.Text;
                // Reset
                NumDiceTextBox.Text = "";
                // Hide
                NumDiceTextBox.Visible = false;
                NumDiceTextBox.Enabled = false;
                NumDiceLabel.Visible = false;
            }
            /* When switching off of the Value Finder tab page:
             *  The 'Number of Dice' text box needs to "move" out of the tab page
             *   This is accomplished by copying the settings and resetting the tab version, then showing the common version
             *  The 'Run' button is re-enabled (no other pages need input save for Dice Finder, which has a default value)
             */
            else
            {
                // Copy settings
                if (!string.IsNullOrEmpty(NumDiceValueFinderTextBox.Text))
                {
                    NumDiceTextBox.Text = NumDiceValueFinderTextBox.Text;
                }
                // Reset
                NumDiceValueFinderTextBox.Text = "";
                // Show
                NumDiceTextBox.Visible = true;
                NumDiceTextBox.Enabled = true;
                NumDiceLabel.Visible = true;
            }
        }

        // Validation checks for each input text box
        //------------------------------------------

        private void TypeDiceTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of d6
                if (string.IsNullOrEmpty(TypeDiceTextBox.Text))
                {
                    x = defaultTypeDice;
                }
                // Since we only use the number, strip the 'd' if it exists
                else if (Char.ToLower(TypeDiceTextBox.Text[0]) == 'd')
                {
                    x = Int32.Parse(TypeDiceTextBox.Text.Substring(1));
                }
                else
                {
                    x = Int32.Parse(TypeDiceTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                    TypeDiceTextBox.Text = "1";
                }
                // Assign variables and enable everything since we passed validation
                typeDice = x;
                // Change the Success Threshold textbox's default value if it doesn't have any user input
                // Also enable editing the Success Threshold textbox since we have a valid dice type for error checking
                if (string.IsNullOrEmpty(SuccessThresholdTextBox.Text))
                {
                    if (typeDice % 2 == 0)
                    {
                        x = (typeDice / 2) + 1;
                    }
                    else
                    {
                        x = (int)Math.Ceiling((double)typeDice / 2);
                    }
                    successThreshold = x;
                    SuccessThresholdTextBox.PlaceholderText = successThreshold.ToString();
                }
                else if (Int32.Parse(SuccessThresholdTextBox.Text) > typeDice)
                {
                    successThreshold = typeDice;
                    SuccessThresholdTextBox.Text = $"{typeDice}";
                }
                ErrorProvider.SetError(TypeDiceTextBox, "");
            }
            catch (Exception ex)
            {
                // Disable the Success Threshold textbox, since it needs a valid dice type for error checking
                TypeDiceTextBox.Text = "";
                SuccessThresholdTextBox.Text = "";
                typeDice = defaultTypeDice;
                successThreshold = defaultSuccessThreshold;
                ErrorProvider.SetError(TypeDiceTextBox, "Invalid value - Enter an integer greater than 0 optionally preceeded by a 'd'");
            }
        }

        private void NumDiceTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of 1 die
                if (string.IsNullOrEmpty(NumDiceTextBox.Text))
                {
                    x = defaultNumDice;
                }
                else
                {
                    x = Int32.Parse(NumDiceTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                    NumDiceTextBox.Text = "1";
                }
                // Assign variables and enable everything since we passed validation
                numDice = x;
                ErrorProvider.SetError(NumDiceTextBox, "");
            }
            catch (Exception ex)
            {
                NumDiceTextBox.Text = "";
                numDice = defaultNumDice;
                valueFinderNumDice = defaultValueFinderNumDice;
                ErrorProvider.SetError(NumDiceTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void SuccessThresholdTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of the smallest number in the upper half of the dice type
                //  Ex: '4' for 'd6', '5' for 'd8', '11' for 'd20'
                if (string.IsNullOrEmpty(SuccessThresholdTextBox.Text))
                {
                    if (typeDice % 2 == 0)
                    {
                        x = (typeDice / 2) + 1;
                    }
                    else
                    {
                        x = (int)Math.Ceiling((double)typeDice / 2);
                    }
                    SuccessThresholdTextBox.PlaceholderText = x.ToString();
                }
                else
                {
                    x = Int32.Parse(SuccessThresholdTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                    SuccessThresholdTextBox.Text = "1";
                }
                else if (x > typeDice)
                {
                    x = typeDice;
                    SuccessThresholdTextBox.Text = $"{typeDice}";
                }
                // Assign variables and enable everything since we passed validation
                successThreshold = x;
                ErrorProvider.SetError(SuccessThresholdTextBox, "");
            }
            catch (Exception ex)
            {
                // Customize error message since "Enter an integer between 1 and 1" sounds clunky
                String errorString;
                if (typeDice == 1)
                {
                    errorString = "Invalid value - Must be 1 since a one-sided dice type is being used";
                }
                else
                {
                    errorString = $"Invalid value - Enter an integer between 1 and {typeDice}, inclusive";
                }
                SuccessThresholdTextBox.Text = "";
                if (typeDice % 2 == 0)
                {
                    successThreshold = (typeDice / 2) + 1;
                }
                else
                {
                    successThreshold = (int)Math.Ceiling((double)typeDice / 2);
                }
                ErrorProvider.SetError(SuccessThresholdTextBox, errorString);
            }
        }

        private void DifficultyValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(DifficultyValueFinderTextBox.Text))
                {
                    x = defaultDifficulty;
                }
                else
                {
                    x = Int32.Parse(DifficultyValueFinderTextBox.Text);
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x < 1)
                    {
                        x = 1;
                        DifficultyValueFinderTextBox.Text = "1";
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                difficulty = x;
                ErrorProvider.SetError(DifficultyValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                DifficultyValueFinderTextBox.Text = "";
                difficulty = defaultDifficulty;
                ErrorProvider.SetError(DifficultyValueFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void NumDiceValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(NumDiceValueFinderTextBox.Text))
                {
                    x = defaultValueFinderNumDice;
                }
                else
                {
                    x = Int32.Parse(NumDiceValueFinderTextBox.Text);
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x < 1)
                    {
                        x = 1;
                        NumDiceValueFinderTextBox.Text = "1";
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                valueFinderNumDice = x;
                ErrorProvider.SetError(NumDiceValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                NumDiceValueFinderTextBox.Text = "";
                numDice = defaultNumDice;
                valueFinderNumDice = defaultValueFinderNumDice;
                ErrorProvider.SetError(NumDiceValueFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }

        private void SuccessChanceValueFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                double x;
                // If this textbox is empty, assign a value of 0
                // NOTE - This is used for checking which value needs to be calculated
                if (string.IsNullOrEmpty(SuccessChanceValueFinderTextBox.Text))
                {
                    x = defaultSuccessChance;
                }
                // If user input has a '%' symbol at the end, strip it and treat the value as a percent (divide by 100)
                else if (SuccessChanceValueFinderTextBox.Text[SuccessChanceValueFinderTextBox.Text.Length - 1] == '%')
                {
                    x = Double.Parse(SuccessChanceValueFinderTextBox.Text.Substring(0, SuccessChanceValueFinderTextBox.Text.Length - 1));
                    x = x / 100;
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x <= 0)
                    {
                        x = 0.01;
                        SuccessChanceValueFinderTextBox.Text = "1%";
                    }
                    else if (x >= 1)
                    {
                        x = 0.99;
                        SuccessChanceValueFinderTextBox.Text = "99%";
                    }
                }
                else
                {
                    x = Double.Parse(SuccessChanceValueFinderTextBox.Text);
                    // If user input doesn't have a '%' at the end, assume values >= 1 are percentage form and treat them as such (divide by 100)
                    if (x >= 1)
                    {
                        x = x / 100;
                    }
                    // Auto assign variable to closest acceptable value when input is out-of-bounds
                    if (x <= 0)
                    {
                        x = 0.01;
                        SuccessChanceValueFinderTextBox.Text = "1%";
                    }
                    else if (x >= 1)
                    {
                        x = 0.99;
                        SuccessChanceValueFinderTextBox.Text = "99%";
                    }
                }
                // Assign variables and enable the 'Run' button if *exactly* one of the Value Finder options is empty, since we passed validation
                successChance = x;
                ErrorProvider.SetError(SuccessChanceValueFinderTextBox, "");
            }
            catch (Exception ex)
            {
                SuccessChanceValueFinderTextBox.Text = "";
                successChance = defaultSuccessChance;
                ErrorProvider.SetError(SuccessChanceValueFinderTextBox, "Invalid value - Enter a number between 0 and 100, exclusive, optionally followed by a '%'");
            }
        }

        private void SumDiceFinderTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int x;
                // Default value of a dice sum of 1
                if (string.IsNullOrEmpty(SumDiceFinderTextBox.Text))
                {
                    x = defaultDiceSum;
                }
                else
                {
                    x = Int32.Parse(SumDiceFinderTextBox.Text);
                }
                // Auto assign variable to closest acceptable value when input is out-of-bounds
                if (x < 1)
                {
                    x = 1;
                    SumDiceFinderTextBox.Text = "1";
                }
                // Assign variables and enable everything since we passed validation
                diceSum = x;
                ErrorProvider.SetError(SumDiceFinderTextBox, "");
            }
            catch (Exception ex)
            {
                SumDiceFinderTextBox.Text = "";
                diceSum = defaultDiceSum;
                ErrorProvider.SetError(SumDiceFinderTextBox, "Invalid value - Enter an integer greater than 0");
            }
        }
    }
}
