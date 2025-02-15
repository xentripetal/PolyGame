# DI Alternatives 

`09/25/2024`

I don't know the best way to handle system registration/DI. I currently hacked together a solution with annotations
and codegen. But it doesn't feel clean. 

An option that would be nice is something more consistent with csharp. Where all the elements passed to the constructor
are automatically treated as system params/dependencies. I should look at how DryIOC resolves a constructor and see if I
can make something similar that handles it or even use an interceptor with DryIOC. 

`02/01/2025`

I went with code generators for now. It works off partial classes which makes some things easier but also breaks a lot of
the IDE features. I'm not sure if it's worth it. 

Also current autogen works off a single class being equivalent to a single system. Might be better to instead play into
the class nature of c# and instead have a single class be multiple systems. Each method can register any parameters it
cares about while any constructor parameters are shared between all methods. 

If I go that route, I should also consider changing the system generation to not rely on partial classes. Could go the route
a lot of other stuff does and have codegen generate registrations for all systems in some static db/class. Then during startup
use the registration if it exists, else try to do a runtime based generation with reflection (assuming not in a AOT environment). 




