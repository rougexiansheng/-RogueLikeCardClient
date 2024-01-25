using System.Collections.Generic;
using UnityEngine;
using System;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using SDKProtocol;
using DG.Tweening;
using System.Linq;
using DG.Tweening.Core.Easing;
using Tayx.Graphy.Utils.NumString;
using System.Data;


[Serializable]
public class MapButtonData
{
    public Sprite EnableImage;

    public Sprite DisableImage;

    public Sprite PassImage;

}

public class UIMap : UIBase
{

    [SerializeField]
    private float fadeOutValue = 0f;
    [SerializeField]
    private float fadeOutTime = 0.7f;

    [SerializeField]
    private float zoomInOutTime = 0.4f;


    [SerializeField]
    private float zoomOutValue = 0f;
    [SerializeField]
    private float zoomInValue = 1f;



    [SerializeField]
    private float flashScaleTime = 0.7f;

    [SerializeField]
    private float flashFadeValue = 0f;
    [SerializeField]
    private float flashFadeTime = 0.7f;

    [Inject]
    AssetManager assetManager;

    [Inject]
    SDKProtocol.IProtocolBridge sdk;

    [SerializeField]
    GameObject root;

    [SerializeField]
    GameObject flashRoot;
    [Inject]
    EnvironmentManager environmentManager;

    [Inject]
    DataManager dataManager;

    [Inject]
    UIManager uIManager;
    [Inject]
    DataTableManager dataTableManager;

    [Inject]
    NetworkSaveManager saveManager;

    [SerializeField]
    public UIMapButton SButtonGameObject;
    [SerializeField]
    public UIMapButton BButtonGameObject;
    [SerializeField]
    public List<Vector3> FirstLayerButtonPosition;

    [SerializeField]
    public List<RectTransform> mapPointPosition;
    private string SMapButton = "SmallUIMapButton";

    private string BMapButton = "BigUIMapButton";

    private NetworkSaveBattleDungeonContainer dungeonData;

    private UIMapButton playerUI;

    /// <summary>
    /// Key -> dungeonId，Value->MapButtonObject
    /// </summary>
    private Dictionary<int, UIMapButton> currentButtonDic = new Dictionary<int, UIMapButton>();

    private Dictionary<int, UnityEngine.UI.Image> flashImageDic = new Dictionary<int, UnityEngine.UI.Image>();

    /// <summary>
    /// List 順序依照 Player (Small) + MapNodeEnum + Player (Big)(EnumDefine.cs)
    /// </summary>
    [SerializeField]
    public List<MapButtonData> ButtonImages;

    [SerializeField]
    public List<UnityEngine.UI.Image> flashImage;


    private DG.Tweening.Sequence flashSequence;
    public override UniTask OnOpen()
    {
        //Init();
        return base.OnOpen();
    }


    public async void Init()
    {
        dungeonData = saveManager.GetContainer<NetworkSaveBattleDungeonContainer>();
        await checkCurrentLayer();
        CreateUIObjectPool();
        createMapButtons(dungeonData.LastCache);
        createPlayer(dungeonData.FightDungeonId);
        await uIPerformanceAsync(dungeonData.FightDungeonId);
    }

    /// <summary>
    /// Determine whether the current layer is the last layer.
    /// </summary>
    /// <returns></returns>
    private async UniTask checkCurrentLayer()
    {
        var playerCurrentID = dungeonData.FightDungeonId;
        List<DungeonLevelData> LastLayerData = dungeonData.LastCache.Last();
        var currentLayer = dataTableManager.GetDungeonDataDefine(playerCurrentID).mapLayer;
        ;
        if (dataTableManager.GetDungeonDataDefine(LastLayerData[0].dungeonId).mapLayer == currentLayer)
        {

            Debug.Log(string.Format("<color=#BA0000>{0}</color>", "Level Data List Empty. Please Choose another Level"));
            uIManager.RemoveUI<UIMap>();
            await UniTask.WaitUntil(() => this == null);
            return;
        }

    }

