# Panics

## Panics In Flecs 
`06-03-2024`

When Flecs throws an exception or an assert it crashes the entire program and dumps an unreadable error message. This is
quite annoying, especially during development as Flecs has a lot of gotchas. There is no solution to this.

Users just need to learn all the gotchas of Flecs and hope they don't run into them. Or I can look at a C# native library
so it uses exceptions that are at least readable and catchable. Unfortunately nothing compares to Flecs in terms of
relationships and graph traversals. To my knowledge the only other library with first class relationship support is TinyECS,
which is great, but very unpolished in its current state.
