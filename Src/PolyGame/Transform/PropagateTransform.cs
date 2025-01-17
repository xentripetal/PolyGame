using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Transform;

public partial class PropagateTransform : AutoSystem
{
    public void Run(TQuery<Position2D, Rotation2D, Scale2D, GlobalTransform2D, GlobalTransform2D, (In<AllTerms>, W<Term3>, Cascade<Parent<Optional<Term4>>>, Cached)> q)
    {
        q.Query.Run(it =>
        {
            while (it.Next())
            {
                if (!it.Changed())
                {
                    it.Skip();
                    continue;
                }
                // TODO bundle views
                var pos = it.Field<Position2D>(0);
                var rot = it.Field<Rotation2D>(1);
                var scale = it.Field<Scale2D>(2);
                var transform = it.Field<GlobalTransform2D>(3);
                var parentTransform = it.Field<GlobalTransform2D>(4);
                var hasParent = it.IsSet(4);

                foreach (var i in it)
                {
                    transform[i].Value = new Affine2(pos[i].Value, rot[i].Radians, scale[i].Value);

                    if (hasParent)
                    {
                        var parent = parentTransform[i].Value;
                        Affine2.Multiply(parent, transform[i].Value, out parent);
                        transform[i].Value = parentTransform[i].Value * transform[i].Value;
                    }
                }
            }
        });
    }
}