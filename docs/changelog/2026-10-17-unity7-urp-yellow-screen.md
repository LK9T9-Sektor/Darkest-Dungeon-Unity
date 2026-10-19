# Unity 7 (URP): «жёлтый экран» рейда устранён, предупреждение о GUI Layer снято

- **«Жёлтый экран» рейда устранён**: причина — камеры `BlurCamera`/`UI_Dangeon/Camera` в
  `Dungeon`/`DungeonMultiplayer` были Base с `Clear Flags = Nothing`; Render Graph путь URP
  (`UniversalRendererRenderGraph.cs:478-485`) чистит фон такой камеры в `Color.yellow` — верхние
  2/3 экрана, мир (стены, пол, герои) скрыт. Исправлено камерным стеком: `DungeonCamera` (Base) +
  `BlurCamera`/UI-камера (Overlay). Детали — `mechanics\presentation\presentation_unity_urp_cameras.md`.
- **Материалы стен `Dungeon` переведены на unlit**: `CoridorWall`/`CoridorWall 1` —
  с `Universal Render Pipeline/Simple Lit` на `Sprites/Default` (unlit). На canvas-изображениях
  комнаты/коридоров лит-шейдер под URP не оставляем; на «жёлтый экран» это не влияло (его причиной
  были камеры — см. выше).
- **Предупреждение «Component GUI Layer … is no longer available» устранено**: компонент
  `GUILayer` (не существует в Unity 7) удалён из сцен (Intro, LoadingScreen, EstateManagement,
  DungeonMultiplayer, CampaignSelection) и префабов камер (`Main Camera`, `UIDungeonCamera`).
