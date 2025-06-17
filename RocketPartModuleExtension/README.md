This example adds TWR targeter module, system, flight module panel and tooltip to the assembly shop. This module is also automatically added to all command pods. When command pod has this module you can set target TWR in the part panel during the flight:

![](Screenshots/module_panel.png)

When this module is enabled it will automatically change the throttle of all active engines to match the spacecraft TWR with the target one. It also will consume electricity and produce heat while working. 

Assembly tooltip will look like this:

![](Screenshots/module_tooltip.png)

## Concepts

### RocketPart

Base building block of any rocket. Consist of part properties and part modules.

### RocketPartModule

Adds functionality to the Rocket Part.

Responsibilites: 
- module state and config serialization / deserialization;
- adds itself to the appropriate system;
- provides the interface for displaying parameters in the part panel and tooltips;
- receives commands from part window UI, changes the state and routes it to the system.

### RocketSystem

This is where Rocket Part and Rocket Part Module functionality is implemented.

### RocketPartPanel

UI of RocketPartModule displayed when you click on the rocket part during the flight.
