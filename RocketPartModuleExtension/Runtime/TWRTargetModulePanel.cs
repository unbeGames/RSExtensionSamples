using UnbeGames.UI;

namespace PartModuleExtensionSample {
  // this is a module panel that will be visible when you will open part panel during the flight
  public class TWRTargetModulePanel : PartModulePanel<TWRTargeterModule> {
    public override sbyte Priority => 16; // determines module position on UI 

    private ToggleParameter enableToggle;
    private FloatValueParameter electricity, heat;

    private FloatValueParameter respnseTime;
    
    private SliderParameter targetTWRParam;
    private InputFloatBinding targetTWRBinding; // automatically maps slider to input

    // cached values for electricity and heat
    private int lastElectricityFlow, lastHeatFlow;


    // Enable toggle, electricity and heat parameters are builtin controls
    // part module should implement IEnablableModule, IHeatFlow, IElectirictyFlow
    // and then they can be binded semi-automatically
    private void Awake() {
      SetCaption("TWR Targeter");

      // built-in UI elements
      enableToggle = GetEnableToggle();
      electricity = GetElecticityParameter();
      heat = GetHeatParameter();

      // built-in bindings
      BindEnabled(enableToggle, module);
      BindElectricity(electricity, module);
      BindHeat(heat, module);

      // our custom controls
      respnseTime = CreateFloatValueParameter("Response time");
      // binding our custom value perameter this way will automatically check bitrate and electricity
      // and turn off data if there is not enough of one of it
      Bind(respnseTime, ResponseTime, bitrate: 8);

      targetTWRParam = CreateSliderInputParameter("Target TWR", out targetTWRBinding);
      // third parameter sets the function that will be called when you change the control
      // you can use GetBinding<float>(TWRTargeterModule.targetTWRStr) instead as a shortcut
      Bind(targetTWRParam, TargetTWR, SetTargetTWR);
    }

    // called right after new module was assigned
    // used to setup additional module-dependent data
    protected override void OnSetup() {
      base.OnSetup();

      targetTWRParam.Set(module.TargetTWR);
      // here an example if min-max TWR range was configured per module
      // if it is the same, it could be done in Awake
      targetTWRBinding.SetMinMax(0.1f, 10, remapTo01: false);
    }

    // parameters are interactable controls that could change part module state
    // they are updated every simulation step update
    public override void UpdateParameters() {
      enableToggle.Fetch();
      targetTWRParam.Fetch();
    }

    // Stats are updated 10 time per second, this is your displayed module stats
    protected override void UpdateStatsInternal() {
      FetchElectricityFlow(ref lastElectricityFlow, electricity);
      FetchHeatFlow(ref lastHeatFlow, heat);

      respnseTime.Fetch();
    }

    // this is called when spacecraft reaquires antenna connection,
    // you need to reset all cached values
    // IMPROTANT: always call base.ResetCachedStats()
    protected override void ResetCachedStats() {
      base.ResetCachedStats();
      lastElectricityFlow = lastHeatFlow = 0;
    }

    private float ResponseTime() {
      return module.ResponseTime;
    }

    private float TargetTWR() {
      return module.TargetTWR;
    }

    // IMPORTANT: you should alway use SetCommand, when sending commands to the module
    // otherwise a lot of features of part panel will stop working, delayd commands on time pause
    // or multimple commands to different parts
    private void SetTargetTWR(float value) {
      SetCommand(TWRTargeterModule.targetTWRStr, value);
    }
  }
}
