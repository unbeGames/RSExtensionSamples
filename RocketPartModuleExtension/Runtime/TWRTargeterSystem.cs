using UnbeGames.Model;
using Unity.Collections;
using Unity.Mathematics;

namespace PartModuleExtensionSample {
  // we are using RocketSingleModuleSystem here because this system process modules of a single type
  public class TWRTargeterSystem : RocketSingleModuleSystem<TWRTargeterModule> {
    // you need to store all data in arrays, because RocketPartSystem expects that
    // also this way you can easily expose it using ReadOnlySpan<T> with other systems
    private float[] targetTWR;
    private float[] responseTime;
    
    private NativeArray<float> state; // enabled / disabled in float form for 
    private NativeArray<int> electricityConsumptionIndices;

    private readonly ElectricityConsumptionSystem electricityConsumption;
    private readonly EngineSystem engineSystem;
    private readonly ElectricitySystem electricity;

    public TWRTargeterSystem(Rocket rocket) : base(rocket) {
      var count = DefaultCount;

      // allocate regualr arrays
      targetTWR = new float[count];
      responseTime = new float[count];

      // electricity system uses different array type for storing data that's why we doing it that way
      state = Allocate<float>(count);
      electricityConsumptionIndices = Allocate<int>(count);

      // get all systems that needs to be interacted with
      electricityConsumption = rocket.GetSystem<ElectricityConsumptionSystem>();
      electricity = rocket.electricity; // refernce for some sytems saved in the rocket
      engineSystem = rocket.GetSystem<EngineSystem>();
    }

    public int Add(RocketPart part, TWRTargeterConfig config, TWRTargeterState state, int electricityConsIndex) {
      // getting module position in arrays
      var index = Add(part);

      // setting data to arrays
      responseTime[index] = config.responseTime;
                 
      UpdateTargetTWR(index, state.targetTWR);
      UpdateState(index, state.isEnabled);
      UpdateConsumptionIndex(index, electricityConsIndex);

      return index;
    }

    // implementation of system functionality
    // time represents what time it was when simulation step started
    // that means current time is time + deltaTime
    protected override void SimulateInternal(double time, double deltaTime) {
      // not a single module is present or engines not working, early out
      if(count == 0 || !engineSystem.IsRunning) {
        return;
      }

      var currCommandModule = rocket.CurrCommandModule;

      var primaryData = currCommandModule.PrimaryData;
      var commandState = currCommandModule.CurrState;

      var currPressure = commandState.currPressure;

      var currThrottle = engineSystem.Throttle; // calculated total throtle, including thrust limiter and enabled state
      var thrustRange = engineSystem.ThrustRange; // x - thrust at sea level, y - thrust in vacuum
      var throttleConfig = engineSystem.ThrottleConfig; // x - throttle, y - thrust limiter, z - enabled state
      var engineIds = engineSystem.PartId;

      // calculating current total thrust and max possible thrust in one loop
      // max possible thrust account for thrust limiter and ebabled state of engines
      var currTotalThrust = 0f;
      var maxPossibleThrust = 0f;
      for(int i = 0; i < engineSystem.Count; i++) {
        var thrust = thrustRange[i];
        var maxThrust = Constants.MaxThrust((float)currPressure, thrust.x, thrust.y);
        currTotalThrust += maxThrust * currThrottle[i];

        var throttleConf = throttleConfig[i];
        maxPossibleThrust += maxThrust * throttleConf.y * throttleConf.z;
      }      

      // no enabled egines, nothing to work with
      if(maxPossibleThrust == 0) {
        return;
      } 

      var gForce = (float)primaryData.primary.Gravity(math.length(commandState.position));
      var currTWR = currTotalThrust / ((float)rocket.mass * gForce);

      // iterate over each active module and calculate TWR change
      // in this case it would be nice to make it so only one module could be active
      // even beter to make it as instrument, but the goal is to should API example, so oh well

      // IMPROTANT: always use count, not the array length, becaue array have bigger
      // size than actual number of modules in the system
      for (int i = 0; i < count; i++) {
        var target = targetTWR[i];
        var delta = (target - currTWR) * state[i]; 
        // if module is off, state is 0, then delta is 0, and we don't need to do anything
        // this is also true if targetTWR is reached
        if(math.abs(delta) > math.EPSILON) {
          // we can actually calculate it once and save to array instead of responseTime
          // but for example this is simpler and is fine
          var maxChangePerSecond = 1 / responseTime[i];
          // IMPORTANT: don't forget to use deltaTime if you have continuous process
          var maxChange = maxChangePerSecond * (float)deltaTime;
          delta = math.clamp(delta, -maxChange, maxChange);
          var finalTWR = currTWR + delta;

          // calculating desired thrust
          var targetTotalThrust = (float)rocket.mass * gForce * finalTWR;
          var targetThrottle = math.clamp(targetTotalThrust / maxPossibleThrust, 0, 1);

          for(int j = 0; j < engineSystem.Count; j++) {
            var throttleConf = throttleConfig[i];
            // engine is enabled and limiter > 0
            if (throttleConf.y * throttleConf.z != 0) {
              // we should change any module parameters via module
              var enginePart = rocket.PartById(engineIds[i]);
              var engineModule = enginePart.GetModule<EngineModule>();
              engineModule.SetThrottle(targetThrottle);
            }
          }
        }
      }

      // consume electricity
      electricityConsumption.RequestConsumption(electricity.State, count, state, electricityConsumptionIndices);
    }

    internal void UpdateConsumptionIndex(int index, int electricityConsIndex) {
      electricityConsumptionIndices[index] = electricityConsIndex;
    }

    internal void UpdateTargetTWR(int index, float targetTWR) {
      this.targetTWR[index] = targetTWR;
    }

    internal void UpdateState(int twrTargeterIndex, bool isEnabled) {
      state[twrTargeterIndex] = isEnabled ? 1 : 0;
    }

    // IMPROTANT: we should resize all our data arrays
    // when new module was added but we run out of space
    protected override void OnResize() {
      Resize(ref targetTWR);
      Resize(ref state);
      Resize(ref electricityConsumptionIndices);
    }

    // IMPORTANT: module position could change when parts with similar modules get destroyed
    // so we need to move data in corresponding positions
    protected override void ChangedSystemIndex(int oldIndex, int index) {
      targetTWR[index] = targetTWR[oldIndex];
      state[index] = state[oldIndex];
      electricityConsumptionIndices[index] = electricityConsumptionIndices[oldIndex];

      base.ChangedSystemIndex(oldIndex, index);
    }

    // if we are using NativeArrays we should always dispose them here
    // otherwise memory leak
    protected override void DisposeInternal() {
      state.Dispose();
      electricityConsumptionIndices.Dispose();
    }
  }
}
