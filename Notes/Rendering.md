# Rendering

## ECS Viable Rendering
`06-20-2024`

I'm not sure how to handle intelligent rendering in an ECS paradigm. We can't just have each renderable work as an
interface.

> Or can we? Extract could transform components into IRenderable and then we could have a system that renders all the interfaces
> Nah the performance would be awful


We need to be able to sort, layer, and apply affects for multiple cameras.

Maybe I just make a dedicated renderer for each thing I care about and say if you want to add a new primitive make your
own renderer in the stack.

For now I think I'm going to state that everything is in chunks and do optimized sorting that way. Maybe I don't have that
done via relationships but instead have a system that buckets into chunks?