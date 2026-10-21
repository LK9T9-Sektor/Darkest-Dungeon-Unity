using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Reusable equipment slot: a weapon/armor glyph, a rounded level badge and a tall image
    /// placeholder (72x144, matching the Unity character window).</summary>
    public partial class EquipmentSlotView : UserControl
    {
        /// <summary>Identifies the <see cref="Glyph"/> dependency property.</summary>
        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            nameof(Glyph), typeof(string), typeof(EquipmentSlotView), new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Level"/> dependency property.</summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
            nameof(Level), typeof(string), typeof(EquipmentSlotView), new PropertyMetadata("1"));

        /// <summary>Identifies the <see cref="Image"/> dependency property.</summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            nameof(Image), typeof(ImageSource), typeof(EquipmentSlotView), new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Tooltip"/> dependency property.</summary>
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register(
            nameof(Tooltip), typeof(string), typeof(EquipmentSlotView), new PropertyMetadata(string.Empty));

        /// <summary>Initializes a new instance of the <see cref="EquipmentSlotView"/> class.</summary>
        public EquipmentSlotView()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the header glyph (crossed swords or shield).</summary>
        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        /// <summary>Gets or sets the equipment level shown in the rounded badge (without an "Lv." prefix).</summary>
        public string Level
        {
            get { return (string)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        /// <summary>Gets or sets the equipment image; null shows the placeholder.</summary>
        public ImageSource? Image
        {
            get { return (ImageSource?)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>Gets or sets the slot tooltip.</summary>
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
    }
}
