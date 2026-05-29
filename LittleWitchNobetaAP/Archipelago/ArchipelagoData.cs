using System.Collections.Immutable;
using Il2Cpp;
using LittleWitchNobetaAP.Utils;
using UnityEngine;

namespace LittleWitchNobetaAP.Archipelago;

public enum BarrierType
{
    MagicPuzzle,
    MetalGate,
}

public class BarrierMapping
{
    public StageId StageId { get; init; }
    public BarrierType Type { get; init; } = BarrierType.MagicPuzzle;
    public string LocationName { get; init; } = "";
    public string ItemName { get; init; } = "";
    public string TriggerPath { get; init; } = "";
    public List<BarrierAction> Actions { get; init; } = new();
}

public class StageLoadAction
{
    public StageId StageId { get; init; }
    public string? ItemName { get; init; }
    public List<BarrierAction> Actions { get; init; } = new();
}

public class CutsceneTrigger
{
    public StageId StageId { get; init; }
    public string Trigger { get; init; } = "";
    public Func<bool> ShouldSkip { get; init; } = () => true;
}

public class BossTrigger
{
    public StageId StageId { get; init; }
    public string SoulName { get; init; } = "";
    public string Trigger { get; init; } = "";
}

public static class ArchipelagoData
{
    public static readonly SortedDictionary<string, string> Locations = new(StringComparer.Ordinal)
    {
        { "Shrine - 6. Broken Cross Spear from first ranged Enemy", "Lore" },
        { "Shrine - 1. Crafted Soul Reader from pot in side alcove", "Lore" },
        { "Shrine - Chest at first magic switch barrier", "Chest" },
        { "Shrine - Chest on platform at first magic switch barrier", "Chest" },
        { "Shrine - First magic switch", "Barrier" },
        { "Shrine - 3. Copper Coin in Grand Hall statue barrel", "Lore" },
        { "Shrine - Second magic switch", "Barrier" },
        { "Shrine - Meet Cat barrier", "Barrier" },
        { "Shrine - Chest behind gate", "Chest" },
        { "Shrine - 12. Pointy Witch's Hat in pot at chest behind gate", "Lore" },
        { "Shrine - 7. Deformed Cavalier Armor from Enemy before Central Hall", "Lore" },
        { "Shrine - Specter Armor", "Bosses" },
        { "Shrine - Secret passage magic switch", "Barrier" },
        { "Shrine - Underground shortcut gate switch", "Metal Gate" },
        { "Shrine - 14. Corpse Shroud from pot in Underground shortcut", "Lore" },
        { "Secret Passage - 35. Dwarven Metalwork in pot after first drop", "Lore" },
        { "Secret Passage - 36. High Elf's Mana Ring in pot beside destructible wall", "Lore" },
        { "Secret Passage - 11. Broken Queen Doll from enemy behind destructible wall", "Lore" },
        { "Secret Passage - Absorption spell chest behind destructible wall", "Chest" },
        { "Secret Passage - First fire barrier magic switch", "Barrier" },
        { "Secret Passage - 37. Forest Elf's Vest from enemy before hole in floor", "Lore" },
        { "Secret Passage - Wind spell chest in alcove during fall", "Chest" },
        { "Secret Passage - Ice spell chest behind breakable wall after fall", "Chest" },
        { "Secret Passage - 4. Unknown House Banner from big enemy at spiral stairs", "Lore" },
        { "Secret Passage - Secret area shortcut gate switch", "Metal Gate" },
        { "Secret Passage - Fire spell chest at second fire barrier magic switch", "Chest" },
        { "Secret Passage - Second fire barrier magic switch", "Barrier" },
        { "Secret Passage - 38. Dark Elf's Ear Sample from pot at magic barrier", "Lore" },
        { "Secret Passage - 19. Knight's Halberd from pot before boss", "Lore" },
        { "Secret Passage - Enraged Armor", "Bosses" },
        { "Secret Passage - 56. Knight Kingdom Crown from Enraged Armor", "Lore" },
        { "Secret Passage - Teleport from Enraged Armor", "Teleport" },
        { "Secret Passage - Defeat Enraged Armor barrier", "Barrier" },
        { "Secret Passage - Thunder spell chest after boss", "Chest" },
        { "Secret Passage - Boss shortcut gate switch", "Metal Gate" },
        { "Secret Passage - 13. Sleeve Dagger", "Lore" },
        { "Secret Passage - Dark Tunnel shortcut gate switch", "Metal Gate" },
        { "Underground - 8. Hero's Cross Sword", "Lore" },
        { "Underground - 9. Giant Axe", "Lore" },
        { "Underground - 10. Shield of the Church", "Lore" },
        { "Underground - 97. Faithful Soul Shard", "Lore" },
        { "Underground - Wind spell chest", "Chest" },
        { "Underground - 18. Soul Doll Remnant from first doll enemy", "Lore" },
        { "Underground - Chest in alcove before falling rocks", "Chest" },
        { "Underground - Chest across staircase on backtrack path", "Chest" },
        { "Underground - 2. Stained Ribbon down on glowing rock", "Lore" },
        { "Underground - Arcane chest at bridge jumping puzzle", "Chest" },
        { "Underground - Ice spell chest in bottom pit", "Chest" },
        { "Underground - 17. Tattered Maid Outfit from maid enemy", "Lore" },
        { "Underground - Cat absorption hint & gift", "Item" },
        { "Underground - Magic barrier switches at maid enemy", "Barrier" },
        { "Underground - Lava ruins shortcut gate switch", "Metal Gate" },
        { "Underground - 15. Headcutter Circular Saw", "Lore" },
        { "Underground - Tania shortcut switch on statue side", "Metal Gate" },
        { "Underground - 16. Test Subject Manacle from barrel after fire", "Lore" },
        { "Underground - Chest after fire", "Chest" },
        { "Underground - After fire magic switch", "Barrier" },
        { "Underground - Defeat enemies barrier", "Barrier" },
        { "Underground - Defeat Tania", "Bosses" },
        { "Underground - Tania boss arena barrier", "Barrier" },
        { "Underground - Tania shortcut switch on Tania side", "Metal Gate" },
        { "Underground - 98. Lost Maiden's Soul Shard from Tania", "Lore" },
        { "Lava Ruins - 26. Teddy Bear", "Lore" },
        { "Lava Ruins - 30. Merchant's Ledger from enemy", "Lore" },
        { "Lava Ruins - Chest on scaffolding", "Chest" },
        { "Lava Ruins - Chest on left side lava ledge", "Chest" },
        { "Lava Ruins - 29. Slave Branding Iron in barrel after dropping down", "Lore" },
        { "Lava Ruins - Absorption spell chest after dropping down", "Chest" },
        { "Lava Ruins - Chest on right ruins ledge at shotgun enemies magic switch", "Chest" },
        { "Lava Ruins - Magic platform switch at shotgun enemies", "Barrier" },
        { "Lava Ruins - 27. Fractured Stone Axe from red enemy behind destructible wall", "Lore" },
        { "Lava Ruins - Wind spell chest behind destructible walls", "Chest" },
        { "Lava Ruins - 20. Weathered Cloak in corner on ruins top", "Lore" },
        { "Lava Ruins - 28. Cage on path to starting area shortcut gate", "Lore" },
        { "Lava Ruins - Fake floor shortcut gate switch", "Metal Gate" },
        { "Lava Ruins - Fake floor bait item", "Item" },
        { "Lava Ruins - 23. Cursed Turquoise Necklace from scissor enemy", "Lore" },
        { "Lava Ruins - Defeat scissor enemy barrier", "Barrier" },
        { "Lava Ruins - Lift magic switch at scissor enemy", "Barrier" },
        { "Lava Ruins - 32. Slave Collar from ranged enemy in corner at spewing lava", "Lore" },
        { "Lava Ruins - Chest in spewing lava room", "Chest" },
        { "Lava Ruins - Fire spell chest at double staircase", "Chest" },
        { "Lava Ruins - 31. Slave Tag from pot atop of double staircase", "Lore" },
        { "Lava Ruins - Fire magic switch", "Barrier" },
        { "Lava Ruins - 25. Copper Ingot on path through hole in wall", "Lore" },
        { "Lava Ruins - 22. Intricate Clock from barrel in lava maze", "Lore" },
        { "Lava Ruins - Chest in lava maze", "Chest" },
        { "Lava Ruins - Activate moving ring", "Chest" },
        { "Lava Ruins - Jumping puzzle arcane chest at moving ring gauntlet", "Chest" },
        { "Lava Ruins - Monica shortcut switch", "Metal Gate" },
        { "Lava Ruins - 21. Silver Coin from barrel at Monica statue", "Lore" },
        { "Lava Ruins - 24. Glass Lantern from scissor enemy", "Lore" },
        { "Lava Ruins - Monica", "Bosses" },
        { "Lava Ruins - Monica boss arena barrier", "Barrier" },
        { "Lava Ruins - 34. Bestian Ear from Monica", "Lore" },
        { "Lava Ruins - 33. Bestian Palm from Monica", "Lore" },
        { "Lava Ruins - 99. Child's Soul Shard from Monica", "Lore" },
        { "Lava Ruins - Monica warp gate switch", "Metal Gate" },
        { "Lava Ruins - Ice spell chest in corner on path to dark tunnel", "Chest" },
        { "Lava Ruins - 5. Melted Silver Candlestick in front of underground shortcut", "Lore" },
        { "Dark Tunnel - 39. Dark Elf's Short Bow from barrel on scaffolding", "Lore" },
        { "Dark Tunnel - 43. Bloodstained Javelin from barrel at statue", "Lore" },
        { "Dark Tunnel - 40. Ogre's Kidney from first shield enemy", "Lore" },
        { "Dark Tunnel - 41. Ogre's Eye from first ranged enemy", "Lore" },
        { "Dark Tunnel - 44. Nomad's Cookware from barrel at magic barrier switch", "Lore" },
        { "Dark Tunnel - First magic barrier switch", "Barrier" },
        { "Dark Tunnel - 42. Ogre's Club from shield enemy at gate switch", "Lore" },
        { "Dark Tunnel - First gate switch", "Metal Gate" },
        { "Dark Tunnel - Absorption spell chest hidden behind rubble at staircase", "Chest" },
        { "Dark Tunnel - 45. Golden Coin from first mimic", "Lore" },
        { "Dark Tunnel - Chest at jump from broken staircase", "Chest" },
        { "Dark Tunnel - 46. Tooth Thief's Pouch from barrel at scaffolding", "Lore" },
        { "Dark Tunnel - Wind spell chest in dark hole", "Chest" },
        { "Dark Tunnel - 50. Crafted Soul Injector from pot at light switch after getting the hat", "Lore" },
        { "Dark Tunnel - Light switch after getting the hat", "Barrier" },
        { "Dark Tunnel - 57. Lady's Feather Hat from puppeteer", "Lore" },
        { "Dark Tunnel - 47. Blood Orc's Skin Sample from barrel above dark maze", "Lore" },
        { "Dark Tunnel - Fire spell chest inside dark maze", "Chest" },
        { "Dark Tunnel - 48. Chief's Skull from right mimic in mimic room", "Lore" },
        { "Dark Tunnel - 49. Chief's Skull from straight mimic in mimic room", "Lore" },
        { "Dark Tunnel - Thunder spell chest in mimic room", "Chest" },
        { "Dark Tunnel - 53. Ceremonial Sword from barrel at second statue", "Lore" },
        { "Dark Tunnel - Thunder barrier magic switches", "Barrier" },
        { "Dark Tunnel - 51. Hero's Insignia", "Lore" },
        { "Dark Tunnel - Chest in alcove", "Chest" },
        { "Dark Tunnel - 54. Banner of the Lance Hero from lightning enemy", "Lore" },
        { "Dark Tunnel - Floating platform switch one", "Barrier" },
        { "Dark Tunnel - Floating platform switch two", "Barrier" },
        { "Dark Tunnel - Floating platform switch three", "Barrier" },
        { "Dark Tunnel - Chest on left side after floating platforms", "Chest" },
        { "Dark Tunnel - Arcane spell chest on collapsed bridge", "Chest" },
        { "Dark Tunnel - 52. Mutated Beast Claw from pot in big hall", "Lore" },
        { "Dark Tunnel - 69. Pontiff's Scepter from singular barrel in side room", "Lore" },
        { "Dark Tunnel - 71. Apocalypse Knight Record from knight enemy", "Lore" },
        { "Dark Tunnel - 103. Loyal Soul Shard from knight enemy", "Lore" },
        { "Dark Tunnel - Chest after knight enemy", "Chest" },
        { "Dark Tunnel - 65. Declaration of War from barrel at statue", "Lore" },
        { "Dark Tunnel - Vanessa", "Bosses" },
        { "Dark Tunnel - 100. King's Final Honor from Vanessa", "Lore" },
        { "Dark Tunnel - 78. Ancient Throne Rune from Vanessa", "Lore" },
        { "Dark Tunnel - 77. The Throne from Vanessa", "Lore" },
        { "Spirit Realm - 72. Apocalypse Knight Shield from crystal at statue", "Lore" },
        { "Spirit Realm - 73. Apocalypse Knight Axe from enemy in trapped barrel", "Lore" },
        { "Spirit Realm - 79. Doctor's Mask hidden on ledge", "Lore" },
        { "Spirit Realm - Wind spell chest inside roof hole", "Chest" },
        { "Spirit Realm - Wind spell chest gate switch", "Metal Gate" },
        { "Spirit Realm - 55. Banner of the Lionhearted from crystal in doorway", "Lore" },
        { "Spirit Realm - 75. Apocalypse Knight Bow from bow enemy", "Lore" },
        { "Spirit Realm - 61. Knight Kingdom Entry Pass from sword enemy", "Lore" },
        { "Spirit Realm - 58. Super Cleanser Soap from crystal on path", "Lore" },
        { "Spirit Realm - Chest hidden in dropdown alcove", "Chest" },
        { "Spirit Realm - 63. Inquisitor's List from crystal at second statue", "Lore" },
        { "Spirit Realm - Ice spell chest in right side alcove", "Chest" },
        { "Spirit Realm - Ice spell chest gate switch in right side alcove", "Metal Gate" },
        { "Spirit Realm - 76. Apocalypse Knight Sword from enemy", "Lore" },
        { "Spirit Realm - 74. Apocalypse Knight Staff from enemy", "Lore" },
        { "Spirit Realm - Arcane barrier magic switches", "Barrier" },
        { "Spirit Realm - Thunder spell chest behind breakable wall", "Chest" },
        { "Spirit Realm - 86. Missing Person Poster on bridge", "Lore" },
        { "Spirit Realm - Platform shortcut switch", "Barrier" },
        { "Spirit Realm - 59. Exquisite Leather Lamp from crystal before Seal", "Lore" },
        { "Spirit Realm - 66. Attic Key from crystal at first Seal phase", "Lore" },
        { "Spirit Realm - 64. Envoy's Rune from crystal at first Seal phase", "Lore" },
        { "Spirit Realm - 60. Premium Grass Ash from crystal at first Seal phase", "Lore" },
        { "Spirit Realm - 62. Bone Chess Set from crystal at first Seal phase", "Lore" },
        { "Spirit Realm - First Seal magic barrier", "Barrier" },
        { "Spirit Realm - 90. Enchanted Shackles from second Seal phase", "Lore" },
        { "Spirit Realm - Fire spell chest at second Seal phase", "Chest" },
        { "Spirit Realm - Second Seal magic barrier", "Barrier" },
        { "Spirit Realm - 87. Saint's Cane from crystal at third statue", "Lore" },
        { "Spirit Realm - Statue shortcut gate switch", "Metal Gate" },
        { "Spirit Realm - 67. Halfling's Forelimb from crystal at elevator magic switch", "Lore" },
        { "Spirit Realm - Elevator magic switch", "Barrier" },
        { "Spirit Realm - 68. Ratian Claw from crystal after elevator", "Lore" },
        { "Spirit Realm - Absorption spell chest behind crystals after elevator", "Chest" },
        { "Spirit Realm - Fire control magic switch", "Barrier" },
        { "Spirit Realm - Magic switch barrier switch", "Barrier" },
        { "Spirit Realm - Teleporter magic switch", "Barrier" },
        { "Spirit Realm - 70. Abandoned Rag Doll from crystal on spiral stairs", "Lore" },
        { "Spirit Realm - Chest before boss behind breakable wall", "Chest" },
        { "Spirit Realm - 89. Bloodstained Key at statue before boss", "Lore" },
        { "Spirit Realm - Vanessa V2", "Bosses" },
        { "Spirit Realm - 101. Proud King's Crafted Soul Shard from Vanessa V2", "Lore" },
        { "Spirit Realm - Thunder spell from Vanessa V2", "Item" },
        { "Abyss - 80. Ceremonial Incense at first statue", "Lore" },
        { "Abyss - First gate switch", "Metal Gate" },
        { "Abyss - 81. Moonlight Blade from trapped barrel", "Lore" },
        { "Abyss - 83. Castle Blueprint from crystal on brittle ledge", "Lore" },
        { "Abyss - Arcane spell chest on pillar", "Chest" },
        { "Abyss - 84. Witch Worshipper Puppet from pot right of pillars", "Lore" },
        { "Abyss - Giant maid barrier", "Barrier" },
        { "Abyss - Trap gate trigger", "Metal Gate" },
        { "Abyss - 82. Prostitute's Chiffon from crystal in trap gate", "Lore" },
        { "Abyss - Chest in trap gate", "Chest" },
        { "Abyss - 85. Polymorphism Scroll from crystal behind gate", "Lore" },
        { "Abyss - 88. Hero Summon Rune in front of statue", "Lore" },
        { "Abyss - Fire Spell chest underground trial", "Chest" },
        { "Abyss - Underground trial activate scissor enemies magic switch", "Barrier" },
        { "Abyss - 91. Gaseous Soul Essence from scissor enemy in underground trial", "Lore" },
        { "Abyss - Underground trial scissor enemy barrier", "Barrier" },
        { "Abyss - Underground trial magic switch", "Barrier" },
        { "Abyss - Underground Trial Complete", "Abyss Trial" },
        { "Abyss - 92. Semi-gaseous Soul Essence in front of underground trial magic switch", "Lore" },
        { "Abyss - Thunder spell chest dark tunnel trial", "Chest" },
        { "Abyss - 95. Refined Soul Shard from maid enemy in dark tunnel trial", "Lore" },
        { "Abyss - Dark tunnel trial maid enemy barrier", "Barrier" },
        { "Abyss - Dark Tunnel trial magic switch", "Barrier" },
        { "Abyss - Dark Tunnel Trial Complete", "Abyss Trial" },
        { "Abyss - 96. Knight's Soul Shard in front of dark tunnel trial magic switch", "Lore" },
        { "Abyss - Ice spell chest lava ruins trial", "Chest" },
        { "Abyss - Lava Ruins trial lower lava switch", "Barrier" },
        { "Abyss - 93. Enchanted Soul Shard from maid enemy in lava ruins trial", "Lore" },
        { "Abyss - Lava Ruins trial defeat maids enemy barrier", "Barrier" },
        { "Abyss - Lava Ruins trial magic switch", "Barrier" },
        { "Abyss - Lava Ruins Trial Complete", "Abyss Trial" },
        { "Abyss - 94. Knight's Soul Shard in front of lava ruins trial magic switch", "Lore" },
        { "Abyss - 102. Lost Maiden's Crafted Soul Shard from Nonota", "Lore" },
        { "Abyss - Nonota", "Event" }
    };

