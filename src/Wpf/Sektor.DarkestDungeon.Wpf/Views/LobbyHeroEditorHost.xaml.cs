using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Sektor.DarkestDungeon.Wpf.ViewModels;

namespace Sektor.DarkestDungeon.Wpf.Views
{
    /// <summary>Full-window modal host for the hero loadout editor. The lobby screens place it over
    /// their root grid and call <see cref="Open"/> when a party slot requests editing.</summary>
    public partial class LobbyHeroEditorHost : UserControl
    {
        /// <summary>Initializes a new instance of the <see cref="LobbyHeroEditorHost"/> class.</summary>
        public LobbyHeroEditorHost()
        {
            InitializeComponent();
        }

        /// <summary>Opens the editor for the selected hero slot.</summary>
        /// <param name="slot">The slot to edit.</param>
        public void Open(HeroSlotViewModel slot)
        {
            Editor.DataContext = slot;
            Visibility = Visibility.Visible;
            Focus();
        }

        private void OnCloseRequested(object? sender, EventArgs e)
        {
            CloseEditor();
        }

        private void OnBackdropClick(object sender, MouseButtonEventArgs e)
        {
            CloseEditor();
            e.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseEditor();
                e.Handled = true;
            }
        }

        private void CloseEditor()
        {
            Visibility = Visibility.Collapsed;
            Editor.DataContext = null;
        }
    }
}
