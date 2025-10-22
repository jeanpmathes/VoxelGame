// <copyright file="ResourceLoadingReportHook.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.UserInterfaces;

namespace VoxelGame.Client.Scenes.Components;

/// <summary>
///     Presents a <see cref="ResourceLoadingIssueReport" /> to the user via the <see cref="StartUserInterface" />.
/// </summary>
public partial class ResourceLoadingReportHook : SceneComponent
{
    private readonly ResourceLoadingIssueReport report;
    private readonly StartUserInterface ui;

    [Constructible]
    private ResourceLoadingReportHook(Scene scene, ResourceLoadingIssueReport report, StartUserInterface ui) : base(scene)
    {
        this.report = report;
        this.ui = ui;
    }

    /// <inheritdoc />
    public override void OnLoad()
    {
        ui.PresentResourceLoadingIssueReport(report.Report, report.AnyErrors);
    }
}
