using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the standalone BattleTest scene: a clean orthographic camera, the battlefield world-space
/// canvas with the two formations, the HUD, the battle driver and its config panel, and an event
/// system. The scene needs no legacy raid objects and no manager prefab — the core battle content is
/// loaded through <c>FightContentLoader</c> and rendered by the thin view layer.
/// </summary>
public static class BattleTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/BattleTest.unity";

    private const float CameraX = 0f;
    private const float CameraSize = 32f;
    private const float WorldCanvasScale = 0.1f;

    private static CoreBattleDriver _driver;
    private static BattleTestConfigPanel _configPanel;

    /// <summary>Menu entry to rebuild the BattleTest scene.</summary>
    [MenuItem("Tools/Battle Test/Create Scene")]
    public static void CreateScene()
    {
        BuildScene();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        Debug.Log("[BattleTest] Scene saved: " + ScenePath);
    }

    /// <summary>Batch-mode entry point: builds and saves the BattleTest scene.</summary>
    public static void Generate()
    {
        BuildScene();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        Debug.Log("[BattleTest] Scene generated: " + ScenePath);
    }

    private static void BuildScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        _driver = null;
        _configPanel = null;

        CreateCamera();

        GameObject managerObject = new GameObject("BattleTestManager");
        _driver = managerObject.AddComponent<CoreBattleDriver>();
        _configPanel = managerObject.AddComponent<BattleTestConfigPanel>();

        GameObject battlefield = CreateWorldCanvas();
        SetDriverField(battlefield.transform);

        GameObject heroFormation = CreateFormation("HeroFormation", battlefield.transform);
        GameObject monsterFormation = CreateFormation("MonsterFormation", battlefield.transform);
        SetDriverFormation("heroFormation", heroFormation.GetComponent<BattleFormationView>());
        SetDriverFormation("monsterFormation", monsterFormation.GetComponent<BattleFormationView>());

        GameObject hudObject = new GameObject("BattleHud");
        hudObject.transform.SetParent(managerObject.transform, false);
        SetDriverHud(hudObject.AddComponent<BattleHud>());

        SetConfigPanelDriver();
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = CameraSize;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.08f, 0.1f);
        cameraObject.transform.position = new Vector3(CameraX, 0f, -10f);
    }

    private static GameObject CreateWorldCanvas()
    {
        GameObject battlefield = new GameObject("Battlefield");
        RectTransform rect = battlefield.AddComponent<RectTransform>();
        Canvas canvas = battlefield.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 0;
        rect.position = Vector3.zero;
        rect.sizeDelta = new Vector2(1920, 1080);
        rect.localScale = new Vector3(WorldCanvasScale, WorldCanvasScale, WorldCanvasScale);
        return battlefield;
    }

    private static GameObject CreateFormation(string name, Transform parent)
    {
        GameObject formationObject = new GameObject(name);
        formationObject.transform.SetParent(parent, false);
        formationObject.AddComponent<RectTransform>();
        formationObject.AddComponent<BattleFormationView>();
        return formationObject;
    }

    private static void SetDriverField(Transform fieldRoot)
    {
        SerializedObject serialized = new SerializedObject(_driver);
        serialized.FindProperty("fieldRoot").objectReferenceValue = fieldRoot;
        serialized.ApplyModifiedProperties();
    }

    private static void SetDriverFormation(string propertyName, BattleFormationView view)
    {
        SerializedObject serialized = new SerializedObject(_driver);
        serialized.FindProperty(propertyName).objectReferenceValue = view;
        serialized.ApplyModifiedProperties();
    }

    private static void SetDriverHud(BattleHud hud)
    {
        SerializedObject serialized = new SerializedObject(_driver);
        serialized.FindProperty("hud").objectReferenceValue = hud;
        serialized.ApplyModifiedProperties();
    }

    private static void SetConfigPanelDriver()
    {
        SerializedObject serialized = new SerializedObject(_configPanel);
        serialized.FindProperty("driver").objectReferenceValue = _driver;
        serialized.ApplyModifiedProperties();
    }
}