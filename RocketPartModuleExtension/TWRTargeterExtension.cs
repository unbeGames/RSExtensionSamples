using UnbeGames.API;

namespace PartModuleExtensionSample {
  public class TWRTargeterExtension : BaseExtension {
    // name could be anything, but it is better use "{YourExtensionName}.{ClassName}"
    private const string extensionPartModuleName = "PartModuleExtensionSample.TWRTargeterModule";

    public TWRTargeterExtension() {
    }

    // register your custom types so the game would know about it
    public override void OnEnable() {
      GameAPI.RegisterPartModuleType<TWRTargeterModule>(extensionPartModuleName);
      GameAPI.RegisterRocketSystem<TWRTargeterSystem>();
      GameAPI.RegisterPartModulePanel<TWRTargetModulePanel>();
    }

    // IMPORTANT: always deregister everything you've registered
    public override void OnDisable() {
      GameAPI.DeregisterPartModuleType<TWRTargeterModule>(extensionPartModuleName);
      GameAPI.DeregisterSystemType<TWRTargeterSystem>();
      GameAPI.DeregisterPartModulePanel<TWRTargetModulePanel>();
    }
  }
}
