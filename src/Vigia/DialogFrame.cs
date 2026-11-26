// Vigia -- the frame shared by the app's own dialogs.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 26, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Vigia;

/// <summary>The look of InputDialog, ChoiceDialog, AboutDialog and ThemeDialog: a borderless window centred on its
/// owner, holding a rounded card with a soft shadow in the theme's popup colours. Dragging the card moves the window.</summary>
internal static class DialogFrame
{
    /// <summary>Makes <paramref name="w"/> a borderless, transparent dialog of its content's size, centred on
    /// <paramref name="owner"/> and kept off the taskbar.</summary>
    public static void Setup(Window w, Window owner)
    {
        w.Owner = owner;
        w.WindowStyle = WindowStyle.None;
        w.AllowsTransparency = true;
        w.Background = Brushes.Transparent;
        w.ResizeMode = ResizeMode.NoResize;
        w.SizeToContent = SizeToContent.WidthAndHeight;
        w.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        w.ShowInTaskbar = false;
    }

    /// <summary>The card around a dialog's content. With <paramref name="dragFromCardOnly"/>, only a press on the card
    /// itself (not on a child such as a colour tile) starts dragging the window.</summary>
    public static Border Card(Window w, UIElement content, Thickness padding, bool dragFromCardOnly = false)
    {
        var card = new Border
        {
            CornerRadius = new CornerRadius(14), Padding = padding, Margin = new Thickness(24),
            BorderThickness = new Thickness(1), Child = content,
            Effect = new DropShadowEffect { BlurRadius = 36, ShadowDepth = 10, Direction = 270, Opacity = 0.18 },
        };
        card.SetResourceReference(Border.BackgroundProperty, "Gm.Popup");
        card.SetResourceReference(Border.BorderBrushProperty, "Gm.PopupBorder");
        card.MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed && (!dragFromCardOnly || e.Source == card)) w.DragMove();
        };
        return card;
    }
}
