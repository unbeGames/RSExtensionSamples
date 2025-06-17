using UnbeGames.API;
using UnbeGames.Model;
using UnbeGames.Services;
using UnbeGames.Support;

namespace PartModuleExtensionSample {
  public class TWRTargeterExtension : BaseExtension {
    // command module schema defindices
    private static readonly int[] commandModuleDefindices = new int[] { 1000, 1006 };

    private bool moduleWasAddedToSchema = false;

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

      // register assembly shop tooltip for module
      GameAPI.RegisterPartModuleTooltip<TWRTargeterModule, TWRTargeterTooltip>();

      // add module to existing part schema
      // this will fail on first OnEnable since all game data still not loaded, but will work on subsequent Enable
      TryAddModuleToExistingSchema();

      Log.Info($"{nameof(TWRTargeterExtension)} is succesfully enabled");
    }

    // OnStart is called after application fully loaded when everything is initialized    
    // since OnEnable right after app load could not add module to schema, we need to it here
    public override void OnStart() {
      TryAddModuleToExistingSchema();
    }

    // IMPORTANT: always deregister everything you've registered
    public override void OnDisable() {
      var partSchemaCache = CacheStore.Get<RocketPartCache>();
      // remove from part schema first
      for (int i = 0; i < commandModuleDefindices.Length; i++) {
        var partSchema = partSchemaCache.Get(commandModuleDefindices[i]);
        GameAPI.DeregisterAddPartModuleFromPartSchema<TWRTargeterModule>(partSchema);
      }

      moduleWasAddedToSchema = false;

      // deregisters tooltip for module
      GameAPI.DeregsiterPartModuleTooltip<TWRTargeterModule>();

      // deregister flight UI panel
      GameAPI.DeregisterPartModulePanel<TWRTargeterModule>();

      // deregister system
      GameAPI.DeregisterSystemType<TWRTargeterSystem>();

      // and finally module type
      GameAPI.DeregisterPartModuleType<TWRTargeterModule>(TWRTargeterModule.extensionPartModuleName);

      Log.Info($"{nameof(TWRTargeterExtension)} is succesfully disabled");
    }


    private void TryAddModuleToExistingSchema() {
      // when extension is enabled on game load, part schema caches is not loaded yet, we need to check that here
      var partSchemaCache = CacheStore.Get<RocketPartCache>();
      if (partSchemaCache.isPreloaded && !moduleWasAddedToSchema) {
        // it would not be added immediately, only after game load for games that supports mods
        // survival mode will not support mods and achievemnts at the same time in the future

        // the way to get all schema items and then enumarate them applying different property filters
        // var allParts = partSchemaCache.All();

        // we use the same config for all command modules here, but you can use unique ones for each
        var config = new TWRTargeterConfig { consumption = 5, eceCoeff = 0.9f, responseTime = 1 };
        var state = new TWRTargeterState { isEnabled = false, targetTWR = 2 };
        for (int i = 0; i < commandModuleDefindices.Length; i++) {
          var partSchema = partSchemaCache.Get(commandModuleDefindices[i]);
          GameAPI.RegisterAddPartModuleToPartSchema<TWRTargeterModule, TWRTargeterConfig, TWRTargeterState>(partSchema, config, state);
        }

        moduleWasAddedToSchema = true;
      }
    }
  }
}