    public static readonly SortedDictionary<string, string> Items = new(StringComparer.Ordinal)
    {
        { "Arcane", "Attack Magics" },
        { "Ice", "Attack Magics" },
        { "Fire", "Attack Magics" },
        { "Thunder", "Attack Magics" },
        { "Wind", "Double Jump" },
        { "Mana Absorption", "Counter" },
        { "Progressive Bag Upgrade", "Bag Upgrade" },
        { "Specter Armor Soul", "Boss Souls" },
        { "Tania Soul", "Boss Souls" },
        { "Monica Soul", "Boss Souls" },
        { "Enraged Armor Soul", "Boss Souls" },
        { "Vanessa Soul", "Boss Souls" },
        { "Vanessa V2 Soul", "Boss Souls" },
        { "Specter Armor Token", "Boss Tokens" },
        { "Tania Token", "Boss Tokens" },
        { "Monica Token", "Boss Tokens" },
        { "Enraged Armor Token", "Boss Tokens" },
        { "Vanessa Token", "Boss Tokens" },
        { "Vanessa V2 Token", "Boss Tokens" },
        { "Meager Life Crystal", "Filler" },
        { "Faint Life Crystal", "Filler" },
        { "Fair Life Crystal", "Filler" },
        { "Fine Life Crystal", "Filler" },
        { "Meager Magic Crystal", "Filler" },
        { "Faint Magic Crystal", "Filler" },
        { "Fair Magic Crystal", "Filler" },
        { "Fine Magic Crystal", "Filler" },
        { "Faint Defense Crystal", "Filler" },
        { "Fair Defense Crystal", "Filler" },
        { "Fine Defense Crystal", "Filler" },
        { "Faint Arcane Crystal", "Filler" },
        { "Fair Arcane Crystal", "Filler" },
        { "Fine Arcane Crystal", "Filler" },
        { "Faint Holy Crystal", "Filler" },
        { "Fair Holy Crystal", "Filler" },
        { "Fine Holy Crystal", "Filler" },
        { "Souls", "Filler" },
        { "HP Souls", "Filler" },
        { "MP Souls", "Filler" },
        { "Bonk Trap", "Trap" },
        { "Mana Drain Trap", "Trap" },
        { "1. Crafted Soul Reader", "Lore" },
        { "2. Stained Ribbon", "Lore" },
        { "3. Copper Coin", "Lore" },
        { "4. Unknown House Banner", "Lore" },
        { "5. Melted Silver Candlestick", "Lore" },
        { "6. Broken Cross Spear", "Lore" },
        { "7. Deformed Cavalier Armor", "Lore" },
        { "8. Hero's Cross Sword", "Lore" },
        { "9. Giant Axe", "Lore" },
        { "10. Shield of the Church", "Lore" },
        { "11. Broken Queen Doll", "Lore" },
        { "12. Pointy Witch's Hat", "Lore" },
        { "13. Sleeve Dagger", "Lore" },
        { "14. Corpse Shroud", "Lore" },
        { "15. Headcutter Circular Saw", "Lore" },
        { "16. Test Subject Manacle", "Lore" },
        { "17. Tattered Maid Outfit", "Lore" },
        { "18. Soul Doll Remnant", "Lore" },
        { "19. Knight's Halberd", "Lore" },
        { "20. Weathered Cloak", "Lore" },
        { "21. Silver Coin", "Lore" },
        { "22. Intricate Clock", "Lore" },
        { "23. Cursed Turquoise Necklace", "Lore" },
        { "24. Glass Lantern", "Lore" },
        { "25. Copper Ingot", "Lore" },
        { "26. Teddy Bear", "Lore" },
        { "27. Fractured Stone Axe", "Lore" },
        { "28. Cage", "Lore" },
        { "29. Slave Branding Iron", "Lore" },
        { "30. Merchant's Ledger", "Lore" },
        { "31. Slave Tag", "Lore" },
        { "32. Slave Collar", "Lore" },
        { "33. Bestian Palm", "Lore" },
        { "34. Bestian Ear", "Lore" },
        { "35. Dwarven Metalwork", "Lore" },
        { "36. High Elf's Mana Ring", "Lore" },
        { "37. Forest Elf's Vest", "Lore" },
        { "38. Dark Elf's Ear Sample", "Lore" },
        { "39. Dark Elf's Short Bow", "Lore" },
        { "40. Ogre's Kidney", "Lore" },
        { "41. Ogre's Eye", "Lore" },
        { "42. Ogre's Club", "Lore" },
        { "43. Bloodstained Javelin", "Lore" },
        { "44. Nomad's Cookware", "Lore" },
        { "45. Golden Coin", "Lore" },
        { "46. Tooth Thief's Pouch", "Lore" },
        { "47. Blood Orc's Skin Sample", "Lore" },
        { "48. Chief's Skull", "Lore" },
        { "49. Chief's Skull", "Lore" },
        { "50. Crafted Soul Injector", "Lore" },
        { "51. Hero's Insignia", "Lore" },
        { "52. Mutated Beast Claw", "Lore" },
        { "53. Ceremonial Sword", "Lore" },
        { "54. Banner of the Lance Hero", "Lore" },
        { "55. Banner of the Lionhearted", "Lore" },
        { "56. Knight Kingdom Crown", "Lore" },
        { "57. Lady's Feather Hat", "Lore" },
        { "58. Super Cleanser Soap", "Lore" },
        { "59. Exquisite Leather Lamp", "Lore" },
        { "60. Premium Grass Ash", "Lore" },
        { "61. Knight Kingdom Entry Pass", "Lore" },
        { "62. Bone Chess Set", "Lore" },
        { "63. Inquisitor's List", "Lore" },
        { "64. Envoy's Rune", "Lore" },
        { "65. Declaration of War", "Lore" },
        { "66. Attic Key", "Lore" },
        { "67. Halfling's Forelimb", "Lore" },
        { "68. Ratian Claw", "Lore" },
        { "69. Pontiff's Scepter", "Lore" },
        { "70. Abandoned Rag Doll", "Lore" },
        { "71. Apocalypse Knight Record", "Lore" },
        { "72. Apocalypse Knight Shield", "Lore" },
        { "73. Apocalypse Knight Axe", "Lore" },
        { "74. Apocalypse Knight Staff", "Lore" },
        { "75. Apocalypse Knight Bow", "Lore" },
        { "76. Apocalypse Knight Sword", "Lore" },
        { "77. The Throne", "Lore" },
        { "78. Ancient Throne Rune", "Lore" },
        { "79. Doctor's Mask", "Lore" },
        { "80. Ceremonial Incense", "Lore" },
        { "81. Moonlight Blade", "Lore" },
        { "82. Prostitute's Chiffon", "Lore" },
        { "83. Castle Blueprint", "Lore" },
        { "84. Witch Worshipper Puppet", "Lore" },
        { "85. Polymorphism Scroll", "Lore" },
        { "86. Missing Person Poster", "Lore" },
        { "87. Saint's Cane", "Lore" },
        { "88. Hero Summon Rune", "Lore" },
        { "89. Bloodstained Key", "Lore" },
        { "90. Enchanted Shackles", "Lore" },
        { "91. Gaseous Soul Essence", "Lore" },
        { "92. Semi-gaseous Soul Essence", "Lore" },
        { "93. Enchanted Soul Shard", "Lore" },
        { "94. Knight's Soul Shard", "Lore" },
        { "95. Refined Soul Shard", "Lore" },
        { "96. Knight's Soul Shard", "Lore" },
        { "97. Faithful Soul Shard", "Lore" },
        { "98. Lost Maiden's Soul Shard", "Lore" },
        { "99. Child's Soul Shard", "Lore" },
        { "100. King's Final Honor", "Lore" },
        { "101. Proud King's Crafted Soul Shard", "Lore" },
        { "102. Lost Maiden's Crafted Soul Shard", "Lore" },
        { "103. Loyal Soul Shard", "Lore" },
        { "Shrine First Magic Barrier", "Magic Barrier" },
        { "Shrine Second Magic Barrier", "Magic Barrier" },
        { "Shrine Meet Cat Magic Barrier", "Magic Barrier" },
        { "Secret Passage Entrance Magic Barrier", "Magic Barrier" },
        { "Secret Passage First Fire Barrier", "Magic Barrier" },
        { "Secret Passage Second Fire Barrier", "Magic Barrier" },
        { "Defeat Enraged Armor Barrier", "Magic Barrier" },
        { "Underground Magic Barrier At Maid Enemy", "Magic Barrier" },
        { "Underground Fire Barrier Magic Barrier", "Magic Barrier" },
        { "Underground Enemy Magic Barrier", "Magic Barrier" },
        { "Underground Tania Arena Barrier", "Magic Barrier" },
        { "Lava Ruins Magic Platforms", "Magic Barrier" },
        { "Lava Ruins Scissor Enemy Barrier", "Magic Barrier" },
        { "Lava Ruins Scissor Enemy Lift", "Magic Barrier" },
        { "Lava Ruins Fire Magic Barrier", "Magic Barrier" },
        { "Lava Ruins Moving Ring", "Magic Barrier" },
        { "Lava Ruins Monica Arena Barrier", "Magic Barrier" },
        { "Dark Tunnel First Magic Barrier", "Magic Barrier" },
        { "Dark Tunnel Light Switch Barrier", "Magic Barrier" },
        { "Dark Tunnel Thunder Barrier", "Magic Barrier" },
        { "Dark Tunnel Floating Platform One", "Magic Barrier" },
        { "Dark Tunnel Floating Platform Two", "Magic Barrier" },
        { "Dark Tunnel Floating Platform Three", "Magic Barrier" },
        { "Spirit Realm Arcane Barrier", "Magic Barrier" },
        { "Spirit Realm Platform Shortcut", "Magic Barrier" },
        { "Spirit Realm First Seal Magic Barrier", "Magic Barrier" },
        { "Spirit Realm Second Seal Magic Barrier", "Magic Barrier" },
        { "Spirit Realm Elevator", "Magic Barrier" },
        { "Spirit Realm Teleporter", "Magic Barrier" },
        { "Abyss After Giant Maid Barrier", "Magic Barrier" },
        { "Abyss Underground Trial Magic Switch", "Magic Barrier" },
        { "Abyss Underground Trial Exit Barrier", "Magic Barrier" },
        { "Abyss Underground Trial After Scissor Enemy Barrier", "Magic Barrier" },
        { "Abyss Lava Ruins Trial Magic Switch", "Magic Barrier" },
        { "Abyss Lava Ruins Trial Lower Lava", "Magic Barrier" },
        { "Abyss Dark Tunnel Trial Magic Switch", "Magic Barrier" },
        { "Abyss Dark Tunnel Trial Maid Enemy Barrier", "Magic Barrier" },
        { "Abyss Lava Ruins Trial Maid Enemy Barrier", "Magic Barrier" },
        { "Spirit Realm Fire Deactivation", "Magic Barrier" },
        { "Spirit Realm Magic Switch Barrier", "Magic Barrier" },
        { "Shrine Secret Boss Shortcut Gate", "Metal Gate" },
        { "Shrine Secret Area Shortcut Gate", "Metal Gate" },
        { "Shrine Underground Shortcut Gate", "Metal Gate" },
        { "Secret Passage Dark Tunnel Shortcut Gate", "Metal Gate" },
        { "Underground Lava Ruins Shortcut Gate", "Metal Gate" },
        { "Underground Tania Shortcut Gate On Grand Hall Side", "Metal Gate" },
        { "Underground Tania Shortcut Gate On Tania Side", "Metal Gate" },
        { "Lava Ruins Fake Floor Shortcut Gate", "Metal Gate" },
        { "Lava Ruins Monica Shortcut Gate", "Metal Gate" },
        { "Lava Ruins Monica Warp Gate", "Metal Gate" },
        { "Dark Tunnel First Gate", "Metal Gate" },
        { "Spirit Realm Wind Spell Chest Gate", "Metal Gate" },
        { "Spirit Realm Ice Spell Chest Gate", "Metal Gate" },
        { "Spirit Realm Statue Shortcut Gate", "Metal Gate" },
        { "Abyss First Gate", "Metal Gate" },
        { "Abyss Trap Gates", "Metal Gate" },
        { "Abyss Underground Trial Clear", "Abyss Trial Clear" },
        { "Abyss Lava Ruins Trial Clear", "Abyss Trial Clear" },
        { "Abyss Dark Tunnel Trial Clear", "Abyss Trial Clear" },
        { "Teleport", "Teleport" },
        { "Trial Key", "Trial Key" }
    };


