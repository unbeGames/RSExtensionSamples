using Newtonsoft.Json.Linq;
using UnbeGames.Model;

namespace PartModuleExtensionSample {
  // part module config that is the same for all parts with the same defindex
  // that have this module
  public struct TWRTargeterConfig {
    public float responseTime; // how quickly module can change engine throttle, fraction per second
    public float consumption; // electricity consumption W / s
    public float eceCoeff; // energy efficiency coefficient, in 0 - 1 range;
  }

  // here we are storing part module state, like configuration that could be changed
  // during the flight
  public struct TWRTargeterState {
    public bool isEnabled;
    public float targetTWR;
  }

  // implement interfaces IEnablableModule, IHeatFlow, IElectirictyFlow, this is a common functionality
  // for a lot of modules, so some of that streamlined for UI for example
  public class TWRTargeterModule : RocketModule<TWRTargeterConfig, TWRTargeterState>, IEnablableModule, IHeatFlow, IElectirictyFlow {
    public float TargetTWR => state.targetTWR;
    public float ResponseTime => config.responseTime;
        
    public float Consumption => config.consumption;
    public float EceCoeff => config.eceCoeff;

    // interfaces implementation
    public bool IsEnabled { get => state.isEnabled; private set => state.isEnabled = value; }
    public float ElectricityFlow => electricityConsIndex == -1 ? 0 : -electricityConsumptionSys.CurrElectricityConsumption(electricityConsIndex);
    public float HeatFlow => electricityConsIndex == -1 ? 0 : electricityConsumptionSys.CurrThermoProduction(electricityConsIndex);


    // all systems that module needs to be added
    // since rocket consumes electricity and produce heat, we need to add it to those systems too
    private TWRTargeterSystem twrTargeterSystem;
    private ElectricityConsumptionSystem electricityConsumptionSys;

    // saved system indices, module need to track them
    // IMPORTANT: always initialize it with -1
    private int twrTargeterIndex = -1, electricityConsIndex = -1;

    // name could be anything, but it is better use "{YourExtensionName}.{ClassName}"    
    internal const string extensionPartModuleName = "PartModuleExtensionSample.TWRTargeterModule";
    internal const string targetTWRStr = "targetTWR";

    public TWRTargeterModule(RocketPart part, JObject config, JObject state) : base(part, config, state) {
    }

    // rocket part was initialized, we can request other part modules from it
    public override void Init() {
      
    }

    // rocket part was attached to the rocket
    // here we adding our module to all rocket systems that need to process it
    // and saving returned index to respective system
    public override void Attach(Rocket rocket) {
      // this system is also a multi-module one, but other than that it needs to know
      // thermoproducer index, so it can automatically produce heat when electricity is consuned
      electricityConsumptionSys = rocket.GetSystem<ElectricityConsumptionSystem>();
      electricityConsIndex = electricityConsumptionSys.Add(part, GetType(), Consumption, 1 - EceCoeff);

      // add module to the system itself
      twrTargeterSystem = rocket.GetSystem<TWRTargeterSystem>();
      twrTargeterIndex = twrTargeterSystem.Add(part, config, state, electricityConsIndex);
    }

    // rocket part was detached from the rocket
    public override void Detach(Rocket rocket) {
      RemoveFromSystem(ref twrTargeterSystem, ref twrTargeterIndex);
      RemoveFromSystem(ref electricityConsumptionSys, ref electricityConsIndex);
    }

    // system index was changed, determine what system and update respective index
    public override void ChangeSystemIndex<T>(T system, int oldIndex, int index) {
      switch (system) {
        case TWRTargeterSystem _:
          twrTargeterIndex = index;
          break;
        case ElectricityConsumptionSystem _:
          electricityConsIndex = index;
          // electricity consumption changed index, need to update it in twr targeter system
          twrTargeterSystem.UpdateConsumptionIndex(twrTargeterIndex, electricityConsIndex);
          break;
      }
    }

    // part system index changed, part system index correspond to systems, that every rocket part has
    // electricity system uses this index to store produced heat in the thermo production system
    public override void ChangePartSystemIndex(int sysIndex) {
      electricityConsumptionSys?.UpdateSysIndex(electricityConsIndex, sysIndex);
    }

    // this method is called when command was sent from the part panel UI to the module
    public override void Execute(JObject overrides) {
      // built-in command
      if (overrides.TryGetValue(enableStr, out var token)) {
        SetEnabled(token.ToObject<bool>());
      }
      // out custom command
      if(overrides.TryGetValue(targetTWRStr, out token)) {
        SetTargetTWR(token.ToObject<float>());
      }
    }

    public void SetEnabled(bool isEnabled) {
      if (IsEnabled == isEnabled) {
        return;
      }
      IsEnabled = isEnabled;
      // do not forget to update state in the system
      twrTargeterSystem?.UpdateState(twrTargeterIndex, isEnabled);
    }

    public void SetTargetTWR(float targetTWR) {
      if(state.targetTWR == targetTWR) {
        return;
      }
      state.targetTWR = targetTWR;
      // don't forget update this value in system
      twrTargeterSystem?.UpdateTargetTWR(twrTargeterIndex, state.targetTWR);
    }
  }
}
