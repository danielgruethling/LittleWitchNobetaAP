using MelonLoader;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSaveFile
{
    public static readonly MelonPreferences_Category ArchipelagoSlotInfoCategory = MelonPreferences.CreateCategory("LwnApSaveFileCategory");
    private MelonPreferences_Entry<ArchipelagoSessionData> _sessionData;
    /*private MelonPreferences_Entry<string> _hostName;
    private MelonPreferences_Entry<string> _slotName;
    private MelonPreferences_Entry<string> _password;
    private MelonPreferences_Entry<string> _port;
    private MelonPreferences_Entry<List<long>> _receivedItems;
    private MelonPreferences_Entry<List<long>> _sentLocations;
    private MelonPreferences_Entry<Dictionary<long, List<SceneEvent>>> _storedEvents;*/
    
    public ArchipelagoSaveFile()
    {
        ArchipelagoSlotInfoCategory.SetFilePath("LittleWitchNobetaAP/ArchipelagoSlotInfo.cfg");
        _sessionData = ArchipelagoSlotInfoCategory.CreateEntry("SessionData", new ArchipelagoSessionData());
        /*_hostName = ArchipelagoSlotInfoCategory.CreateEntry("HostName", string.Empty);
        _slotName = ArchipelagoSlotInfoCategory.CreateEntry("SlotName", string.Empty);
        _password = ArchipelagoSlotInfoCategory.CreateEntry("Password", string.Empty);
        _port = ArchipelagoSlotInfoCategory.CreateEntry("Port", string.Empty);
        _receivedItems = ArchipelagoSlotInfoCategory.CreateEntry("ReceivedItems", new List<long>());
        _sentLocations = ArchipelagoSlotInfoCategory.CreateEntry("SentLocations", new List<long>());
        _storedEvents = ArchipelagoSlotInfoCategory.CreateEntry("SentLocations", new Dictionary<long, List<SceneEvent>>());*/
    }

    public static void Save ()
    {
        Melon<LwnApMod>.Logger.Msg("Saving archipelago state...");

        ArchipelagoSlotInfoCategory.SaveToFile();

        Melon<LwnApMod>.Logger.Msg("Archipelago state saved");
    }
}