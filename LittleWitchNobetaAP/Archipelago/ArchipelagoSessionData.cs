using Il2Cpp;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoSessionData
{
    public string Hostname;
    public string SlotName;
    public string Password;
    public string Port;
    public int Index;

    public List<long> CheckedLocations;
    public List<long> ReceivedItems;
    public Dictionary<long, List<SceneEvent>> StoredEvents;

    /// <summary>
    /// seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    /// load is valid to the room it's connecting to.
    /// </summary>
    private string seed;

    private Dictionary<string, object> slotData;

    public bool NeedSlotData => slotData == null;

    public ArchipelagoSessionData ()
    {
        Hostname = "localhost";
        SlotName = "Player1";
        Port = "38281";
        Password = string.Empty;
        CheckedLocations = new List<long>();
        ReceivedItems = new List<long>();
        StoredEvents = new Dictionary<long, List<SceneEvent>>();
    }

    public ArchipelagoSessionData (string uri, string port, string slotName, string password)
    {
        Hostname = uri;
        Port = port;
        SlotName = slotName;
        Password = password;
        CheckedLocations = new List<long>();
        ReceivedItems = new List<long>();
        StoredEvents = new Dictionary<long, List<SceneEvent>>();
    }

    /// <summary>
    /// assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public void SetupSession (Dictionary<string, object> roomSlotData, string roomSeed)
    {
        slotData = roomSlotData;
        seed = roomSeed;

        Singletons.Dispatcher.Enqueue(() =>
        {
            if (StartPatches.CopyrightText != null)
            {
                StartPatches.CopyrightText.text =
                    $"""
                     Archipelago Seed Hash: {seed}
                     © 2022 Pupuya Games / SimonCreative / Justdan  © 2016 COVER Corp.
                     """;
            }
        });
    }

    /// <summary>
    /// returns the object as a json string to be written to a file which you can then load
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
        return JsonConvert.SerializeObject(this);
    }
}