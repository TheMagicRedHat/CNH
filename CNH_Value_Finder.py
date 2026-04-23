import random as r
from math import comb
from math import floor
from math import ceil

# TODO: Add support for Critical Successes, Critical Failures, Critical Hits, and Critical Misses (once those are properly established)

# Misc. Helper Functions
#-----------------------

# Generate an array of the probability distribution for Successes given a number of dice rolled,
#   the advantage state, and the probability that any single die will be a Success
# @param num_dice <int> The number of dice used for a roll
# @param advantage <int> The advantage state (0: Normal, 1: Advantage, -1: Disadvantage)
# @param success_probability <float> The percentage chance of any single die roll being a Success (between 0 and 1)
# @return final_distribution The probability distribution as a list where the position corresponds to the amount of Successes
def generate_success_distribution(num_dice, advantage, success_probability):
    # Successes and Failures are always measured as a binary state
    # Start by finding the normal probability distribution
    base_success_distribution = []
    for x in range(num_dice + 1):
        probability = comb(num_dice, x) * (success_probability**x) * ((1 - success_probability)**(num_dice - x))
        base_success_distribution.append(probability)
    # Assume normal advantage state for initial assignment
    final_distribution = base_success_distribution
    # Use the base distribution to find the modified distributions for Advantage/Disadvantage
    # Advantage probability is equal to:
    #   the probability of rolling 2 of the same value (base probability squared)
    #   plus twice (one for the first die, one for the second die)
    #   the probability of rolling a specific value multiplied by the probabilities of the higher/lower values
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
    return final_distribution

# Generate an array of the probability distribution for the sum of face values given a number of dice rolled,
#   the number of faces on each die, and the advantage state
# @param type_dice <int> The number of faces on the dice to be rolled
# @param num_dice <int> The number of dice used for a roll
# @param advantage <int> The advantage state (0: Normal, 1: Advantage, -1: Disadvantage)
# @return final_damage_distribution The probability distribution of each possible total result as a list where the position corresponds to the total sum
def generate_damage_distribution(type_dice, num_dice, advantage):
    # Average total value of the dice roll for Damage
    base_damage_distribution = []
    for x in range(num_dice):
        base_damage_distribution.append(0)
    # Determine the probability distribution for normal dice rolls
    # Probability for each possible damage value requires:
    #   the number of ways to roll a specific amount of damage (ways_to_x)
    #   divided by
    #   the number of total possible rolls (6 for 1d6, 36 for 2d6, etc)
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
    #   the probability of rolling 2 of the same value (base probability squared)
    #   plus twice (one for the first die, one for the second die)
    #   the probability of rolling a specific value multiplied by the probabilities of the higher/lower values
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
    return final_damage_distribution

