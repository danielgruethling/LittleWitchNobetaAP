using Il2Cpp;

namespace LittleWitchNobetaAP.Utils;

public class SceneUtils
{
    public static int SceneStartSavePoint(int destinationScene) => destinationScene switch
    {
        2 => -1,
        3 => -1,
        4 => 2,
        5 => 3,
        6 => -1,
        7 => -1,
        _ => throw new ArgumentOutOfRangeException(nameof(destinationScene), destinationScene, null)
    };

    public static int SceneNumberFromName(string sceneName) => sceneName switch
    {
        "Act02_01" => 2,
        "Act03_01" => 3,
        "Act04_01" => 4,
        "Act05_02" => 5,
        "Act06_03" => 6,
        "Act07" => 7,
        _ => -1
    };

    public static GameStage SceneNumberToGameStage(int sceneNumber) => sceneNumber switch
    {
        2 => GameStage.Act02_01,
        3 => GameStage.Act03_01,
        4 => GameStage.Act04_01,
        5 => GameStage.Act05_02,
        6 => GameStage.Act06_03,
        7 => GameStage.Act07,
        _ => throw new ArgumentOutOfRangeException(nameof(sceneNumber), sceneNumber, null)
    };
}