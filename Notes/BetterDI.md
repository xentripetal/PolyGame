# DI Alternatives 

09/25/2025

I don't know the best way to handle system registration/DI. I currently hacked together a solution with annotations
and codegen. But it doesn't feel clean. 

An option that would be nice is something more consistent with csharp. Where all the elements passed to the constructor
are automatically treated as system params/dependencies. I should look at how DryIOC resolves a constructor and see if I
can make something similar that handles it or even use an interceptor with DryIOC. 