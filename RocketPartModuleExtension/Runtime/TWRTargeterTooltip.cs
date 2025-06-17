using UnbeGames.UI;

namespace PartModuleExtensionSample {
  public class TWRTargeterTooltip : PartModuleTooltip<TWRTargeterModule> {
    private TooltipUnit responseTime;
    private TooltipUnit consumption;

    private void Awake() {
      SetCaption("TWR Targeter");

      responseTime = CreateTooltipUnit("Response time");
      consumption = CreateTooltipUnit("Consumption");
    }

    // called when cursor is hovered over part that have this module
    protected override void OnSetup() {
      responseTime.SetDuration(module.ResponseTime);
      consumption.SetPower(module.Consumption);
    }
  }
}
