using System;
using VoxelGame.GUI;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Commands;
using VoxelGame.GUI.Controls;

namespace VoxelGame.Presentation.Demo;

internal static class DemoHarnessView
{
    internal static Control Create(DemoHarness harness)
    {
        return new LinearLayout
        {
            Orientation = {Value = Orientation.Vertical},

            Children =
            {
                new LinearLayout
                {
                    Orientation = {Value = Orientation.Horizontal},

                    Children =
                    {
                        new LinearLayout
                        {
                            Orientation = {Value = Orientation.Vertical},

                            Children =
                            {
                                new Text
                                {
                                    Content = {Binding = Binding.To(harness.CurrentShowcase).Compute(showcase => $"Showcase: {showcase?.Name}")}
                                },
                                new Button<String>
                                {
                                    Content = {Value = "Next Showcase"},
                                    Command = {Value = Command.FromAction(harness.MoveToNextShowcase)}
                                },
                                new Button<String>
                                {
                                    Content = {Value = "Previous Showcase"},
                                    Command = {Value = Command.FromAction(harness.MoveToPreviousShowcase)}
                                }
                            }
                        },
                        new Border
                        {
                            Child = new ContentControl<Showcase>
                            {
                                Content = {Binding = Binding.To(harness.CurrentShowcase)},

                                VerticalAlignment = {Value = VerticalAlignment.Stretch},
                                HorizontalAlignment = {Value = HorizontalAlignment.Stretch}
                            }
                        }
                    }
                },
                new LinearLayout
                {
                    Orientation = {Value = Orientation.Vertical},

                    Children =
                    {
                        new Text
                        {
                            Content = {Binding = Binding.To(harness.RenderFrequency).Compute(fps => $"FPS: {fps:00.0}")}
                        },
                        new Text
                        {
                            Content = {Binding = Binding.To(harness.UpdateFrequency).Compute(fps => $"UPS: {fps:00.0}")}
                        }
                    }
                }
            }
        };
    }
}
