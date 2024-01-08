using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/CreateGameConfig", order = 1)]
public class GameConfig : ConfigObjectBase<GameConfig>
{
    public bool enableReporter = false;
    public bool enableStatsMonitor = false;
    public int targetFrameRate = 30;
}