# Calculate the precise number of dice needed to reach a specific sum from a given method
#   Methods include average damage, upper/lower values within 1 std. dev., and upper/lower values within 2 std. devs.
# @param type_dice <int> The number of faces on the dice to be rolled
# @param advantage <int> The advantage state (0: Normal, 1: Advantage, -1: Disadvantage)
# @param std_devs <int> The number of standard deviations to use for the method
#                       Acceptable values are -2, -1, 0, 1, or 2
#                         Where positive values are upper values within the specified amount of std. devs.
#                         and negative values are lower values within the specified amount of std. devs.
# @return {num_dice_low: damage_low, num_dice_high: damage_high} The associated range of values necessary for the given damage sum
#         {num_dice: damage} If the value necessary was found to be exactly equal to the damage sum
def find_dice_for_damage(type_dice, advantage, damage_sum, std_devs):
    num_dice = floor(damage_sum / type_dice)
    damage_average = 0
    damage_checker = 0
    # Start with a lower boundary for amount of dice, then gradually increase until the right amount is found
    while damage_checker < damage_sum:
        num_dice += 1
        # Recalculate average damage and standard deviation with new dice amount
        damage_average = 0
        standard_deviation = 0
        distribution = generate_damage_distribution(type_dice, num_dice, advantage)
        for x in range(num_dice, (num_dice * type_dice) + 1):
            damage_average += x * distribution[x]
        standard_deviation = 0
        for x in range(num_dice, (num_dice * type_dice) + 1):
            standard_deviation += ((x - damage_average)**2) * distribution[x]
        standard_deviation = standard_deviation**0.5
        # Figure out what value is needed for the loop check - based on amount of std. devs.
        damage_checker = damage_average + (std_devs * standard_deviation)
        # Damage can't be less than the absolute minimum
        if damage_checker < num_dice:
            damage_checker = num_dice
        # Damage can't be more than the absolute maximum
        elif damage_checker > num_dice * type_dice:
            damage_checker = num_dice * type_dice
    # If we found the right value exactly
    if damage_checker == damage_sum:
        return {num_dice: damage_checker}
    # If we didn't, find the lower values in the range
    num_dice -= 1
    # Account for edge-case where 1 die is more than enough
    if (num_dice == 0):
        return {0: 0, num_dice + 1: damage_checker}
    # Find the lower number of dice and its associated damage
    damage_average = 0
    standard_deviation = 0
    distribution = generate_damage_distribution(type_dice, num_dice, advantage)
    for x in range(num_dice, (num_dice * type_dice) + 1):
        damage_average += x * distribution[x]
    standard_deviation = 0
    for x in range(num_dice, (num_dice * type_dice) + 1):
        standard_deviation += ((x - damage_average)**2) * distribution[x]
    standard_deviation = standard_deviation**0.5
    # Figure out what value is needed for the loop check - based on amount of std. devs.
    damage_checker_low = damage_average + (std_devs * standard_deviation)
    # Damage can't be less than the absolute minimum
    if damage_checker_low < num_dice:
        damage_checker_low = num_dice
    # Damage can't be more than the absolute maximum
    elif damage_checker_low > num_dice * type_dice:
        damage_checker_low = num_dice * type_dice
    # Return the final desired dictionary of amount of dice and associated damage
    return {num_dice: damage_checker_low, num_dice + 1: damage_checker}

# User Input Functions
#---------------------

# Find the number of dice used via user input
# @return The number of dice
def get_num_dice():
    num_dice = input("How many dice are you using?\n")
    try:
        num_dice = int(num_dice)
    except:
        print("WARNING: Non-integer value entered for number of dice")
        print("         Acceptable values are integers greater than 0")
        print("         Proceeding assuming 1 die")
        num_dice = 1
    if num_dice < 1:
        print("WARNING: Value for number of dice is out-of-bounds")
        print("         Acceptable values are integers greater than 0")
        print("         Proceeding assuming 1 die")
        num_dice = 1
    return num_dice

# Find the required number of Successes via user input
# @param num_dice <int>
# @return The number of Successes
def get_successes(num_dice):
    successes = input("At least how many Successes do you want?\n")
    try:
        successes = int(successes)
    except:
        print("WARNING: Non-integer value entered for number of Successes")
        print("         Proceeding assuming at least 1 Success")
        return 1
    if successes < 0:
        print("WARNING: Value for number of Successes is out-of-bounds")
        print("         Acceptable values are integers between 0 and {} (inclusive)".format(num_dice))
        print("         Proceeding assuming at least 1 Success")
        return 1
    elif successes > num_dice:
        print("WARNING: Value for number of Successes is out-of-bounds")
        print("         Acceptable values are integers between 0 and {} (inclusive)".format(num_dice))
        print("         Proceeding assuming at least {} Successes".format(num_dice))
        return num_dice
    return successes

# Find the difficulty via user input
# @return The difficulty
def get_difficulty():
    difficulty = input("What difficulty are you using?\n")
    try:
        difficulty = int(difficulty)
    except:
        print("WARNING: Non-integer value entered for difficulty")
        print("         Acceptable values are integers greater than or equal to 0")
        print("         Proceeding assuming a difficulty of 1")
        difficulty = 1
    if difficulty < 0:
        print("WARNING: Value for difficulty is out-of-bounds")
        print("         Acceptable values are integers greater than or equal to 0")
        print("         Proceeding assuming a difficulty of 1")
        difficulty = 1
    return difficulty

