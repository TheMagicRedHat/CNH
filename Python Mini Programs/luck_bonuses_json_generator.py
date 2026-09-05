import random as r
from math import comb
from math import floor

def main():
    number_of_entries = 19
    file_name = input("Please enter a file name (excluding file extention)\n")
    file = open(file_name + ".json", "w")
    file.write("[\n")
    for i in range(number_of_entries):
        file.write("  \"" + str(i+2) + "\":{\n")
        file.write("    \"hp\":0,\n")
        file.write("    \"block\":1,\n")
        file.write("    \"dodge\":1,\n")
        file.write("    \"magic\":0,\n")
        file.write("    \"mana\":0,\n")
        file.write("    \"movement\":0,\n")
        file.write("    \"attention\":2,\n")
        file.write("    \"face\":2,\n")
        file.write("    \"stealth\":2,\n")
        file.write("    \"luckyPoints\":2,\n")
        file.write("    \"resolve\":2,\n")
        file.write("    \"primaryAttempt\":1,\n")
        file.write("    \"secondaryAttempt\":1,\n")
        file.write("    \"castingAttempt\":1,\n")
        file.write("    \"initiative\":0,\n")
        file.write("    \"spellLimit\":0,\n")
        file.write("    \"mainstay\":0,\n")
        file.write("    \"protection\":0\n")
        if i != number_of_entries - 1:
            file.write("  },\n")
        else:
            file.write("  }\n")
    file.write("]")
    file.close()

if __name__ == "__main__":
    main()