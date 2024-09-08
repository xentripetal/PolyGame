<h1 align="center">PolyGame - A ECS MonoGame Framework</h1>

# What is PolyGame?
PolyGame is a MonoGame framework that uses the Entity-Component-System (ECS) architecture. It is designed to be a simple
and easy-to-use framework for creating games in MonoGame. It is inspired by Bevy and its modular approach to game development
while using a lot of the features present in Nez.

# References
- [Bevy](https://bevyengine.org/) - A rust ECS game engine, used heavily for design inspiration 
- [Nez](https://github.com/prime31/Nez/) - An existing MonoGame helper library, a majority of the systems are ports of Nez aligned with an ECS approach
- [FlecsNet](https://github.com/BeanCheeseBurrito/Flecs.NET/) - A C# port of the Flecs ECS library. For now I'm using 
this but I have a bad habit of switching ECS libraries every month. Potentially looking at switching to 
[TinyECS](https://github.com/andreakarasho/TinyEcs)
- [HexaEngine](https://github.com/HexaEngine/HexaEngine) - Another C# game engine with great ImGui integration. I'm using this as a reference for the Editor/ImGUI
