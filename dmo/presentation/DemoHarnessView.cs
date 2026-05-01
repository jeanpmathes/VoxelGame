using System;
using VoxelGame.GUI;
using VoxelGame.GUI.Commands;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Texts;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.Presentation.Demo;

internal static class DemoHarnessView
{
    internal static Control Create(DemoHarness harness)
    {
        return new Border
        {
            HorizontalAlignment = {Value = HorizontalAlignment.Stretch},
            VerticalAlignment = {Value = VerticalAlignment.Stretch},
            Margin = {Value = new ThicknessF(5)},
            Padding = {Value = new ThicknessF(5)},

            Child = new LinearLayout
            {
                Children =
                {
                    new Border
                    {
                        MinimumWidth = {Value = 500f},
                        MinimumHeight = {Value = 250f},
                        HorizontalAlignment = {Value = HorizontalAlignment.Center},
                        VerticalAlignment = {Value = VerticalAlignment.Center},
                    },
                    new Text
                    {
                        MaximumWidth = {Value = 100f},
                        TextTrimming = {Value = TextTrimming.CharacterEllipsis},

                        Visibility = {Value = Visibility.Collapsed},

                        Content =
                        {
                            Value =
                                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a diam lectus. Sed sit amet ipsum mauris. Maecenas congue ligula ac quam viverra nec consectetur ante hendrerit. Donec et mollis dolor. Praesent et diam eget libero egestas mattis sit amet vitae augue. Nam tincidunt congue enim, ut porta lorem lacinia consectetur. Donec ut libero sed arcu vehicula ultricies a non tortor. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                        }
                    },
                    new LinearLayout
                    {
                        Orientation = {Value = Orientation.Vertical},

                        Children =
                        {
                            new Button<String>
                            {
                                Content = {Value = "Click Me"},

                                Margin = {Value = new ThicknessF(30)},

                                // todo: fix that click currently does not work - check input stuff
                                Command = {Value = Command.FromAction(() => harness.Write("Button clicked!"))}
                            },
                            new Button<String>
                            {
                                Content = {Value = "Click Me"},

                                Margin = {Value = new ThicknessF(30)}
                            },
                            new Button<String>
                            {
                                Content = {Value = "Click Me"},

                                Margin = {Value = new ThicknessF(30)},

                                Command = {Value = Command.FromAction(() => harness.Write("Button clicked!"))}
                            }
                        }
                    },
                    new Text
                    {
                        Content = {Value = "Hello, World!"}
                    },
                    new Text
                    {
                        MaximumWidth = {Value = 100f},
                        TextTrimming = {Value = TextTrimming.CharacterEllipsis},

                        Content =
                        {
                            Value =
                                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec a diam lectus. Sed sit amet ipsum mauris. Maecenas congue ligula ac quam viverra nec consectetur ante hendrerit. Donec et mollis dolor. Praesent et diam eget libero egestas mattis sit amet vitae augue. Nam tincidunt congue enim, ut porta lorem lacinia consectetur. Donec ut libero sed arcu vehicula ultricies a non tortor. Lorem ipsum dolor sit amet, consectetur adipiscing elit."
                        }
                    }
                }
            }
        };
    }
}