    public static readonly Dictionary<string, ItemSystem.ItemType> DescriptiveItemToGameItemMap = new()
    {
        { "Arcane", ItemSystem.ItemType.MagicNull },
        { "Ice", ItemSystem.ItemType.MagicIce },
        { "Fire", ItemSystem.ItemType.MagicFire },
        { "Thunder", ItemSystem.ItemType.MagicLightning },
        { "Wind", ItemSystem.ItemType.SkyJump },
        { "Mana Absorption", ItemSystem.ItemType.Absorb },
        { "Progressive Bag Upgrade", ItemSystem.ItemType.BagMaxAdd },
        { "Souls", ItemSystem.ItemType.Null }
    };

    public static string GameLocationToDescriptiveLocation(string gameLocation)
    {
        var descriptiveLocation = gameLocation switch
        {
            // shrine_start_locations
            "TreasureBox_Room03" => "Shrine - Chest at first magic switch barrier",
            "TreasureBox02_Room03" => "Shrine - Chest on platform at first magic switch barrier",
            // shrine_armor_locations
            "TreasureBox_Room05" => "Shrine - Chest behind gate",
            "Boss_Act01" => "Shrine - Specter Armor",
            // shrine_secret_passage_locations
            "TreasureBox07" => "Secret Passage - Absorption spell chest behind destructible wall",
            "TreasureBox07To08" => "Secret Passage - Wind spell chest in alcove during fall",
            "TreasureBox08" => "Secret Passage - Ice spell chest behind breakable wall after fall",
            "TreasureBox09" => "Secret Passage - Fire spell chest at second fire barrier magic switch",
            "TreasureBox10" => "Secret Passage - Thunder spell chest after boss",
            "Boss_Act01_Plus" => "Secret Passage - Enraged Armor",
            // underground_start_locations
            "TreasureBox_Room01" => "Underground - Wind spell chest",
            // Handled elsewhere because name appears twice
            /*case "TreasureBox_Room03":
                descriptiveLocation = "Underground - Chest in alcove before falling rocks";
                break;
            */
            "TreasureBox_Room04" => "Underground - Chest across staircase on backtrack path",
            "TreasureBox_Room05_01" => "Underground - Arcane chest at bridge jumping puzzle",
            "TreasureBox_Room05_02" => "Underground - Ice spell chest in bottom pit",
            "04_CatAbsorbSkill" => "Underground - Cat absorption hint & gift",
            "04_SkillBookAgain" => "Underground - Cat absorption hint & gift",
            //underground_tania_locations
            "TreasureBox_Room08" => "Underground - Chest after fire",
            "Boss_Level02" => "Underground - Defeat Tania",
            // lava_ruins_start_locations
            "Room02_TreasureBox01" => "Lava Ruins - Chest on scaffolding",
            "Room02_TreasureBox02" => "Lava Ruins - Chest on left side lava ledge",
            "Room03_TreasureBox01" => "Lava Ruins - Absorption spell chest after dropping down",
            "Room03_TreasureBox02" => "Lava Ruins - Chest on right ruins ledge at shotgun enemies magic switch",
            "Room02To04_TreasureBox02" => "Lava Ruins - Wind spell chest behind destructible walls",
            "Room05To06_TreasureBox" => "Lava Ruins - Chest in spewing lava room",
            "Room06_TreasureBox" => "Lava Ruins - Fire spell chest at double staircase",
            // lava_ruins_after_fire_barrier_locations
            "Room07_TreasureBox" => "Lava Ruins - Chest in lava maze",
            "Room08_TreasureBox" => "Lava Ruins - Jumping puzzle arcane chest at moving ring gauntlet",
            "Boss_Level03_Big" => "Lava Ruins - Monica",
            "Room01_TreasureBox" => "Lava Ruins - Ice spell chest in corner on path to dark tunnel",
            // dark_tunnel_start_locations
            "TreasureBox02_Room02_03" => "Dark Tunnel - Absorption spell chest hidden behind rubble at staircase",
            "TreasureBox02_Room03_01" => "Dark Tunnel - Chest at jump from broken staircase",
            "TreasureBox02_Room03_02" => "Dark Tunnel - Wind spell chest in dark hole",
            "TreasureBox02_Room04" => "Dark Tunnel - Fire spell chest inside dark maze",
            "TreasureBox02_Room05" => "Dark Tunnel - Thunder spell chest in mimic room",
            // dark_tunnel_after_thunder_locations
            "TreasureBox02_Room06To07" => "Dark Tunnel - Chest in alcove",
            "TreasureBox02_Room07" => "Dark Tunnel - Chest on left side after floating platforms",
            "TreasureBox02_Room08" => "Dark Tunnel - Arcane spell chest on collapsed bridge",
            "TreasureBox02_Room09To10" => "Dark Tunnel - Chest after knight enemy",
            "Boss_Level04" => "Dark Tunnel - Vanessa",
            // spirit_realm_start_locations
            "TreasureBox02_R02" => "Spirit Realm - Wind spell chest inside roof hole",
            "TreasureBox02_R03" => "Spirit Realm - Chest hidden in dropdown alcove",
            "TreasureBox02_R0401" => "Spirit Realm - Ice spell chest in right side alcove",
            // spirit_realm_after_arcane_barrier_locations
            "TreasureBox02_R0402" => "Spirit Realm - Thunder spell chest behind breakable wall",
            "TreasureBox02_R06" => "Spirit Realm - Fire spell chest at second Seal phase",
            "TreasureBox02_R07" => "Spirit Realm - Absorption spell chest behind crystals after elevator",
            // spirit_realm_after_teleport_locations
            "TreasureBox02_R08" => "Spirit Realm - Chest before boss behind breakable wall",
            "Boss_Level05" => "Spirit Realm - Vanessa V2",
            // abyss_locations
            "TreasureBox_Act02Room04" => "Abyss - Arcane spell chest on pillar",
            "TreasureBox_Act02Room05" => "Abyss - Chest in trap gate",
            // abyss_trials_locations
            "Act04Room05To06_TreasureBox" => "Abyss - Ice spell chest lava ruins trial",
            "Act05_TreasureBox02_Room09To10" => "Abyss - Thunder spell chest dark tunnel trial",
            "Act03TreasureBox_Room05_02" => "Abyss - Fire Spell chest underground trial",
            "Boss_Level06" => "Abyss - Nonota",
            _ => ""
        };

        return descriptiveLocation;
    }