# Find the required percentage via user input
# @return The percentage as a decimal between 0 and 1
def get_percentage():
    percent = input("At least what chance of success do you want?\n")
    # Assume that the value entered is between 0 and 100 if it ends with a '%' symbol
    # Remove the '%' symbol then process and return the percentage
    if len(percent) > 0 and percent[-1] == "%":
        percent = percent[:-1]
        try:
            percent = float(percent)
            percent /= 100
            return percent
        except:
            print("WARNING: Invalid value entered for percentage")
            print("         Proceeding assuming a value of 50%")
            return 0.5
    # Make sure the value is a number
    try:
        percent = int(percent)
    except:
        try:
            percent = float(percent)
        except:
            print("WARNING: Invalid value entered for percentage")
            print("         Proceeding assuming a value of 50%")
            return 0.5
    # Check boundaries for the entered value
    if percent < 0:
        print("WARNING: Value for percentage is out-of-bounds")
        print("         Acceptable values are integers or decimals between 0 and 100")
        print("         Proceeding assuming value of 0%")
        return 0.0
    elif percent > 100:
        print("WARNING: Value for percentage is out-of-bounds")
        print("         Acceptable values are integers or decimals between 0 and 100")
        print("         Proceeding assuming value of 0%")
        return 1.0
    elif percent > 1:
        percent = percent / 100
    # If value was already between 0 and 1, no need to modify it
    else:
        print("Assuming value entered was a decimal representation of probability between 0 and 1")
    return percent


# Find the advantage state via user input
# @return 0 for normal roll, 1 for advantage, or -1 for disadvantage
def get_advantage():
    # Assume a normal roll by default
    advantage_text = "Enter a value for Advantage state\n"
    advantage_text += " Advantage:     1\n"
    advantage_text += " Normal:        0\n"
    advantage_text += " Disadvantage: -1\n"
    advantage = input(advantage_text) #-1 for Disadvantage, 0 for Normal, 1 for Advantage
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
    return advantage

# Find the total amount that some dice values should sum to
# @param type_dice <int> The number of faces on each dice
# @return The desired sum
def get_sum(type_dice):
    recommended_max = round(((-185 * (type_dice**2)) + (10250 * type_dice) + 29160) / 224)
    if recommended_max < 0:
        damage_sum = input("How much damage would you like to find the dice for?\n")
    else:
        damage_sum = input("How much damage would you like to find the dice for?\nNOTE: Not recommended to pick anything above {}\n".format(recommended_max))
    try:
        damage_sum = int(damage_sum)
    except:
        print("WARNING: Non-integer value entered for total damage")
        print("         Acceptable values are integers greater than 0")
        print("         Proceeding assuming 10 damage")
        damage_sum = 10
    if damage_sum < 1:
        print("WARNING: Value for total damage is out-of-bounds")
        print("         Acceptable values are integers greater than 0")
        print("         Proceeding assuming 1 damage")
        damage_sum = 1
    return damage_sum

# Scenario Functions
#-------------------

# Simulate rolling dice
# @param type_dice <int> The number of faces on the dice to be rolled
# @param minimum_success_value <int> The minimum face value on each die that counts as a Success
def roll_dice(type_dice, minimum_success_value):
    num_dice = get_num_dice()
    advantage = get_advantage()
    # Simulate rolls - print out each roll result and tally the final successes amount
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
    return

# Given a number of dice to roll, find the percentage chance of each possible number of Successes
# @param success_probability <float> The percentage chance of any single die roll being a Success (between 0 and 1)
def calculate_probability_distribution(success_probability):
    num_dice = get_num_dice()
    advantage = get_advantage()
    distribution = generate_success_distribution(num_dice, advantage, success_probability)
    # Print the probability distribution
    print("\nProbability distribution for all possible number of Successes:")
    for x in range(num_dice + 1):
        if x == 1:
            print("{:2} Success:   {:7.3%}".format(x, distribution[x]))
        else:
            print("{:2} Successes: {:7.3%}".format(x, distribution[x]))
    return

