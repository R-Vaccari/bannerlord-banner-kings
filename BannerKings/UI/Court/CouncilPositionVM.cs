using System;
using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Court;

public class CouncilPositionVM : HeroVM
{
    private readonly CouncilMember position;
    private readonly Action<string> setId;
    private readonly Action<string> updatePosition;

    public CouncilPositionVM(CouncilMember position, Action<string> setId, Action<string> updatePosition) : base(
        position.Member)
    {
        this.position = position;
        this.setId = setId;
        this.updatePosition = updatePosition;
    }

    [DataSourceProperty] public string Title => position.GetName().ToString();

    private void SetId()
    {
        if (setId != null)
        {
            setId(position.Position.ToString());
        }
    }

    private void UpdatePosition()
    {
        if (updatePosition != null)
        {
            updatePosition(position.Position.ToString());
        }
    }
}