    private void CreateUIObjectPool()
    {
        assetManager.SetDefaultObject<UIMapButton>(SButtonGameObject, SMapButton);
        assetManager.SetDefaultObject<UIMapButton>(BButtonGameObject, BMapButton);
    }

    private void createMapButtons(List<List<DungeonLevelData>> dungeonLevelDatas)
    {
        int currentLayer = dataTableManager.GetDungeonDataDefine(dungeonData.FightDungeonId).mapLayer - 1;
        int dungeonLayer = dungeonLevelDatas.Count - currentLayer;
        if (dungeonLayer > 4)
            dungeonLayer = 4;

        for (int i = 0; i < dungeonLayer; i++)
        {
            List<DungeonLevelData> oneLayer = dungeonLevelDatas[currentLayer + i];
            for (int j = 0; j < oneLayer.Count; j++)
            {
                DungeonLevelData node = oneLayer[j];
                int nodePosition = dataTableManager.GetDungeonDataDefine(node.dungeonId).UIPosition - 1;
                //Vector3 position = FirstLayerButtonPosition[nodePosition] + new Vector3(0, 300 * i, 0);
                Vector3 position = mapPointPosition[nodePosition + i * 3].localPosition;
                currentButtonDic[node.dungeonId] = createUI((int)node.nodeEnum, position);
                flashImageDic[node.dungeonId] = createFlashImage((int)node.nodeEnum, node.dungeonId, position);
                float center = 1;
                if (nodePosition == 1)
                {
                    center = 0.85f - 0.1f * i;

                }
                else
                {
                    center = 0.8f - 0.1f * i;
                }

                currentButtonDic[node.dungeonId].gameObject.transform.localScale = new Vector3(center, center, center);
                flashImageDic[node.dungeonId].gameObject.transform.localScale = new Vector3(center, center, center);


            }
        }
    }

    private UnityEngine.UI.Image createFlashImage(int nodeEnum, int id, Vector3 uiPosition)
    {
        UnityEngine.UI.Image prefab = GetImageObject(nodeEnum);
        UnityEngine.UI.Image image = Instantiate(prefab).GetComponent<UnityEngine.UI.Image>();
        image.gameObject.name = id.ToString();
        setFlashPosition(image, flashRoot.transform, uiPosition, nodeEnum);
        image.gameObject.SetActive(false);
        return image;
    }


    private void setFlashPosition(UnityEngine.UI.Image ui, Transform root, Vector3 position, int mapNodeEnum)
    {
        ui.transform.SetParent(root, false);
        ui.transform.localPosition = Vector3.zero;
        ui.transform.localRotation = Quaternion.identity;
        ui.transform.localPosition += position;
        if (IsBigUIorNot(mapNodeEnum))
            ui.transform.localPosition += new Vector3(0, 57, 0);
    }


    private void createPlayer(int playerCurrentID)
    {
        UIMapButton oldMapButton = currentButtonDic[playerCurrentID];
        int playerEnumNumber = 0;
        // 因為多了一個Player(0)，所以-1。
        if (oldMapButton.MapNodeEnum - 1 == (int)MapNodeEnum.Boss)
        {
            playerEnumNumber = 8;
        }

        playerUI = createUI(playerEnumNumber, oldMapButton.transform.localPosition, 0);
        playerUI.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        oldMapButton.gameObject.SetActive(false);
        flashImageDic[playerCurrentID].gameObject.SetActive(false);


    }

    /// <summary>
    /// 生成UI， isNodeEnume 決定是不是該值是MapNodeEnum的參數
    /// </summary>
    /// <param name="nodeEnum"></param>
    /// <param name="buttonPosition"></param>
    /// <param name="isNodeEnume"></param> 判斷
    /// <returns></returns>
    private UIMapButton createUI(int nodeEnum, Vector3 buttonPosition, int isNodeEnume = 1)
    {
        UIMapButton mapButton = GetUIMapButtonObject(nodeEnum);
        mapButton.SetUIPosition(gameObject.transform, buttonPosition);
        mapButton.Init(ButtonImages[nodeEnum + isNodeEnume], UIMapButton.MapButtonState.Disable, nodeEnum + isNodeEnume);
        return mapButton;
    }



