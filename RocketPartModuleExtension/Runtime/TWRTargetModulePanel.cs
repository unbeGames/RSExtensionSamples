using UnbeGames.UI;

namespace PartModuleExtensionSample {
  public class TWRTargetModulePanel : PartModulePanel<TWRTargeterModule> {
    private ToggleParameter enableToggle;
    private FloatValueParameter electricity, heat;

    // cached values for electricity and heat
    private int lastElectricityFlow, lastHeatFlow;


    // Enable toggle, electricity and heat parameters are builtin controls
    // part module should implement IEnablableModule, IHeatFlow, IElectirictyFlow
    // and then they can be binded semi-automatically
    private void Awake() {
      SetCaption("TWR Targeter");

      enableToggle = GetEnableToggle();
      electricity = GetElecticityParameter();
      heat = GetHeatParameter();

      BindEnabled(enableToggle, module);
      BindElectricity(electricity, module);
      BindHeat(heat, module);
    }

    // parameters are interactable controls that could change part module state
    // they are updated every simulation step update
    public override void UpdateParameters() {
      enableToggle.Fetch();
    }

    // this is called when spacecraft reaquires antenna connection,
    // you need to reset all cached values
    // IMPROTANT: always call base.ResetCachedStats()
    protected override void ResetCachedStats() {
      base.ResetCachedStats();
      lastElectricityFlow = lastHeatFlow = 0;
    }

    // Stats are updated 10 time per second, this is your displayed module stats
    protected override void UpdateStatsInternal() {
      FetchElectricityFlow(ref lastElectricityFlow, electricity);
      FetchHeatFlow(ref lastHeatFlow, heat);
    }
  }
}
