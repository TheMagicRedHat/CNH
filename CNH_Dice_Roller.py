import random as r
from math import comb
from math import floor

def main():
    # Hard-coded basic variables. By default, assume 6-sided dice where Successes are 4, 5, & 6
    type_dice = 6 #Type of dice to roll (number of faces per die)
    minimum_success_value = 4 #Minimum value on each die to be considered a Success
    success_probability = (type_dice + 1 - minimum_success_value) / type_dice #Odds of each die resulting in a Success

    # Establish basic variables via user-input
    num_dice = input("How many dice are you using?\n") #Number of dice to roll
    try:
        num_dice = int(num_dice)
    except:
        print("ERROR: Non-integer value entered for number of dice\nEnding program")
        return
    if num_dice < 1:
        print("WARNING: Value for number of dice is out-of-bounds")
        print("         Acceptable values are integers greater than 0")
        print("         Proceeding assuming 1 die")
        num_dice = 1
    # Assume a normal roll by default
    advantage = input("\nEnter a value for Advantage state\n 1: Advantage\n 0: Normal\n-1: Disadvantage\n") #-1 for Disadvantage, 0 for Normal, 1 for Advantage
    try:
        advantage = int(advantage)
    except:
        print("WARNING: Non-integer value entered for Advantage state")
        print("         Proceeding assuming a Normal state")
        advantage = 0
    if advantage < -1 or advantage > 1:
        print("WARNING: Value for Advantage state is out-of-bounds")
        if advantage < -1:
            print("         Proceeding assuming a Disadvantageous state".format())
            advantage = -1
        else:
            print("         Proceeding assuming an Advantageous state".format())
            advantage = 1

    # Check if a roll is needed, or if statistics are needed
    # Assume 'No' by default, and only run a simulation if 'Yes' (or something similar) is entered
    roll_check = input("\nDo you want to simulate a roll?\n")
    # Simulate rolls - print out each roll result and tally the final successes amount
    if "Y" in roll_check.upper():
        print("\nRolls:")
        total_successes = 0
        total_sum = 0
        for x in range(num_dice):
            roll = r.randint(1, type_dice)
            if advantage == 1:
                roll_2 = r.randint(1, type_dice)
                roll = max(roll, roll_2)
            elif advantage == -1:
                roll_2 = r.randint(1, type_dice)
                roll = min(roll, roll_2)
            total_sum += roll
            if roll >= minimum_success_value:
                total_successes += 1
            print("{:2}: {}".format(x + 1, roll))
        print("\nTotal Successes: {}".format(total_successes))
        print("Total Sum: {}".format(total_sum))

    # Display various stats about the requested roll
    else:
        if "N" not in roll_check.upper():
            print("WARNING: Non 'Yes' or 'No' response received")
            print("         Proceeding assuming a 'No' response")
        # Success probability distribution
        # Successes and Failures are always measured as a binary state, so probability is always 1:1
        # Start by finding the normal probability distribution
        print("\nProbability distribution for all possible number of Successes:")
        base_success_distribution = []
        for x in range(num_dice + 1):
            probability = comb(num_dice, x) * (success_probability**x) * ((1 - success_probability)**(num_dice - x))
            base_success_distribution.append(probability)
        final_distribution = base_success_distribution
        # Use the base distribution to find the modified distributions for Advantage/Disadvantage
        # Advantage probability is equal to:
        #  the probability of rolling 2 of the same value (base probability squared)
        #  plus twice (one for the first die, one for the second die)
        #  the probability of rolling a specific value multiplied by the probabilities of the higher/lower values
        if advantage != 0:
            modified_success_distribution = []
            for x in range(num_dice + 1):
                # Advantage
                if advantage == 1:
                    probability = (base_success_distribution[x]**2) + (2 * sum(base_success_distribution[0:x]) * base_success_distribution[x])
                # Disadvantage
                else:
                    probability = (base_success_distribution[x]**2) + (2 * sum(base_success_distribution[x + 1:num_dice + 1]) * base_success_distribution[x])
                modified_success_distribution.append(probability)
            final_distribution = modified_success_distribution
        for x in range(num_dice + 1):
            if x == 1:
                print("{:2} Success:   {:7.3%}".format(x, final_distribution[x]))
            else:
                print("{:2} Successes: {:7.3%}".format(x, final_distribution[x]))

        # Probability of reaching at least a certain amount of Successes
        minimum_successes = input("\nAt least how many Successes do you want?\n")
        try:
            minimum_successes = int(minimum_successes)
        except:
            print("WARNING: Non-integer value entered for minimum number of Successes")
            print("         Proceeding assuming at least 1 Success")
            minimum_successes = 1
        if minimum_successes < 0 or minimum_successes > num_dice:
            print("WARNING: Value for minimum number of Successes is out-of-bounds")
            print("         Acceptable values are integers between 0 and {} (inclusive)".format(num_dice))
            if minimum_successes < 0:
                print("         Proceeding assuming a value of 0")
                minimum_successes = 0
            else:
                print("         Proceeding assuming a value of {}".format(num_dice))
                minimum_successes = num_dice
        if minimum_successes == 1:
            print("Probability of at least {} Success: {:.3%}".format(minimum_successes, sum(final_distribution[minimum_successes:num_dice + 1])))
        else:
            print("Probability of at least {} Successes: {:.3%}".format(minimum_successes, sum(final_distribution[minimum_successes:num_dice + 1])))
        if minimum_successes + 4 > num_dice:
            print("Impossible to Critically Succeed\n")
        else:
            print("Probability of a Critical Success: {:.3%}\n".format(sum(final_distribution[minimum_successes + 4:num_dice + 1])))
        
        # Average total value of the dice roll for Damage
        base_damage_distribution = []
        for x in range(num_dice):
            base_damage_distribution.append(0)
        damage_average = 0
        # Determine the probability distribution for normal dice rolls
        # Probability for each possible damage value requires:
        #  the number of ways to roll a specific amount of damage (ways_to_x)
        #  divided by
        #  the number of total possible rolls (6 for 1d6, 36 for 2d6, etc)
        for x in range(num_dice, (num_dice * type_dice) + 1):
            if num_dice == 1:
                ways_to_x = 1
            else:
                ways_to_x = 0
                potential_repeats = floor((x - num_dice) / (type_dice)) + 1
                for k in range(min(num_dice + 1, potential_repeats)):
                    ways_to_x += ((-1)**k) * (comb(num_dice, k)) * (comb(x - 1 - (type_dice * k), (num_dice - 1)))
            probability = ways_to_x / (type_dice**num_dice)
            base_damage_distribution.append(probability)
        final_damage_distribution = base_damage_distribution
        # Use the base distribution to find the modified distributions for Advantage/Disadvantage
        # Advantage probability is equal to:
        #  the probability of rolling 2 of the same value (base probability squared)
        #  plus twice (one for the first die, one for the second die)
        #  the probability of rolling a specific value multiplied by the probabilities of the higher/lower values
        modified_damage_distribution = []
        for x in range(num_dice):
            modified_damage_distribution.append(0)
        if advantage != 0:
            for x in range(num_dice, (num_dice * type_dice) + 1):
                # Advantage
                if advantage == 1:
                    probability = (base_damage_distribution[x]**2) + (2 * sum(base_damage_distribution[num_dice:x]) * base_damage_distribution[x])
                # Disadvantage
                elif advantage == -1:
                    probability = (base_damage_distribution[x]**2) + (2 * sum(base_damage_distribution[x + 1:(num_dice * type_dice) + 1]) * base_damage_distribution[x])
                modified_damage_distribution.append(probability)
            final_damage_distribution = modified_damage_distribution
        # Calculate the average damage
        for x in range(num_dice, (num_dice * type_dice) + 1):
            damage_average += x * final_damage_distribution[x]
        print("Average value of rolls: {:.3f}\n".format(damage_average))

        # Average expected range of the dice roll for Damage by using 1 & 2 standard deviations
        # Value range within standard deviations is *CURRENTLY* rounded to nearest whole-number for convenience
        # Roughly 68% of values are within 1 standard deviation
        # Roughly 95% of values are within 2 standard deviations
        standard_deviation = 0
        for x in range(num_dice, (num_dice * type_dice) + 1):
            standard_deviation += ((x - damage_average)**2) * final_damage_distribution[x]
        standard_deviation = standard_deviation**0.5
        print("Standard Deviation of rolls: {:.3f}".format(standard_deviation))
        # 1 Standard Deviation
        high_value = round(damage_average + standard_deviation)
        if high_value > num_dice * type_dice:
            high_value = num_dice * type_dice
        low_value = round(damage_average - standard_deviation)
        if low_value < num_dice:
            low_value = num_dice
        print("Range of values within 1 standard deviation:  {:3} to {}".format(low_value, high_value))
        # 2 Standard Deviations
        high_value = round(damage_average + (2 * standard_deviation))
        if high_value > num_dice * type_dice:
            high_value = num_dice * type_dice
        low_value = round(damage_average - (2 * standard_deviation))
        if low_value < num_dice:
            low_value = num_dice
        print("Range of values within 2 standard deviations: {:3} to {}\n".format(low_value, high_value))

if __name__ == "__main__":
    main()