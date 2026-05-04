using Il2Cpp;
using LittleWitchNobetaAP.Utils;

namespace LittleWitchNobetaAP.Archipelago;

public enum StageId
{
    Shrine = 2,
    Underground = 3,
    LavaRuins = 4,
    DarkTunnel = 5,
    SpiritRealm = 6,
    Abyss = 7,
}

public abstract class BarrierAction
{
    public string Path { get; init; } = "";

    public StageId StageId { get; init; }

    // If true, this action will not be run after receiving the item and will only be used
    // to determine whether to allow an event at the path.
    // This is for special cases such as activating elevators, boss barriers, etc.
    public bool DoNotExecuteOnItem { get; init; } = false;
    public abstract void Execute();
}

public class MagicWallStartAction : BarrierAction
{
    public override void Execute()
    {
        var magicWall = UnityUtils.FindObjectByPath(Path)?.GetComponent<MagicWall>();
        magicWall?.OpenEvent();
    }
}

public class MagicWallReleaseAction : BarrierAction
{
    public override void Execute()
    {
        var magicWall = UnityUtils.FindObjectByPath(Path)?.GetComponent<MagicWall>();
        magicWall?.ReleaseEvent();
    }
}

public class MoveFloorAction : BarrierAction
{
    public override void Execute()
    {
        var moveFloor = UnityUtils.FindObjectByPath(Path)?.GetComponent<MoveFloor>();
        moveFloor?.OpenEvent();
    }
}

public class MoveObjectAction : BarrierAction
{
    public override void Execute()
    {
        var moveObject = UnityUtils.FindObjectByPath(Path)?.GetComponent<MoveObject>();
        moveObject?.OpenEvent();
    }
}

public class OpenDoorAction : BarrierAction
{
    public override void Execute()
    {
        var openDoor = UnityUtils.FindObjectByPath(Path)?.GetComponent<OpenDoor>();
        openDoor?.OpenEvent();
    }
}

public class ElevatorAction : BarrierAction
{
    public override void Execute()
    {
        var elevator = UnityUtils.FindObjectByPath(Path)?.GetComponent<Elevator>();
        elevator?.ResetEvent();
    }
}

public class TrapWallReleaseAction : BarrierAction
{
    public override void Execute()
    {
        var trapWall = UnityUtils.FindObjectByPath(Path)?.GetComponent<Trap_Wall_Level01>();
        trapWall?.ReleaseEvent();
    }
}

public class FireTrapStartAction : BarrierAction
{
    public override void Execute()
    {
        var fireTrap = UnityUtils.FindObjectByPath(Path)?.GetComponent<Trap_BoxCollider>();
        fireTrap?.OpenEvent();
    }
}

public class FireTrapReleaseAction : BarrierAction
{
    public override void Execute()
    {
        var fireTrap = UnityUtils.FindObjectByPath(Path)?.GetComponent<Trap_BoxCollider>();
        fireTrap?.ReleaseEvent();
    }
}

public class TeleportEnableAction : BarrierAction
{
    public override void Execute()
    {
        var teleport = UnityUtils.FindObjectByPath(Path)?.GetComponent<Teleport>();
        teleport?.SetEnable(true);
    }
}

public class MagicWallLightningReleaseOnOptionAction : BarrierAction
{
    public override void Execute()
    {
        if (!(ArchipelagoClient.ServerData.Settings?.DisableDarkTunnelThunderWall ?? false)) return;
        var magicWallLightning = UnityUtils.FindObjectByPath(Path)?.GetComponent<MagicWallLightning>();
        magicWallLightning?.ReleaseEvent();
    }
}

// Special class for creating special one-off barrier actions
public class SpecialAction : BarrierAction
{
    private Action OnExecute { get; }

    public SpecialAction(Action onExecute)
    {
        OnExecute = onExecute;
    }

    public override void Execute()
    {
        OnExecute();
    }
}