# Given a number of dice to roll, find the average sum of the face values
#   and both the 1st and 2nd standard deviations as a range of values
# @param type_dice <int> The number of faces on the dice to be rolled
def calculate_avg_std_dev(type_dice):
    num_dice = get_num_dice()
    advantage = get_advantage()
    damage_average = 0
    distribution = generate_damage_distribution(type_dice, num_dice, advantage)
    # Calculate the average damage
    for x in range(num_dice, (num_dice * type_dice) + 1):
        damage_average += x * distribution[x]
    print("\nAverage value of rolls: {:.3f}\n".format(damage_average))

    # Average expected range of the dice roll for Damage by using 1 & 2 standard deviations
    # Value range within standard deviations is *CURRENTLY* rounded to nearest whole-number for convenience
    # Roughly 68% of values are within 1 standard deviation
    # Roughly 95% of values are within 2 standard deviations
    standard_deviation = 0
    for x in range(num_dice, (num_dice * type_dice) + 1):
        standard_deviation += ((x - damage_average)**2) * distribution[x]
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

    # Print standard deviation coverage reminders
    print("Reminder:")
    print("Roughly 68% of values are within 1 standard deviation")
    print("Roughly 95% of values are within 2 standard deviations")
    return

# Given a number of dice to roll and a difficulty to reach, find the percentage chance
#   EX: With an Attack Attempt of X against a Block of Y, find the chance of a Hit
# @param success_probability <float> The percentage chance of any single die roll being a Success (between 0 and 1)
def calculate_probability(success_probability):
    num_dice = get_num_dice()
    minimum_successes = get_successes(num_dice)
    advantage = get_advantage()
    # Need to first find success probability distribution
    distribution = generate_success_distribution(num_dice, advantage, success_probability)
    # Probability of reaching at least a certain amount of Successes
    if minimum_successes == 1:
        if num_dice == 1:
            print("\nProbability of at least 1 Success when rolling 1 die: {:.3%}".format(sum(distribution[minimum_successes:num_dice + 1])))
        else:
            print("\nProbability of at least 1 Success when rolling {} dice: {:.3%}".format(num_dice, sum(distribution[minimum_successes:num_dice + 1])))
    else:
        if num_dice == 1:
            print("\nProbability of at least {} Successes when rolling 1 die: {:.3%}".format(minimum_successes, sum(distribution[minimum_successes:num_dice + 1])))
        else:
            print("\nProbability of at least {} Successes when rolling {} dice: {:.3%}".format(minimum_successes, num_dice, sum(distribution[minimum_successes:num_dice + 1])))
    return

# Given some number of dice and a desired probability of reaching a difficulty, find the required maximum difficulty
#   EX: With an Attack Attempt of X and a desired Hit chance of at least Y%, find the highest possible value for the Defensive stat
# @param success_probability <float> The percentage chance of any single die roll being a Success (between 0 and 1)
def calculate_difficulty(success_probability):
    num_dice = get_num_dice()
    chance = get_percentage()
    advantage = get_advantage()
    if chance == 0:
        distribution = generate_success_distribution(num_dice, advantage, success_probability)
        probability_sum = distribution[num_dice]
        if num_dice == 1:
            print("\nMaximum possible difficulty when rolling 1 die to have any possible chance of success: {} (results in a {:.3%} chance of success)".format(num_dice, probability_sum))
        else:
            print("\nMaximum possible difficulty when rolling {} dice to have any possible chance of success: {} (results in a {:.3%} chance of success)".format(num_dice, num_dice, probability_sum))
        return
    # Need to first find success probability distribution
    distribution = generate_success_distribution(num_dice, advantage, success_probability)
    # Start with lowest possible difficulty and test chance of success
    difficulty = 0
    probability_sum = 1
    # Gradually increase the difficulty until the success chance is too low
    while probability_sum >= chance:
        difficulty += 1
        probability_sum -= distribution[difficulty - 1]
    # Account for off-by-one error
    difficulty -= 1
    probability_sum += distribution[difficulty]
    if num_dice == 1:
        print("\nMaximum possible difficulty when rolling 1 die to have at least a {:.3%} chance of success: {} (results in a {:.3%} chance of success)".format(chance, difficulty, probability_sum))
    else:
        print("\nMaximum possible difficulty when rolling {} dice to have at least a {:.3%} chance of success: {} (results in a {:.3%} chance of success)".format(num_dice, chance, difficulty, probability_sum))
    return

