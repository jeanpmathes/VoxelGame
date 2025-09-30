// <copyright file="ResourceLoadingReportHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Presents a <see cref="ResourceLoadingIssueReport" /> to the user via the <see cref="StartUserInterface" />.
/// </summary>
public class ResourceLoadingReportHook : SceneComponent, IConstructible<Scene, (ResourceLoadingIssueReport, StartUserInterface), ResourceLoadingReportHook>
{
    private readonly ResourceLoadingIssueReport report;
    private readonly StartUserInterface ui;

    private ResourceLoadingReportHook(Scene scene, ResourceLoadingIssueReport report, StartUserInterface ui) : base(scene)
    {
        this.report = report;
        this.ui = ui;
    }

    /// <inheritdoc />
    public static ResourceLoadingReportHook Construct(Scene input1, (ResourceLoadingIssueReport, StartUserInterface) input2)
    {
        return new ResourceLoadingReportHook(input1, input2.Item1, input2.Item2);
    }

    /// <inheritdoc />
    public override void OnLoad()
    {
        ui.PresentResourceLoadingIssueReport(report.Report, report.AnyErrors);
    }
}
