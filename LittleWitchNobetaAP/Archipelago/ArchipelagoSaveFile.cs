using LittleWitchNobetaAP.Utils;
using MelonLoader;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSaveFile
{
    private static readonly MelonPreferences_Category ArchipelagoSlotInfoCategory =
        MelonPreferences.CreateCategory("LwnApSaveFileCategory", "LwnApSaveFileCategory", false, false);

    private readonly MelonPreferences_Entry<int> _receivedItemCount =
        ArchipelagoSlotInfoCategory.CreateEntry("ReceivedItemCount", 0);

    private readonly MelonPreferences_Entry<string> _seed = ArchipelagoSlotInfoCategory.CreateEntry("Seed", "-1");
    /*private MelonPreferences_Entry<string> _hostName;
    private MelonPreferences_Entry<string> _slotName;
    private MelonPreferences_Entry<string> _password;
    private MelonPreferences_Entry<string> _port;
    private MelonPreferences_Entry<List<long>> _receivedItems;
    private MelonPreferences_Entry<List<long>> _sentLocations;
    private MelonPreferences_Entry<Dictionary<long, List<SceneEvent>>> _storedEvents;*/

    public ArchipelagoSaveFile(string seed)
    {
        var saveFilePath = $"UserData/Slot{Singletons.GameSave?.basic.dataIndex}.cfg";
        var saveFileExists = File.Exists(saveFilePath);

        ArchipelagoSlotInfoCategory.SetFilePath(saveFilePath);
        if (!saveFileExists)
        {
            _seed.Value = seed;
        }
        else
        {
            Melon<LwnApMod>.Logger.Msg($"Item count: {_receivedItemCount.Value}");
            ArchipelagoClient.ServerData.Index = _receivedItemCount.Value;
        }

        /*_hostName = ArchipelagoSlotInfoCategory.CreateEntry("HostName", string.Empty);
        _slotName = ArchipelagoSlotInfoCategory.CreateEntry("SlotName", string.Empty);
        _password = ArchipelagoSlotInfoCategory.CreateEntry("Password", string.Empty);
        _port = ArchipelagoSlotInfoCategory.CreateEntry("Port", string.Empty);
        _receivedItems = ArchipelagoSlotInfoCategory.CreateEntry("ReceivedItems", new List<long>());
        _sentLocations = ArchipelagoSlotInfoCategory.CreateEntry("SentLocations", new List<long>());
        _storedEvents = ArchipelagoSlotInfoCategory.CreateEntry("SentLocations", new Dictionary<long, List<SceneEvent>>());*/
    }

    public void UpdateItemCount(int itemCount)
    {
        _receivedItemCount.Value = itemCount;
    }

    public void Save()
    {
        if (Singletons.GameSave is null) return;

        Melon<LwnApMod>.Logger.Msg("Saving archipelago state...");
        ArchipelagoSlotInfoCategory.SaveToFile();
        Melon<LwnApMod>.Logger.Msg("Archipelago state saved");
    }
}