# Given a difficulty and a desired probability of reaching said difficulty on a roll, find the minimum number of dice needed for a roll
#   EX: With a Defensive stat of X and a desired Hit chance of at least Y%, find the minimum number of dice needed for the Attack Attempt
# @param success_probability <float> The percentage chance of any single die roll being a Success (between 0 and 1)
def calculate_dice(success_probability):
    difficulty = get_difficulty()
    if difficulty == 0:
        print("\nFor a difficulty of 0, no dice need to be rolled. An Attempt with a difficulty of 0 will always be successful")
        return
    chance = get_percentage()
    if chance == 1:
        print("\nFor any non-zero difficulty, at least a 100% chance of success is impossible.")
        return
    advantage = get_advantage()
    if chance == 0:
        distribution = generate_success_distribution(difficulty, advantage, success_probability)
        probability_sum = distribution[difficulty]
        print("\nNumber of dice needed for a difficulty of {} and any possible chance of success: {} (results in a {:.3%} chance of success)".format(difficulty, difficulty, probability_sum))
        return
    # Brute force solution - can probably be optimized
    # Start with lowest possible number of dice and test chance of success
    num_dice = difficulty
    distribution = generate_success_distribution(num_dice, advantage, success_probability)
    probability_sum = sum(distribution[difficulty:num_dice + 1])
    # Gradually increase the number of dice until the success chance is too high
    while probability_sum < chance:
        num_dice += 1
        distribution = generate_success_distribution(num_dice, advantage, success_probability)
        probability_sum = sum(distribution[difficulty:num_dice + 1])
    print("\nNumber of dice needed for a difficulty of {} and at least a {:.3%} chance of success: {} (results in a {:.3%} chance of success)".format(difficulty, chance, num_dice, probability_sum))
    return

# Given a total value, find lots of different information
#   Specifically, find the dice needed for the sum based on:
#   Max possible rolls
#   Average rolls
#   Upper & lower rolls within 1 std. dev.
#   Upper & lower rolls within 2 std. devs.
# @param type_dice <int> The number of faces on each die (type of dice: d6, d20, etc)
def calculate_dice_sum(type_dice):
    damage_sum = get_sum(type_dice)
    advantage = get_advantage()

    # Max possible rolls
    num_dice = ceil(damage_sum / type_dice)
    if num_dice * type_dice == damage_sum:
        print("\nWith max rolls, you need {} dice to reach {} damage (results in {} damage)".format(num_dice, damage_sum, damage_sum))
    else:
        print("\nWith max rolls, you need {}-{} dice to reach {} damage (results in {}-{} damage)".format(num_dice - 1, num_dice, damage_sum, (num_dice - 1) * type_dice, num_dice * type_dice))
    
    # Average rolls
    results = find_dice_for_damage(type_dice, advantage, damage_sum, 0)
    if len(results) == 1:
        print("\nWith average rolls, you need {} dice to reach damage (results in {} damage)".format(list(results.keys())[0], damage_sum))
    else:
        print("\nWith average rolls, you need {}-{} dice to reach {} damage (results in {:.3f}-{:.3f} damage)".format(list(results.keys())[0], list(results.keys())[1], damage_sum, list(results.values())[0], list(results.values())[1]))

    # Upper rolls at boundary of 1 std. dev.
    results = find_dice_for_damage(type_dice, advantage, damage_sum, 1)
    if len(results) == 1:
        print("\nWith values of the upper bound of 1 std. dev., you need {} dice to reach damage (results in {} damage)".format(list(results.keys())[0], damage_sum))
    else:
        print("\nWith values of the upper bound of 1 std. dev., you need {}-{} dice to reach {} damage (results in {:.3f}-{:.3f} damage)".format(list(results.keys())[0], list(results.keys())[1], damage_sum, list(results.values())[0], list(results.values())[1]))

    # Upper rolls at boundary of 2 std. devs.
    results = find_dice_for_damage(type_dice, advantage, damage_sum, 2)
    if len(results) == 1:
        print("With values of the upper bound of 2 std. devs., you need {} dice to reach damage (results in {} damage)".format(list(results.keys())[0], damage_sum))
    else:
        print("With values of the upper bound of 2 std. devs., you need {}-{} dice to reach {} damage (results in {:.3f}-{:.3f} damage)".format(list(results.keys())[0], list(results.keys())[1], damage_sum, list(results.values())[0], list(results.values())[1]))

    # Lower rolls at boundary of 1 std. dev.
    results = find_dice_for_damage(type_dice, advantage, damage_sum, -1)
    if len(results) == 1:
        print("\nWith values of the lower bound of 1 std. dev., you need {} dice to reach damage (results in {} damage)".format(list(results.keys())[0], damage_sum))
    else:
        print("\nWith values of the lower bound of 1 std. dev., you need {}-{} dice to reach {} damage (results in {:.3f}-{:.3f} damage)".format(list(results.keys())[0], list(results.keys())[1], damage_sum, list(results.values())[0], list(results.values())[1]))

    # Lower rolls at boundary of 2 std. devs.
    results = find_dice_for_damage(type_dice, advantage, damage_sum, -2)
    if len(results) == 1:
        print("With values of the lower bound of 2 std. dev., you need {} dice to reach damage (results in {} damage)".format(list(results.keys())[0], damage_sum))
    else:
        print("With values of the lower bound of 2 std. dev., you need {}-{} dice to reach {} damage (results in {:.3f}-{:.3f} damage)".format(list(results.keys())[0], list(results.keys())[1], damage_sum, list(results.values())[0], list(results.values())[1]))

    # Print standard deviation coverage reminders
    print("\nReminder:")
    print("Roughly 68% of values are within 1 standard deviation")
    print("Roughly 95% of values are within 2 standard deviations")
    return

