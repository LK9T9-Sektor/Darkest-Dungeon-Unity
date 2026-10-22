using System.Windows;
using System.Windows.Controls;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Scene, room, round, turn, torch and gold bar shared by battle screens.</summary>
    public partial class BattleSceneBarView : UserControl
    {
        /// <summary>Identifies the <see cref="Scene"/> dependency property.</summary>
        public static readonly DependencyProperty SceneProperty = DependencyProperty.Register(
            nameof(Scene),
            typeof(string),
            typeof(BattleSceneBarView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Room"/> dependency property.</summary>
        public static readonly DependencyProperty RoomProperty = DependencyProperty.Register(
            nameof(Room),
            typeof(string),
            typeof(BattleSceneBarView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="RoomType"/> dependency property.</summary>
        public static readonly DependencyProperty RoomTypeProperty = DependencyProperty.Register(
            nameof(RoomType),
            typeof(string),
            typeof(BattleSceneBarView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Round"/> dependency property.</summary>
        public static readonly DependencyProperty RoundProperty = DependencyProperty.Register(
            nameof(Round),
            typeof(int),
            typeof(BattleSceneBarView),
            new PropertyMetadata(0));

        /// <summary>Identifies the <see cref="Turn"/> dependency property.</summary>
        public static readonly DependencyProperty TurnProperty = DependencyProperty.Register(
            nameof(Turn),
            typeof(string),
            typeof(BattleSceneBarView),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Torch"/> dependency property.</summary>
        public static readonly DependencyProperty TorchProperty = DependencyProperty.Register(
            nameof(Torch),
            typeof(int),
            typeof(BattleSceneBarView),
            new PropertyMetadata(0));

        /// <summary>Identifies the <see cref="Gold"/> dependency property.</summary>
        public static readonly DependencyProperty GoldProperty = DependencyProperty.Register(
            nameof(Gold),
            typeof(int),
            typeof(BattleSceneBarView),
            new PropertyMetadata(0));

        /// <summary>Initializes a new instance of the <see cref="BattleSceneBarView"/> class.</summary>
        public BattleSceneBarView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the scene name.</summary>
        public string Scene
        {
            get { return (string)GetValue(SceneProperty); }
            set { SetValue(SceneProperty, value); }
        }

        /// <summary>Gets or sets the current room name.</summary>
        public string Room
        {
            get { return (string)GetValue(RoomProperty); }
            set { SetValue(RoomProperty, value); }
        }

        /// <summary>Gets or sets the current room type.</summary>
        public string RoomType
        {
            get { return (string)GetValue(RoomTypeProperty); }
            set { SetValue(RoomTypeProperty, value); }
        }

        /// <summary>Gets or sets the current round number.</summary>
        public int Round
        {
            get { return (int)GetValue(RoundProperty); }
            set { SetValue(RoundProperty, value); }
        }

        /// <summary>Gets or sets the current turn label.</summary>
        public string Turn
        {
            get { return (string)GetValue(TurnProperty); }
            set { SetValue(TurnProperty, value); }
        }

        /// <summary>Gets or sets the current torch value.</summary>
        public int Torch
        {
            get { return (int)GetValue(TorchProperty); }
            set { SetValue(TorchProperty, value); }
        }

        /// <summary>Gets or sets the current gold amount.</summary>
        public int Gold
        {
            get { return (int)GetValue(GoldProperty); }
            set { SetValue(GoldProperty, value); }
        }
    }
}
