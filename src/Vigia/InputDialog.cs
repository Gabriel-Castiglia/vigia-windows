// Vigia -- a small dialog that asks for one line of text.
// Copyright (C) 2026 Gabriel Castiglia. Licensed under the GNU GPL, version 2 only.
// Date: November 10, 2026
// SPDX-License-Identifier: GPL-2.0-only

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vigia;

/// <summary>Small rounded dialog with labelled fields (link, image, table). Built in code to stay in one place.</summary>
public sealed class InputDialog : Window
{
    private readonly List<TextBox> _fields = new();
    public string[] Values => _fields.Select(f => f.Text.Trim()).ToArray();

    public InputDialog(Window owner, string title, (string Label, string Value)[] fields, Func<InputDialog, int, Button?>? extraButton = null)
    {
        DialogFrame.Setup(this, owner);

        var stack = new StackPanel { Width = 380 };
        var head = new TextBlock
        {
            Text = title, FontSize = 15, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 14),
            FontFamily = (FontFamily)FindResource("Gm.Font"),
        };
        head.SetResourceReference(TextBlock.ForegroundProperty, "Gm.Text");
        stack.Children.Add(head);
        for (int i = 0; i < fields.Length; i++)
        {
            var lbl = new TextBlock { Text = fields[i].Label, FontSize = 12, Margin = new Thickness(0, 0, 0, 5), FontFamily = (FontFamily)FindResource("Gm.Font") };
            lbl.SetResourceReference(TextBlock.ForegroundProperty, "Gm.TextMuted");
            var tb = new TextBox { Text = fields[i].Value, Style = (Style)FindResource("Gm.Field") };
            _fields.Add(tb);
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
            var extra = extraButton?.Invoke(this, i);
            if (extra != null) { DockPanel.SetDock(extra, Dock.Right); row.Children.Add(extra); }
            row.Children.Add(tb);
            stack.Children.Add(lbl);
            stack.Children.Add(row);
        }
        var ok = new Button { Content = "OK", IsDefault = true, Style = (Style)FindResource("Gm.DialogButton") };
        var cancel = new Button { Content = Strings.Get("Cancel"), IsCancel = true, Style = (Style)FindResource("Gm.DialogButton") };
        ok.Click += (_, _) => DialogResult = true;
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 6, 0, 0) };
        buttons.Children.Add(cancel); buttons.Children.Add(ok);
        stack.Children.Add(buttons);

        var card = DialogFrame.Card(this, stack, new Thickness(22, 20, 22, 18));
        Content = card;
        Loaded += (_, _) => (_fields.FirstOrDefault(f => f.Text.Length == 0) ?? _fields.FirstOrDefault())?.Focus();   // the first empty field
    }

    public void SetField(int i, string value) => _fields[i].Text = value;
}