# Main Function
#--------------

def main():
    # Hard-coded basic variables. By default, assume 6-sided dice where Successes are 4, 5, & 6
    type_dice = 6 #Type of dice to roll (number of faces per die)
    minimum_success_value = 4 #Minimum value on each die to be considered a Success
    # Calculate generally universally helpful variable
    success_probability = (type_dice + 1 - minimum_success_value) / type_dice #Odds of each die resulting in a Success

    # Establish scenario via user input
    scenario_string = "Please enter the associated number with the desired scenario:\n"
    scenario_string += " Roll some dice:                                                         1\n"
    scenario_string += " Given some number of dice, find probabilities of every Success outcome: 2\n"
    scenario_string += " Given some number of dice, find the average & std deviations:           3\n"
    scenario_string += " Given some number of dice & a difficulty, find the chance of success:   4\n"
    scenario_string += " Given some number of dice & a chance of success, find the difficulty:   5\n"
    scenario_string += " Given a difficulty & a chance of success, find the number of dice:      6\n"
    scenario_string += " Given a sum of dice values to reach, find various ways to reach it:     7\n"
    scenario = input(scenario_string)
    # Error check input
    try:
        scenario = int(scenario)
    except:
        print("WARNING: Non-integer value entered for scenario")
        print("         Proceeding assuming scenario 2")
        scenario = 2
    if scenario < 1 or scenario > 7:
        print("WARNING: Value for scenario is out-of-bounds")
        print("         Acceptable values are 1, 2, 3, 4, 5, 6, 7")
        print("         Proceeding assuming scenario 2")
        scenario = 2
    # Simple if-statement for all possible scenarios
    if scenario == 1:
        roll_dice(type_dice, minimum_success_value)
    elif scenario == 2:
        calculate_probability_distribution(success_probability)
    elif scenario == 3:
        calculate_avg_std_dev(type_dice)
    elif scenario == 4:
        calculate_probability(success_probability)
    elif scenario == 5:
        calculate_difficulty(success_probability)
    elif scenario == 6:
        calculate_dice(success_probability)
    elif scenario == 7:
        calculate_dice_sum(type_dice)
    else:
        print("ERROR: Invalid scenario value\nEnding program")
    return

if __name__ == "__main__":
    main()