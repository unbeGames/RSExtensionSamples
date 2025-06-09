
## Concepts

### RocketPart

Base building block of any rocket. Consist of part properties and part modules

### RocketPartModule

Adds functionality to the Rocket Part

Responsibilites: 
- module state and config serialization / deserialization;
- tracks to what systems it belongs;
- provides the interface for displaying parameters in tooltips;
- receives commands from part window UI, change the state and routes it to the system.

### RocketSystem

This is where Rocket Part and Rocket Part Module functionality is implemented.
