using System.ComponentModel;

namespace Mistaken.CustomCuffing;

internal sealed class Config
{
    [Description("If true then debug will be displayed")]
    public bool Debug { get; set; } = false;

    [Description("How many people one player can disarm")]
    public int CuffLimit { get; set; } = 1;

    [Description("After what distance from the disarming player should the cuffed player be uncuffed")]
    public float AutoDisarmDistance { get; set; } = 90f;

    [Description("Can Scps release disarmed players")]
    public bool AllowScps { get; set; } = false;

    [Description("Can MTFs release someone when they are cuffed by another MTF Unit")]
    public bool AllowOtherMtfs { get; set; } = true;
}
