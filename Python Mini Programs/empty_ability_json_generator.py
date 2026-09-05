import random as r
from math import comb
from math import floor

def main():
    number_of_entries = 5
    file_name = input("Please enter a file name (excluding file extention)\n")
    file = open(file_name + ".json", "w")
    file.write("[\n")
    for i in range(number_of_entries):
        file.write("  {\n")
        file.write("    \"name\":\"TEMP\", \"description\":\"TEMP\", \"requirements\":{\n")
        file.write("      \"strength\":0,\n")
        file.write("      \"dexterity\":0,\n")
        file.write("      \"intelligence\":0,\n")
        file.write("      \"charisma\":0,\n")
        file.write("      \"luck\":0,\n")
        file.write("      \"hp\":0,\n")
        file.write("      \"block\":0,\n")
        file.write("      \"dodge\":0,\n")
        file.write("      \"magic\":0,\n")
        file.write("      \"mana\":0,\n")
        file.write("      \"movement\":0,\n")
        file.write("      \"speed\":0,\n")
        file.write("      \"attention\":0,\n")
        file.write("      \"face\":0,\n")
        file.write("      \"stealth\":0,\n")
        file.write("      \"luckyPoints\":0,\n")
        file.write("      \"resolve\":0,\n")
        file.write("      \"primaryAttacks\":0,\n")
        file.write("      \"primaryAttackAttempt\":0,\n")
        file.write("      \"primaryDamage\":0,\n")
        file.write("      \"secondaryAttacks\":0,\n")
        file.write("      \"secondaryAttackAttempt\":0,\n")
        file.write("      \"secondaryDamage\":0,\n")
        file.write("      \"castingAttacks\":0,\n")
        file.write("      \"castingAttackAttempt\":0,\n")
        file.write("      \"castingDamage\":0,\n")
        file.write("      \"castingHealing\":0,\n")
        file.write("      \"initiative\":0,\n")
        file.write("      \"spellLimit\":0,\n")
        file.write("      \"mainstay\":0,\n")
        file.write("      \"protection\":0},\n")
        file.write("    \"requirementsLabel\":\"\", \"xpCost\":10, \"timeCost\":1, \"costLabel\":\"TEMP\",\n")
        file.write("    \"potentialChanges\":[],\n")
        file.write("    \"tags\":[\"combat\", \"rp\", \"offensive\", \"defensive\", \"support\", \"utility\", \"buff\", \"debuff\", \"heal\", \"casting\", \"misc\"]\n")
        if i != number_of_entries - 1:
            file.write("  },\n")
        else:
            file.write("  }\n")
    file.write("]")
    file.close()

if __name__ == "__main__":
    main()