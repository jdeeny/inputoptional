using UnityEngine;

class NameGenerator
{
    public struct city
    {
        public string name;
        public bool usePrefix;
        public bool useTypeAffix;
        public bool useExtraAffix;

        public city (string _name, bool _extraAffix = false, bool _typeAffix = false, bool _prefix = true)
        {
            name          = _name;
            usePrefix     = _prefix;
            useTypeAffix  = _typeAffix;
            useExtraAffix = _extraAffix; 
        }
    }

    public static readonly string[] cityPrefixes =
    {
        "Mega", "Giga", "Tera", "Neo", "Nova", "Robo", "Mars", "Moon", "Old", "New",
        "Space", "Los", "Las", "East", "West", "North", "South"
    };

    //If a - is the first character append it without using spaces
    public static readonly string[] cityTypeAffixes =
    {
        "City", "Dome", "Town", "-tropolis", "-opolis", "-otron"
    };

    public static readonly string[] cityExtraAffixes =
    {
        "One", "2.0", "14-B", "13-A", "64-Z", "D-45", "GT"
    };

    public static readonly city[] cityNames =
    {
        new city("Junk", true, true, false),
        new city("York"),
        new city("Boston"),
        new city("Angeles"),
        new city("Dallas"),
        new city("Hamburg"),
        new city("Berlin"),
        new city("Scranton"),
        new city("Tokyo"),
        new city("Deigo"),
        new city("Quebec"),
        new city("Vancouver"),
        new city("Seattle"),
        new city("Baltimore"),
        new city("Vegas"),
        new city("Houston"),
        new city("Orleans"),
        new city("London"),
        new city("Glasgow"),
        new city("Washington"),
        new city("Moscow"),
        new city("Miami"),
        new city("Francisco"),
        new city("Pacific", true, true),
        new city("Atlantic", true, true),
        new city("Antarctica", true, true),
        new city("Sydney"),
        new city("Melbourne"),
        new city("Texas", true, true),
        new city("Florida", false, true)
    };

    public static readonly string[] teamNames =
    {
        "Killbots", "Tryhards", "Shitpiles", "Junkers",
        "Errors", "Deprecated", "Raytracers", "Overclockers",
        "Exceptions", "Blazers", "Zeroes", "Integers",
        "Novas", "Martians", "Quasars", "Electrons",
        "Hadrons", "Borts", "Junkheaps", "Switchers",
        "Gigabits", "Teraflops"
    };


    public static readonly string[] robotSuffixes =
    {
        "Esquire", "III", "IV", "MXVII", "Jr.", "Sr.", "2.0", "2.1", "(Refurbished)",
        "M.D."
    };

    public static readonly string[] robotFirstNames =
    {
        "Bobson", "Bob", "John", "Jack", "Rob", "Jerkface", "Bort",
        "Samantha", "Emily", "Sarah", "Johnny", "Kane", "Al",
        "Linux", "Henry", "Hank", "Honk", "Junk", "Jeff", "Jorge"
    };

    public static readonly string[] robotLastNames =
    {
        "Dugnutt", "McGee", "McGill", "Blargenstein", "Dorkinson",
        "Connor", "Five", "Sixty-Nine", "Borkenson", "Jerkenorf",
        "Borkensoft", "Platinum", "Aluminum", "Jones", "Jackson",
        "Junkenstein", "Bonzalez", "McBain", "Toyota", "Honda"
    };

    public static string GenerateCityName()
    {
        string result = ""; 
        city citySeed = cityNames[Random.Range(0, cityNames.Length)];

        result = citySeed.name;

        if (citySeed.usePrefix) result = cityPrefixes[Random.Range(0, cityPrefixes.Length)] + " " + result;

        if (citySeed.useTypeAffix)
        {
            string affix = cityTypeAffixes[Random.Range(0, cityTypeAffixes.Length)]; 
            result += (affix[0] == '-' ? "" : " ") + affix;
        }

        if (citySeed.useExtraAffix) result += " " + cityExtraAffixes[Random.Range(0, cityExtraAffixes.Length)];

        return result; 
    }

    public static string GenerateRobotName()
    {
        bool useSuffix = Random.Range(0.0f, 1.0f) > 0.95f;

        return robotFirstNames[Random.Range(0, robotFirstNames.Length)] + " " +
            robotLastNames[Random.Range(0, robotLastNames.Length)] + " " +
            (useSuffix ? robotSuffixes[Random.Range(0, robotSuffixes.Length)] : ""); 
    }
}