    /// <summary>
    /// 取得ButtonObject，判斷依據為是不是Boss關卡
    /// </summary>
    /// <param name="mapNodeEnum"></param> 
    /// <returns></returns>
    private UIMapButton GetUIMapButtonObject(int mapNodeEnum)
    {
        if (IsBigUIorNot(mapNodeEnum))
            return assetManager.GetObject<UIMapButton>(BMapButton);

        return assetManager.GetObject<UIMapButton>(SMapButton);
    }

    private Boolean IsBigUIorNot(int mapNodeEnum)
    {
        if (mapNodeEnum == (int)MapNodeEnum.Boss || mapNodeEnum == 8)
            return true;
        return false;
    }

    private UnityEngine.UI.Image GetImageObject(int mapNodeEnum)
    {
        if (IsBigUIorNot(mapNodeEnum))
            return flashImage[1];

        return flashImage[0];
    }

    private async UniTask uIPerformanceAsync(int playerCurrentID)
    {
        await showMapButtonAsync();
        var currentNode = dataTableManager.GetDungeonDataDefine(playerCurrentID);
        //上一層暗
        foreach (var node in currentButtonDic)
        {
            var nodeLayerNumber = dataTableManager.GetDungeonDataDefine(node.Key).mapLayer;
            if (currentNode.mapLayer == nodeLayerNumber)
            {
                node.Value.ChangeImage(UIMapButton.MapButtonState.Pass);
                node.Value.RemoveAllListerner();
            }
        }
        flashSequence = DOTween.Sequence();
        //下一層亮
        foreach (var nodeIDString in currentNode.pathNodes)
        {
            int nodeID = Convert.ToInt32(nodeIDString);
            var ui = currentButtonDic[nodeID];
            addFlashSequence(flashImageDic[nodeID]);

            ui.ChangeImage(UIMapButton.MapButtonState.Enable);
            ui.AddListener(async () =>
            {
                Debug.Log("ID:" + nodeIDString + "Clicked");

                await sdk.BattleNextLevel(nodeID);
                await FadeOut();
                uIManager.RemoveUI<UIMap>();
            });
        }
        flashSequence.SetLoops(-1, LoopType.Restart);
    }

    private async UniTask showMapButtonAsync()
    {
        var sequence = DOTween.Sequence();
        sequence.Join(playerUI.ZoomIn(zoomInValue, zoomInOutTime));
        foreach (var buttonDic in currentButtonDic)
        {
            sequence.Join(buttonDic.Value.ZoomIn(zoomInValue, zoomInOutTime));
        }
        await sequence.AsyncWaitForCompletion();

    }

    private void addFlashSequence(UnityEngine.UI.Image image)
    {
        image.gameObject.SetActive(true);
        var sequence = DOTween.Sequence();
        float flashScaleSize = image.transform.localScale.x + 0.1f;
        sequence.Join(image.transform.DOScale(new Vector3(flashScaleSize, flashScaleSize, flashScaleSize), flashScaleTime))
        .Join(image.DOFade(flashFadeValue, flashFadeTime));

        flashSequence.Join(sequence);

    }

    /// <summary>
    /// UI Map Fade Out Function
    /// </summary>
    /// <returns></returns>
    public async UniTask FadeOut()
    {
        if (flashSequence != null)
            flashSequence.Kill();
        var sequence = DOTween.Sequence();
        sequence.Join(root.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));
        sequence.Join(gameObject.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));
        sequence.Join(flashRoot.GetComponent<CanvasGroup>().DOFade(fadeOutValue, fadeOutTime));

        sequence.Join(playerUI.ZoomOut(zoomOutValue, zoomInOutTime));
        foreach (var buttonDic in currentButtonDic)
        {
            sequence.Join(buttonDic.Value.ZoomOut(zoomOutValue, zoomInOutTime));
        }
        foreach (var flashImage in flashImageDic)
        {
            sequence.Join(flashImage.Value.transform.DOScale(zoomOutValue, zoomInOutTime));
        }

        await sequence.AsyncWaitForCompletion();
    }


    public override void OnClose()
    {

        base.OnClose();
    }
}
