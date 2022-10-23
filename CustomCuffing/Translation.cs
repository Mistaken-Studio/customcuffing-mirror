// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Mistaken.CustomCuffing
{
    internal sealed class Translation : ITranslation
    {
        [Description("Message that appears when you hover on a Player when hes Cuffed")]
        public string CuffedBy { get; set; } = "Cuffed by {0}";

        [Description("Message that displays a list of Cuffed players for the Cuffer")]
        public string CufferListOfTargetsInfo { get; set; } = "Cuffed Players: (<color=yellow>{0}/{1}</color>)<br><br>{2}";

        [Description("Message format for elements of the Cuffed players list for the Cuffer")]
        public string CufferElementOfListInfo { get; set; } = "<color=yellow>{0}</color> - <color=yellow>{1}</color>m away";

        [Description("Message that appears when Cuffer exceeds the cuffing limit")]
        public string CuffingLimitInfo { get; set; } = "<color=red>You can't cuff any player! (You can cuff max {0} people)</color>";

        [Description("Message that appears when Cuffer tries to cuff someone while moving")]
        public string CufferMovingWhenCuffingInfo { get; set; } = "<color=red>You can't cuff that player! (You're moving)</color>";

        [Description("Message that appears when Cuffer tries to cuff someone when the target is moving")]
        public string TargetMovingWhenCuffingInfo { get; set; } = "<color=red>You can't cuff that player! (Your Target is moving)</color>";

        [Description("Message that appears for Player who's from the same team as Cuffer when he attacks Cuffed player")]
        public string AttackingCuffedInfo { get; set; } = "<color=yellow>You have attacked a cuffed player: {0} (Cuffer: {1})</color>";

        [Description("Message that appears for Cuffed player when he's attacked by a Player from the same team as Cuffer's")]
        public string CuffedAttackedInfo { get; set; } = "<color=yellow>You have been attacked by: {0}</color>";
    }
}
