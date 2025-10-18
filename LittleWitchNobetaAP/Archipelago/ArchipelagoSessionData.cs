using Il2Cpp;
using Newtonsoft.Json;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSessionData
{
    private int _index;

    /// <summary>
    ///     seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    ///     load is valid to the room it's connecting to.
    /// </summary>
    private string? _seed;

    private Dictionary<string, object>? _slotData;

    public List<long> CheckedLocations;
    public string Hostname;
    public string Password;
    public string Port;
    public string SlotName;
    public Dictionary<long, List<SceneEvent>> StoredEvents;

    public ArchipelagoSessionData()
    {
        Hostname = "archipelago.gg";
        SlotName = "Player1";
        Port = "38281";
        Password = string.Empty;
        CheckedLocations = new List<long>();
        StoredEvents = new Dictionary<long, List<SceneEvent>>();
        KilledBosses = new HashSet<string>();
        OpenedTrials = new HashSet<string>();
    }

    public ArchipelagoSessionData(string uri, string port, string slotName, string password)
    {
        Hostname = uri;
        Port = port;
        SlotName = slotName;
        Password = password;
        CheckedLocations = new List<long>();
        StoredEvents = new Dictionary<long, List<SceneEvent>>();
        KilledBosses = new HashSet<string>();
        OpenedTrials = new HashSet<string>();
    }

    public int Index
    {
        get => _index;
        set
        {
            _index = value;
            ArchipelagoClient.ApSaveFile?.UpdateItemCount(value);
        }
    }

    public HashSet<string> KilledBosses { get; }
    public HashSet<string> OpenedTrials { get; }

    public bool NeedSlotData => _slotData == null;

    /// <summary>
    ///     assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public void SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        _slotData = roomSlotData;
        _seed = roomSeed;

        ArchipelagoClient.ApSaveFile = new ArchipelagoSaveFile(_seed);
    }

    /// <summary>
    ///     returns the object as a json string to be written to a file which you can then load
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}