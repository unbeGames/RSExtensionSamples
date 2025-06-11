using UnbeGames.API;
using UnbeGames.Model;
using UnbeGames.Services;
using UnbeGames.Support;

namespace PartModuleExtensionSample {
  public class TWRTargeterExtension : BaseExtension {
    // command module schema defindices
    private static readonly int[] commandModuleDefindices = new int[] { 1000, 1006 };

    private bool moduleWasAddedToSchema;

    public TWRTargeterExtension() {
    }

    // register your custom types so the game would know about it when extension is enabled
    public override void OnEnable() {
      // register system
      GameAPI.RegisterRocketSystem<TWRTargeterSystem>();
      
      // register part module
      GameAPI.RegisterPartModuleType<TWRTargeterModule>(TWRTargeterModule.extensionPartModuleName, instantiateFunc: null);

      // register flight UI panel
      GameAPI.RegisterPartModulePanel<TWRTargeterModule, TWRTargetModulePanel>();

      // try to add module to existing part schema
      AddModuleToExistingSchema();

      Log.Info($"{nameof(TWRTargeterExtension)} is succesfully enabled");
    }

    // OnStart is called after game load when everything is initialized
    // since OnEnable after load could not add module to schema, we need to it here
    public override void OnStart() {
      AddModuleToExistingSchema();
    }

    // IMPORTANT: always deregister everything you've registered
    public override void OnDisable() {
      var partSchemaCache = CacheStore.Get<RocketPartCache>();
      // remove from part schema first
      for (int i = 0; i < commandModuleDefindices.Length; i++) {
        var partSchema = partSchemaCache.Get(commandModuleDefindices[i]);
        GameAPI.RemovePartModuleFromPartSchema<TWRTargeterModule>(partSchema);
      }

      // deregister flight UI panel
      GameAPI.DeregisterPartModulePanel<TWRTargeterModule>();

      // deregister system
      GameAPI.DeregisterSystemType<TWRTargeterSystem>();

      // and finally module type
      GameAPI.DeregisterPartModuleType<TWRTargeterModule>(TWRTargeterModule.extensionPartModuleName);

      moduleWasAddedToSchema = false;

      Log.Info($"{nameof(TWRTargeterExtension)} is succesfully disabled");
    }

    private void AddModuleToExistingSchema() {
      // when extension is enabled on game load, part schema caches is not loaded yet, we need to check that here
      var partSchemaCache = CacheStore.Get<RocketPartCache>();
      if (partSchemaCache.isPreloaded && !moduleWasAddedToSchema) {
        // the way to get all schema items and then enumarate them applying different property filters
        // var allParts = partSchemaCache.All();

        // we use the same config for all command modules here, but you can use unique ones for each
        var config = new TWRTargeterConfig { consumption = 5, eceCoeff = 0.9f, responseTime = 1 };
        var state = new TWRTargeterState { isEnabled = false, targetTWR = 2 };
        for (int i = 0; i < commandModuleDefindices.Length; i++) {
          var partSchema = partSchemaCache.Get(commandModuleDefindices[i]);
          GameAPI.AddPartModuleToPartSchema<TWRTargeterModule, TWRTargeterConfig, TWRTargeterState>(partSchema, config, state);
        }

        moduleWasAddedToSchema = true;
      }
    }
  }
}
