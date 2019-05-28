using UnityEngine;

class NameGenerator
{
    public struct city
    {
        public string name;
        public bool usePrefix;
        public bool useTypeAffix;

        public city (string _name, bool _typeAffix = false, bool _prefix = true)
        {
            name          = _name;
            usePrefix     = _prefix;
            useTypeAffix  = _typeAffix;
        }
    }

    public static readonly string[] cityPrefixes =
    {
        "Mega", "Giga", "Tera", "Neo", "Nova", "Robo", "Mars", "Moon", "Old", "New",
        "Space", "Los", "Las", "East", "West", "North", "South", "Not", "Mid", "Super",
        "Ultra", "Mecha", "Hot"
    };

    //If a - is the first character append it without using spaces
    public static readonly string[] cityTypeAffixes =
    {
        "city", "dome", "town", "-tropolis", "-opolis", "-tron", "-town", "-land"
    };

    public static readonly city[] cityNames =
    {
        new city("Defunct", true, false),
        new city("Junk", true, false),
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
        new city("Pacific", true),
        new city("Atlantic", true),
        new city("Antarctica", true),
        new city("Sydney"),
        new city("Melbourne"),
        new city("Texas", true),
        new city("Florida", true),
        new city("Atlanta"), 
        new city("Kentucky", true, true),
        new city("China", true, true),
        new city("Australia", true, true),
        new city("Atlantis", true, true),
        new city("Dirt", true, false),
        new city("Crap", true, false),
        new city("Fart", true, false),
        new city("Moon", true, true),
        new city("Austin"),
        new city("Paris"),
        new city("Dubai"),
        new city("Seoul"),
        new city("Signapore"),
        new city("Rome"),
        new city("Jerusalem"),
        new city("Bangkok"),
        new city("Madrid"),
        new city("Amsterdam"),
        new city("Rio"),
        new city("Dublin"),
        new city("Toronto"),
        new city("Havana"),
        new city("Kyoto")
    };

    public static readonly string[] teamNames =
    {
        "Killbots", "Tryhards", "Shitpiles", "Junkers",
        "Errors", "Deprecated", "Raytracers", "Overclockers",
        "Exceptions", "Blazers", "Zeroes", "Integers",
        "Novas", "Martians", "Quasars", "Electrons",
        "Hadrons", "Borts", "Junkheaps", "Switchers",
        "Gigabits", "Teraflops", "Nulls", "Wildcats",
        "Housecats", "Roombas", "Supers", "Wizards",
        "Warlocks", "Warlords", "Nerds", "Dorks",
        "Geeks", "Weirdos", "Jerks", "Jerkfaces",
        "Crapotrons", "Flying Toasters", "Donkeys", "Earthlings",
        "Humans", "Robos", "Spotters", "Weaklings",
        "Baddies", "Not-Zees", "Clowns", "Morons",
        "Idiots", "Sexbots", "Dongs", "Dongles",
        "Buttholes", "Jebs", "Creepers", "Spiders",
        "Skeletons", "Zombies", "Terminals", "Roaches",
        "Ones", "Draculas", "Warez", "Waluigis",
        "Furbys", "ROBS", "Battlebots", "Ruxpins",
        "Terminators", "Drones", "Clanks", "Clunkers",
        "Floats", "Arrays"
    };


    public static readonly string[] robotSuffixes =
    {
        "Esquire", "III", "IV", "MXVII", "Jr.", "Sr.", "2.0", "2.1", "The Refurbished",
        "M.D.", "The Wretched", "The Great", "The Busted", "2500", "3500", "XL", "XXL",
        "486", "386", "At Law", "Owes me 20 bucks"
    };

    public static readonly string[] robotFirstNames =
    {
        "Bobson", "Bob", "John", "Jack", "Rob", "Jerkface", "Bort",
        "Samantha", "Emily", "Sarah", "Johnny", "Kane", "Al",
        "Linux", "Henry", "Hank", "Honk", "Junk", "Jeff", "Jorge",
        "Kilroy", "Moon", "Homer", "00101101", "10011001", "0xFFAAFE",
        "Weedlord", "Terminus", "Vlad", "Dunk", "Marvin", "Talos",
        "Borg", "Carl", "Lisa", "Lenny", "Alita", "Alexander", "Tim",
        "Jim", "PlayerName", "Guybot", "Girlbot", "Dudebot", "Seymour",
        "Turd", "Trombone", "Fork", "Spoon", "Spork", "Jimmy", "Timmy",
        "Bobby", "Jesus", "Mario", "Luigi", "Wario", "Dwight", "Dork",
        "RNGeejus", "Mike", "Michael"
    };

    public static readonly string[] robotLastNames =
    {
        "Dugnutt", "McGee", "McGill", "Blargenstein", "Dorkinson",
        "Connor", "Five", "Sixty-Nine", "Borkenson", "Jerkenorf",
        "Borkensoft", "Platinum", "Aluminum", "Jones", "Jackson",
        "Junkenstein", "Bonzalez", "McBain", "Toyota", "Honda",
        "Dinkleburg", "Barfenhargen", "Moon", "Fartmaster", "Bonerhitler",
        "Astro", "Dunkenstein", "Truck", "10011101", "10101010",
        "Whatshisface", "PlayerName", "Wonka", "Willy", "Skinner",
        "Chalmers", "0xFFAAFF", "0xFF0000", "Christ", "Satan", "Mario"
    };

    public static readonly string[] robotTitles =
    {
        "Dr", "Prime Minister", "Senator", "President", "Governor",
        "Their holiness", "Prophet", "The honorable", "Bishop", "Pope",
        "Mr", "Ms", "Mrs", "Space Sergeant", "Admiral", "Commander",
        "Groundskeeper", "Warden", "Principal", "Superintendent",
        "Our Lord and Savior"
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
            result += (affix[0] == '-' ? affix.Substring(1) : affix);
        }

        result += " " + teamNames[Random.Range(0, teamNames.Length)];

        return result; 
    }

    public static string GenerateRobotName()
    {
        bool useSuffix = Random.Range(0.0f, 1.0f) > 0.95f;
        bool useTitle  = Random.Range(0.0f, 1.0f) > 0.9f;

        return (useTitle ? robotTitles[Random.Range(0, robotTitles.Length)] + " " : "") +
        robotFirstNames[Random.Range(0, robotFirstNames.Length)] + " " +
            robotLastNames[Random.Range(0, robotLastNames.Length)] + " " +
            (useSuffix ? robotSuffixes[Random.Range(0, robotSuffixes.Length)] : ""); 
    }
}