    public static class Barriers
    {
        public static readonly List<BarrierMapping> BarrierMappings = new()
        {
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Shrine - First magic switch",
                ItemName = "Shrine First Magic Barrier",
                // Triggers when switch device destroyed
                TriggerPath = "/SEM/AreaEvent/Room03/Other/Room03_LoadScript",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room03/Other/MagicWall_Room03" },
                },
            },
            // Room04_02 should probably be disabled to prevent soft lock on random barriers,
            // although players can always return to shrine
            // Also /SEM/AreaEvent/Room04/Other/MagicWall_Room04_L is not disabled to prevent players
            // from breaking the switch without magic
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Shrine - Second magic switch",
                ItemName = "Shrine Second Magic Barrier",
                // Triggers when switch device behind barrier destroyed
                TriggerPath = "/SEM/AreaEvent/Room04/Other/LoadScript_Room04_02",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room04/Other/MagicWall_Room04" },
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room04/Other/MagicWall_Room04_02" },
                },
            },
            // Cutscene trigger for getting near room entrance, which forces player into room and creates barriers
            // SEM/AreaEvent/Room05/Other/Room05_LoadScript
            // May want to disable 04MagicWall01 to prevent softlock
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Shrine - Meet Cat barrier",
                ItemName = "Shrine Meet Cat Magic Barrier",
                // Triggers when enemies defeated and player gets near cat
                TriggerPath = "/SEM/AreaEvent/Room05/Other/Room05_LoadScript02",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room05/Other/04MagicWall01" },
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room05/Other/05MagicWall02" },
                },
            },
            // Should manually open magic wall to previous room if not unlocked as well
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                Type = BarrierType.MetalGate,
                LocationName = "Shrine - Underground shortcut gate switch",
                ItemName = "Shrine Underground Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room06_Save/Special/SceneSwitch01_Room06",
                Actions = new()
                {
                    new OpenDoorAction { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room06/Other/DoorBars01" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room07/Other/Tarp_Wall_Room07" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                Type = BarrierType.MetalGate,
                LocationName = "Secret Passage - Boss shortcut gate switch",
                ItemName = "Shrine Secret Boss Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room06_Save/Special/SceneSwitch02_Room06",
                Actions = new()
                {
                    new OpenDoorAction { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room06/Other/DoorBars02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Shrine - Secret passage magic switch",
                ItemName = "Secret Passage Entrance Magic Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room06To07/Other/LoadScript",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room06To07/Other/MagicWall_Room06To07" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                Type = BarrierType.MetalGate,
                LocationName = "Secret Passage - Secret area shortcut gate switch",
                ItemName = "Shrine Secret Area Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room06ToRoom07/Special/SceneSwitchRoom07To08",
                Actions = new()
                {
                    new OpenDoorAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room06To07/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Secret Passage - Defeat Enraged Armor barrier",
                ItemName = "Defeat Enraged Armor Barrier",
                // Triggers when boss is defeated
                TriggerPath = "/SEM/AreaEvent/Room10/Other/OpenScriptEvent_Room10",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room10/Other/04.MagicWall" },
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room10/Other/05.MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Secret Passage - First fire barrier magic switch",
                ItemName = "Secret Passage First Fire Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room07/Other/LoadScript_Room06To07",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room07/Other/MagicWall_Room07" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                LocationName = "Secret Passage - Second fire barrier magic switch",
                ItemName = "Secret Passage Second Fire Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room09/Other/LoadScript_Room06To07",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room09/Other/MagicWall_Room09" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Shrine,
                Type = BarrierType.MetalGate,
                LocationName = "Secret Passage - Dark Tunnel shortcut gate switch",
                ItemName = "Secret Passage Dark Tunnel Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room08_Save/Special/SceneSwitchRoom08",
                Actions = new()
                {
                    new OpenDoorAction
                        { StageId = StageId.Shrine, Path = "/SEM/AreaEvent/Room08/Other/DoorBars01_Room08" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                LocationName = "Underground - Magic barrier switches at maid enemy",
                ItemName = "Underground Magic Barrier At Maid Enemy",
                // Triggers when 3 fire switches are destroyed
                TriggerPath = "/SEM/AreaEvent/Room06/Other/Room06_LoadScript",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room06/Other/MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                Type = BarrierType.MetalGate,
                LocationName = "Underground - Lava ruins shortcut gate switch",
                ItemName = "Underground Lava Ruins Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room07_Save/Special/SceneSwitch",
                Actions = new()
                {
                    new OpenDoorAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room07/Other/DoorBars_Room07_01" },
                    new OpenDoorAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room07/Other/DoorBars_Room07_02" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room07/Other/Tarp_Wall_Room07" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall01" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall02" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall03" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall04" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                Type = BarrierType.MetalGate,
                LocationName = "Underground - Tania shortcut switch on statue side",
                ItemName = "Underground Tania Shortcut Gate On Grand Hall Side",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room09To07/Other/SceneSwitch02",
                Actions = new()
                {
                    new OpenDoorAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room09To07/Other/DoorBars02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                LocationName = "Underground - After fire magic switch",
                ItemName = "Underground Fire Barrier Magic Barrier",
                // Triggers when switch inside fire trap is destroyed
                TriggerPath = "/SEM/AreaEvent/Room08/Other/Room08_LoadScript",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/MagicWall" },
                    new FireTrapReleaseAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/FireTrap" },
                    new FireTrapReleaseAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/FireTrap" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                Type = BarrierType.MetalGate,
                LocationName = "Underground - Tania shortcut switch on Tania side",
                ItemName = "Underground Tania Shortcut Gate On Tania Side",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room09To07/Other/SceneSwitch01",
                Actions = new()
                {
                    new OpenDoorAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room09To07/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                LocationName = "Underground - Defeat enemies barrier",
                ItemName = "Underground Enemy Magic Barrier",
                // Triggers when lever is flipped
                TriggerPath = "/SEM/AreaEvent/Room09/Other/MagicWall",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room09/Other/MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Underground,
                LocationName = "Underground - Tania boss arena barrier",
                ItemName = "Underground Tania Arena Barrier",
                // Triggers when Tania is defeated
                TriggerPath = "/SEM/AreaEvent/RoomBoss/Other/LoadScriptDialogue_RoomBossEnd",
                Actions = new()
                {
                    new MagicWallReleaseAction
                    {
                        StageId = StageId.Underground,
                        Path = "/SEM/AreaEvent/RoomBoss/Other/10MagicWall02",
                    },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Magic platform switch at shotgun enemies",
                ItemName = "Lava Ruins Magic Platforms",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room02/Other/LoadScript",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room02/Other/MoveFloor01" },
                    new MoveFloorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room02/Other/MoveFloor02" },
                    new MoveFloorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room02/Other/MoveFloor03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                Type = BarrierType.MetalGate,
                LocationName = "Lava Ruins - Fake floor shortcut gate switch",
                ItemName = "Lava Ruins Fake Floor Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room01_Save/Special/SceneSwitchR",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/DoorBars01_02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Defeat scissor enemy barrier",
                ItemName = "Lava Ruins Scissor Enemy Barrier",
                // Triggers when magic wall release event triggers after defeating scissor enemy
                TriggerPath = "/SEM/AreaEvent/Room04/Other/MagicWall01",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room04/Other/MagicWall01" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room04/Other/MagicWall02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Lift magic switch at scissor enemy",
                ItemName = "Lava Ruins Scissor Enemy Lift",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room04/Other/LoadScript",
                Actions = new()
                {
                    new ElevatorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room04/Other/Elevator_Room04" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Fire magic switch",
                ItemName = "Lava Ruins Fire Magic Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room06/Other/LoadScript",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room06/Other/MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Activate moving ring",
                ItemName = "Lava Ruins Moving Ring",
                // Triggers when player gets near the activation prompt for the moving ring
                TriggerPath = "/SEM/AreaEvent/Room08/Other/OpenScriptRoom08On",
                Actions = new()
                {
                    new ElevatorAction()
                    {
                        StageId = StageId.LavaRuins,
                        Path = "/SEM/AreaEvent/Room08/Other/Elevator_Room08",
                        // We don't want to immediately start the movement on item received
                        DoNotExecuteOnItem = true,
                    },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                Type = BarrierType.MetalGate,
                LocationName = "Lava Ruins - Monica shortcut switch",
                ItemName = "Lava Ruins Monica Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room08ToBack/Special/SceneSwitch01",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room08ToBack/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                LocationName = "Lava Ruins - Monica boss arena barrier",
                ItemName = "Lava Ruins Monica Arena Barrier",
                // Triggers when Monica is defeated
                TriggerPath = "/SEM/AreaEvent/RoomBoss02/Other/LoadScriptBoss03",
                Actions = new()
                {
                    new MagicWallReleaseAction
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/RoomBoss/Other/MagicWall03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.LavaRuins,
                Type = BarrierType.MetalGate,
                LocationName = "Lava Ruins - Monica warp gate switch",
                ItemName = "Lava Ruins Monica Warp Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room01_Save/Special/SceneSwitchL",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/DoorBars01" },
                    new OpenDoorAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/DoorBars01 04" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - First magic barrier switch",
                ItemName = "Dark Tunnel First Magic Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room02/Other/LoadScript_Room02",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room02/Other/MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                Type = BarrierType.MetalGate,
                LocationName = "Dark Tunnel - First gate switch",
                ItemName = "Dark Tunnel First Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room02/Special/SceneSwitch",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room02/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - Light switch after getting the hat",
                ItemName = "Dark Tunnel Light Switch Barrier",
                // Triggers when crystal ball is filled with light
                TriggerPath = "/SEM/AreaEvent/Room01To02/Other/LoadScript_CrystalBallCompleteSlowMotion ",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room01To04/Other/MagicWall (1)" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - Thunder barrier magic switches",
                ItemName = "Dark Tunnel Thunder Barrier",
                // Triggers when three switches in thunder barrier room are destroyed
                TriggerPath = "/SEM/AreaEvent/Room06/Other/LoadScript_Room06",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (1)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (2)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (3)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (4)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (5)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (6)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (7)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (8)" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (9)" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - Floating platform switch one",
                ItemName = "Dark Tunnel Floating Platform One",
                // Triggers when middle switch is destroyed
                TriggerPath = "/Scene/Room07/Special/SwitchDevice (1)/AttackabclObject0701",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room07/Other/MoveFloor01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - Floating platform switch two",
                ItemName = "Dark Tunnel Floating Platform Two",
                // Triggers when right switch is destroyed
                TriggerPath = "/Scene/Room07/Special/SwitchDevice (2)/AttackabclObject0702",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room07/Other/MoveFloor02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.DarkTunnel,
                LocationName = "Dark Tunnel - Floating platform switch three",
                ItemName = "Dark Tunnel Floating Platform Three",
                // Triggers when left switch is destroyed
                TriggerPath = "/Scene/Room07/Special/SwitchDevice (3)/AttackabclObject0703",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room07/Other/MoveFloor03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                Type = BarrierType.MetalGate,
                LocationName = "Spirit Realm - Wind spell chest gate switch",
                ItemName = "Spirit Realm Wind Spell Chest Gate",
                // Triggers when lever is flipped.
                // Note this gate is useless but will require return to statue if not given.
                TriggerPath = "/Scene/Room02_Near/Special/SceneSwitch_R02",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room02/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                Type = BarrierType.MetalGate,
                LocationName = "Spirit Realm - Ice spell chest gate switch in right side alcove",
                ItemName = "Spirit Realm Ice Spell Chest Gate",
                // Triggers when lever is flipped.
                // Note this gate is useless but will require return to statue if not given.
                TriggerPath = "/Scene/Room04/Spical/SceneSwitch",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/L05Room04HiddenDoor" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Arcane barrier magic switches",
                ItemName = "Spirit Realm Arcane Barrier",
                // Triggers when the three switches are destroyed in a row
                TriggerPath = "/SEM/AreaEvent/Room04/Other/OpenScriptRoom04",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/00_MoveFloor01" },
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/01_MoveFloor02" },
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/02_MoveFloor03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Platform shortcut switch",
                ItemName = "Spirit Realm Platform Shortcut",
                // Triggers when the switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room04/Other/OpenScriptRoom04_02",
                Actions = new()
                {
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/00_MoveFloor01 (1)" },
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/01_MoveFloor02 (1)" },
                    new MoveFloorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room04/Other/02_MoveFloor03 (1)" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - First Seal magic barrier",
                ItemName = "Spirit Realm First Seal Magic Barrier",
                // Triggers when Seal (stage 1) is defeated
                TriggerPath = "/SEM/AreaEvent/Room05/Other/LoadScriptRoom05_02",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room05/Other/29_MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Second Seal magic barrier",
                ItemName = "Spirit Realm Second Seal Magic Barrier",
                // Triggers when Seal (stage 2) is defeated
                TriggerPath = "/SEM/AreaEvent/Room06/Other/03_MagicWall01",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room06/Other/03_MagicWall01" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room06/Other/04_MagicWall02" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room06/Other/05_MagicWall03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                Type = BarrierType.MetalGate,
                LocationName = "Spirit Realm - Statue shortcut gate switch",
                ItemName = "Spirit Realm Statue Shortcut Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room03To04_Save/Special/SceneSwitch_R03To04",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room03To04/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Elevator magic switch",
                ItemName = "Spirit Realm Elevator",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room07/Other/LoadScriptR0701",
                Actions = new()
                {
                    // This lift is a special class OpenDoor_Act06Room07MoveFloor that extends OpenDoor
                    new OpenDoorAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/00_MoveFloor" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Fire control magic switch",
                ItemName = "Spirit Realm Fire Deactivation",
                // Triggers when fire switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room07/Other/LoadScriptR0702",
                Actions = new()
                {
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap01" },
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap02" },
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap03" },
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap04" },
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap05" },
                    new FireTrapReleaseAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap06" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Magic switch barrier switch",
                ItemName = "Spirit Realm Magic Switch Barrier",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Room07/Special/SceneSwitch",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                    {
                        StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/MagicWall01",
                        DoNotExecuteOnItem = true,
                    },
                    new MagicWallReleaseAction()
                    {
                        StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/MagicWall02",
                        DoNotExecuteOnItem = true,
                    },
                    new MagicWallReleaseAction()
                    {
                        StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/MagicWall03",
                        DoNotExecuteOnItem = true,
                    },
                    new MagicWallReleaseAction()
                    {
                        StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/MagicWall04",
                        DoNotExecuteOnItem = true,
                    },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.SpiritRealm,
                LocationName = "Spirit Realm - Teleporter magic switch",
                ItemName = "Spirit Realm Teleporter",
                // Triggers when top switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Room07/Other/LoadScriptR0704",
                Actions = new()
                {
                    new TeleportEnableAction()
                        { StageId = StageId.SpiritRealm, Path = "/Scene/Special/PE_Teleport_Room07" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                Type = BarrierType.MetalGate,
                LocationName = "Abyss - First gate switch",
                ItemName = "Abyss First Gate",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Act02Room04/Special/SceneSwitch_RoomAct02",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/DoorBars01" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                Type = BarrierType.MetalGate,
                LocationName = "Abyss - Trap gate trigger",
                ItemName = "Abyss Trap Gates",
                // Triggers when player leaves the area
                TriggerPath = "/SEM/AreaEvent/Act02/Other/FightDoorEnemy",
                Actions = new()
                {
                    new OpenDoorAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/DoorBars02" },
                    new OpenDoorAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/DoorBars03" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Giant maid barrier",
                ItemName = "Abyss After Giant Maid Barrier",
                // Triggers when lever is flipped
                TriggerPath = "/SEM/AreaEvent/Act02/Other/MagicWall0201",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/MagicWall0201" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/MagicWall0202" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act02/Other/MagicWall0203" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Underground trial activate scissor enemies magic switch",
                ItemName = "Abyss Underground Trial Exit Barrier",
                // Triggers when switch in pit is destroyed, unlocking the exit barrier but activating stone enemies
                TriggerPath = "/SEM/AreaEvent/Act03/Other/LoadScript_RoomAct03",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act03/Other/MagicWall0101" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act03/Other/MagicWall0102" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Underground trial scissor enemy barrier",
                ItemName = "Abyss Underground Trial After Scissor Enemy Barrier",
                // Triggers when four stone maid enemies are killed in pit after previous switch
                // Useless item because player can just return to statue
                TriggerPath = "/SEM/AreaEvent/Act03/Other/04_MagicWall0205",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act03/Other/04_MagicWall0205" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Underground trial magic switch",
                ItemName = "Abyss Underground Trial Magic Switch",
                // Triggers when switch is destroyed
                TriggerPath = "/Scene/RoomCentral/Special/SwitchDevice_Strengthen (1)/AttackabclObject02_Act03",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0201" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0202" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0203" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Dark tunnel trial maid enemy barrier",
                ItemName = "Abyss Dark Tunnel Trial Maid Enemy Barrier",
                // Triggers when final maid enemy is killed
                TriggerPath = "/SEM/AreaEvent/Act05/Other/MagicWall01",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act05/Other/MagicWall01" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act05/Other/MagicWall02" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Dark Tunnel trial magic switch",
                ItemName = "Abyss Dark Tunnel Trial Magic Switch",
                // Triggers when switch is destroyed
                TriggerPath = "/Scene/RoomCentral/Special/SwitchDevice_Strengthen (2)/AttackabclObject03_Act05",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0301" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0302" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0303" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Lava Ruins trial lower lava switch",
                ItemName = "Abyss Lava Ruins Trial Lower Lava",
                // Triggers when lever is flipped
                TriggerPath = "/Scene/Act04Room05ToRoom06/Special/SceneSwitch",
                Actions = new()
                {
                    new MoveObjectAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act04/Other/00_MoveLava" },
                    new MoveObjectAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act04/Other/01_MoveLavaCollision" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Lava Ruins trial defeat maids enemy barrier",
                ItemName = "Abyss Lava Ruins Trial Maid Enemy Barrier",
                // Triggers when switch is destroyed
                TriggerPath = "/SEM/AreaEvent/Act04/Other/MagicWall",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/Act04/Other/MagicWall" },
                },
            },
            new BarrierMapping()
            {
                StageId = StageId.Abyss,
                LocationName = "Abyss - Lava Ruins trial magic switch",
                ItemName = "Abyss Lava Ruins Trial Magic Switch",
                // Triggers when switch is destroyed
                TriggerPath = "/Scene/RoomCentral/Special/SwitchDevice_Strengthen/AttackabclObject01_Act04",
                Actions = new()
                {
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0101" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0102" },
                    new MagicWallReleaseAction()
                        { StageId = StageId.Abyss, Path = "/SEM/AreaEvent/RoomCentral/Other/MagicWall0103" },
                },
            },
        };

        // Barrier actions that are executed on scene load, used to disable things like trap walls, etc.
        // If an item name is specified, the action is only run if the player DOES NOT have the item.
        // This is because some barriers only activate when the player enters a room or triggers an event, meaning
        // it won't prevent players moving backwards during non-vanilla progression (randomized gates, etc.)
        private static readonly List<StageLoadAction> OnStageLoadActions = new()
        {
            // Below are stage load actions that always run ===============================================

            // Disable trap walls in Lava Ruins that push players into the lobby statue area,
            // normally released once the Monica Warp Gate switch is flipped.
            // Care should be taken here: Underground Tania Arena -> Lava Ruins After Monica Gates should not
            // be in logic without unlocking the gate item first.
            new StageLoadAction()
            {
                StageId = StageId.LavaRuins,
                Actions = new()
                {
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall01" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall02" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall03" },
                    new TrapWallReleaseAction()
                        { StageId = StageId.LavaRuins, Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall04" },
                }
            },
            // Disable Dark Tunnel to Lava Ruins door trap wall that prevents the door from being used
            // unless the Lava Ruins stage clear flag is toggled.
            new StageLoadAction()
            {
                StageId = StageId.DarkTunnel,
                Actions = new()
                {
                    new TrapWallReleaseAction() { Path = "/SEM/AreaEvent/Room01/Other/Tarp_Wall" },
                }
            },
            // Remove special DelayMoveFloor objects on the Dark Tunnel three floating platforms
            // as they cause an infinite loop of trying to open the event when the MoveFloor event is blocked
            new StageLoadAction()
            {
                StageId = StageId.DarkTunnel,
                Actions = new()
                {
                    new SpecialAction(() =>
                    {
                        var delay1 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room07/Other/DelayMoveFloor01");
                        var delay2 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room07/Other/DelayMoveFloor02");
                        var delay3 = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room07/Other/DelayMoveFloor03");
                        UnityEngine.Object.Destroy(delay1);
                        UnityEngine.Object.Destroy(delay2);
                        UnityEngine.Object.Destroy(delay3);
                    })
                }
            },
            // Set delay for Abyss trap gate events to a large as possible to prevent the parent MultipleEventOpen event
            // from attempting to open the gates every frame when the player doesn't have the gate item.
            new StageLoadAction()
            {
                StageId = StageId.Abyss,
                Actions = new()
                {
                    new SpecialAction(() =>
                    {
                        var trapDoorsEvent = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Act02/Other/OpenDoor");
                        var multipleEventOpen = trapDoorsEvent?.GetComponent<MultipleEventOpen>();
                        if (multipleEventOpen is null || multipleEventOpen.DeltaTime.Length != 3) return;
                        multipleEventOpen.DeltaTime[0] = float.MaxValue;
                        multipleEventOpen.DeltaTime[1] = float.MaxValue;
                    })
                }
            },
            // Enables the cat prompt to get the absorption book item if it's in the scene
            // This overrides the expected state the game has for this prompt to keep the check always accessible
            new StageLoadAction()
            {
                StageId = StageId.Underground,
                Actions = new()
                {
                    new SpecialAction(() =>
                    {
                        var catBook = UnityUtils.FindObjectByPath("/SEM/AreaEvent/Room02/Other/CatBook");
                        if (catBook is null) return;
                        catBook.gameObject.SetActive(true);
                    })
                }
            },

            // Below are stage load actions with an associated item =====================================

            new StageLoadAction()
            {
                // Barrier normally only enables when entering from front, meaning won't block movement
                // from Enranged Armor arena room.
                StageId = StageId.Shrine,
                ItemName = "Secret Passage Second Fire Barrier",
                Actions = new()
                {
                    new MagicWallStartAction() { Path = "/SEM/AreaEvent/Room09/Other/MagicWall_Room09" }
                }
            },
            new StageLoadAction()
            {
                StageId = StageId.Underground,
                ItemName = "Underground Magic Barrier At Maid Enemy",
                Actions = new()
                {
                    new MagicWallStartAction() { Path = "/SEM/AreaEvent/Room06/Other/MagicWall" }
                }
            },
            new StageLoadAction()
            {
                StageId = StageId.Underground,
                ItemName = "Underground Fire Barrier Magic Barrier",
                Actions = new()
                {
                    new MagicWallStartAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/MagicWall" },
                    new FireTrapStartAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/FireTrap" },
                    new FireTrapStartAction()
                        { StageId = StageId.Underground, Path = "/SEM/AreaEvent/Room08/Other/FireTrap" },
                },
            },
            new StageLoadAction()
            {
                StageId = StageId.Underground,
                ItemName = "Underground Enemy Magic Barrier",
                Actions = new()
                {
                    new MagicWallStartAction() { Path = "/SEM/AreaEvent/Room09/Other/MagicWall" }
                }
            },

            new StageLoadAction()
            {
                // Barrier after Tania, blocking reverse movement (Lava Ruins -> Underground)
                // Also needed as Tania defeat flag will not be reset
                StageId = StageId.Underground,
                ItemName = "Underground Tania Arena Barrier",
                Actions = new()
                {
                    new MagicWallStartAction() { Path = "/SEM/AreaEvent/RoomBoss/Other/10MagicWall02" }
                }
            },
            new StageLoadAction()
            {
                // Barrier after Monica, only blocking moving to the teleporter.
                StageId = StageId.LavaRuins,
                ItemName = "Lava Ruins Monica Arena Barrier",
                Actions = new()
                {
                    new MagicWallStartAction() { Path = "/SEM/AreaEvent/RoomBoss/Other/MagicWall03" },
                    // Move arena barrier to next doorway, since in rare cases, if the player returns to statue
                    // after the fight but before picking up the 3 items, the items will move behind the barrier
                    // which could potentially cause an unbeatable state.
                    new SpecialAction(() =>
                    {
                        if (!(Singletons.GameSave?.flags.stage03Clear ?? false)) return;
                        var wallCollision = UnityUtils.FindObjectByPath("/SceneManager/EnemyEffect/MagicWall(Clone)");
                        if (wallCollision is null) return;
                        wallCollision.transform.position = new Vector3(-132.6f, -20f, 320f);
                        wallCollision.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    })
                }
            },
            new StageLoadAction()
            {
                // This barrier only activates when entering the room from the front.
                StageId = StageId.DarkTunnel,
                ItemName = "Dark Tunnel Light Switch Barrier",
                Actions = new()
                {
                    new MagicWallStartAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room01To04/Other/MagicWall (1)" },
                }
            },
            new StageLoadAction()
            {
                // Disable thunder wall if that setting is enabled
                StageId = StageId.DarkTunnel,
                Actions = new()
                {
                    new MagicWallLightningReleaseOnOptionAction()
                    {
                        StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room05/Other/MagicWallLightning_Room05_01"
                    },
                    new MagicWallLightningReleaseOnOptionAction()
                    {
                        StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room05/Other/MagicWallLightning_Room05_02"
                    },
                    new MagicWallLightningReleaseOnOptionAction()
                    {
                        StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWallLightning_Room06"
                    },
                }
            },
            new StageLoadAction()
            {
                // These barriers only activate when entering the room from the front.
                StageId = StageId.DarkTunnel,
                ItemName = "Dark Tunnel Thunder Barrier",
                Actions = new()
                {
                    new MagicWallStartAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (4)" },
                    new MagicWallStartAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (5)" },
                    new MagicWallStartAction()
                        { StageId = StageId.DarkTunnel, Path = "/SEM/AreaEvent/Room06/Other/MagicWall (6)" },
                }
            },
            new StageLoadAction()
            {
                // Seal arena barriers only activate on boss trigger, which don't prevent backwards movement
                StageId = StageId.SpiritRealm,
                ItemName = "Spirit Realm First Seal Magic Barrier",
                Actions = new()
                {
                    new MagicWallStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room05/Other/29_MagicWall" },
                }
            },
            new StageLoadAction()
            {
                StageId = StageId.SpiritRealm,
                ItemName = "Spirit Realm Second Seal Magic Barrier",
                Actions = new()
                {
                    new MagicWallStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room06/Other/04_MagicWall02" },
                    new MagicWallStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room06/Other/05_MagicWall03" },
                }
            },
            new StageLoadAction()
            {
                StageId = StageId.SpiritRealm,
                ItemName = "Spirit Realm Fire Deactivation",
                Actions = new()
                {
                    // Normally only starts after first switch is destroyed and elevator activates
                    new FireTrapStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap01" },
                    new FireTrapStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap02" },
                    new FireTrapStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap03" },
                    new FireTrapStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap04" },
                    new FireTrapStartAction()
                        { StageId = StageId.SpiritRealm, Path = "/SEM/AreaEvent/Room07/Other/FireTrap05" },
                }
            }
        };

        public static readonly ILookup<string, BarrierMapping> ByTriggerPath =
            BarrierMappings.ToLookup(b => b.TriggerPath);

        public static readonly ILookup<string, BarrierMapping> ByItemName =
            BarrierMappings.ToLookup(b => b.ItemName);

        public static readonly Dictionary<int, ILookup<string, BarrierMapping>> ByActionPath = BarrierMappings
            .GroupBy(b => b.StageId)
            .ToDictionary(
                group => (int)group.Key,
                group => group.SelectMany(b => b.Actions.Select(action => new { action.Path, barrier = b }))
                    .ToLookup(x => x.Path, x => x.barrier)
            );

        public static readonly ILookup<int, StageLoadAction> OnStageLoadActionsByStageId =
            OnStageLoadActions.ToLookup(b => (int)b.StageId);
    }

    public static long GetLocationIdByName(string name)
    {
        return Locations.Keys.ToImmutableSortedSet().IndexOf(name) + 1;
    }

    public static long GetItemIdByName(string name)
    {
        return Items.Keys.ToImmutableSortedSet().IndexOf(name) + 1;
    }

    public static class CutscenesToSkip
    {
        public static readonly List<CutsceneTrigger> Cutscenes = new()
        {
            // Initial cutscene when entering shrine. Without the cutscene the player will spawn facing the wrong
            // way (which isn't a big issue). This removal is required for randomized start as this triggers even
            // if player enters Shrine through a different entrance.
            new CutsceneTrigger()
            {
                StageId = StageId.Shrine,
                Trigger = "/SEM/AreaEvent/Room01/Other/Room01_OpenDoorScript",
                ShouldSkip = () =>
                    (ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false) ||
                    ((ArchipelagoClient.ServerData.Settings?.StartLevel !=
                      ArchipelagoSettings.StartLevelSetting.OkunShrine))
            },
            // Cutscene when you enter Underground after first boss. This triggers even if the player enters through
            // the shortcut door, which teleports the player to the start and breaks logic
            new CutsceneTrigger()
            {
                StageId = StageId.Underground,
                Trigger = "/SEM/AreaEvent/Room01/Other/LoadScript_Room01",
            },
            // Cutscene where the cat explains using ice magic to pass the fire barriers
            new CutsceneTrigger()
            {
                StageId = StageId.Underground,
                Trigger = "/SEM/AreaEvent/Room07To08/Other/LoadScript",
                ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false
            },
            // Skips cutscene before Tania when walking through the hall with broken dolls
            new CutsceneTrigger()
            {
                StageId = StageId.Underground,
                Trigger = "/SEM/AreaEvent/Room09/Other/LoadScript",
                ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false
            },
            // Removes cutscene from entering Lava Ruins from Underground, which forces player into statue pit
            // This has to be paired with moving the rebirth point of the Underground door (or logic will need
            // to be modified)
            new CutsceneTrigger()
            {
                StageId = StageId.LavaRuins,
                Trigger = "/SEM/AreaEvent/Room01/Other/LoadScript",
            },
            // TODO: figure out what this is...
            new CutsceneTrigger()
            {
                StageId = StageId.LavaRuins,
                Trigger = "/SEM/AreaEvent/Room07To08/Other/LoadScript",
                ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false
            },
            // Skips cutscene near Fire pickup
            new CutsceneTrigger()
            {
                StageId = StageId.LavaRuins,
                Trigger = "/SEM/AreaEvent/Room05/Other/LoadScript_Room05",
                ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false
            },
            // Skips cutscene at entrance of Dark Tunnel where player loses hat
            // As this would normally enable hat lost flag, the hat retrieval cutscene also won't trigger
            // TODO: Currently disabled since it might interfere with light orb logic
            // new CutsceneTrigger()
            // {
            //     StageId = StageId.DarkTunnel,
            //     Trigger = "/SEM/AreaEvent/Room01/Other/LoadScript01",
            //     ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableUnimportantCutscenes ?? false
            // },
            new CutsceneTrigger()
            {
                StageId = StageId.DarkTunnel,
                Trigger = "/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08",
                ShouldSkip = () => ArchipelagoClient.ServerData.Settings?.DisableDarkTunnelBridgeCollapse ?? false
            },
            new CutsceneTrigger()
            {
                StageId = StageId.DarkTunnel,
                Trigger = "/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_02",
            },
            new CutsceneTrigger()
            {
                StageId = StageId.DarkTunnel,
                Trigger = "/SEM/AreaEvent/Room08_02/Other/LoadScriptRoom08_03",
            },
            // Cutscene that plays when you initially load into stage
            // These are disabled unconditionally because they could allow incorrect warping if a player loads a save
            // within Spirit Realm or Abyss but hasn't played the cutscene yet, which warps them to the start
            new CutsceneTrigger()
            {
                StageId = StageId.SpiritRealm,
                Trigger = "/SEM/AreaEvent/Room01/Other/LoadScriptRoom01",
            },
            new CutsceneTrigger()
            {
                StageId = StageId.Abyss,
                Trigger = "/SEM/AreaEvent/RoomStart/Other/LoadScript_RoomStart",
            },
            // Cutscene when Nobeta passes the final teleporter into final boss room
            new CutsceneTrigger()
            {
                StageId = StageId.Abyss,
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/DelayLoadScript",
            },
        };

        public static readonly ILookup<int, CutsceneTrigger> ByStageId =
            Cutscenes.ToLookup(c => (int)c.StageId);
    }

    public static class BossTriggers
    {
        public static readonly List<BossTrigger> Triggers = new()
        {
            new BossTrigger()
            {
                StageId = StageId.Shrine,
                SoulName = "Specter Armor Soul",
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/LoadScript"
            },
            new BossTrigger()
            {
                StageId = StageId.Shrine,
                SoulName = "Enraged Armor Soul",
                Trigger = "/SEM/AreaEvent/Room10/Other/LoadScriptRoom10"
            },
            new BossTrigger()
            {
                StageId = StageId.Underground,
                SoulName = "Tania Soul",
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/LoadScript"
            },
            new BossTrigger()
            {
                StageId = StageId.LavaRuins,
                SoulName = "Monica Soul",
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/LoadScript"
            },
            new BossTrigger()
            {
                StageId = StageId.DarkTunnel,
                SoulName = "Vanessa Soul",
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/LoadScript_Boss"
            },
            new BossTrigger()
            {
                StageId = StageId.SpiritRealm,
                SoulName = "Vanessa V2 Soul",
                Trigger = "/SEM/AreaEvent/RoomBoss/Other/LoadScriptRoomBoss"
            },
        };

        public static readonly Dictionary<string, BossTrigger> BySoulName =
            Triggers.ToDictionary(t => t.SoulName);
    }
}