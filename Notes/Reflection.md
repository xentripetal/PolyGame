# Reflection

`08/04/2024`
While Flecs has builtin reflection capabilities for unmanaged types, it does not support managed (Need to confirm).
It also doesn't support custom annotations for controlling serialization/deserialization. I think long term I should
look at implementing my own reflection and annotation systme based on Dotnet reflection and make sure it can be translated
into Flecs reflection. 

Then the debug ui can use the custom reflection and have nice displays similar to Odin.


