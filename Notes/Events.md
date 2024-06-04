# Events 

## On Flecs Event Bus 
`06-03-2024`
 
Flecs event system isn't great as it has to be tied to entities of an archetype and won't work with routines/scheduling.

I should see if theres a simple implementation of EventReader/EventWriter like bevy in C#
- Might be able to have it also hook up to Flecs events so it autopopulates the event system?
  - Nah cause we have to link it to an archetype or we would miss information. 
- What about inverse? Have it populate the flecs event system?
  - No probably not but we could easily make an adapter routine that does this for specific use cases

