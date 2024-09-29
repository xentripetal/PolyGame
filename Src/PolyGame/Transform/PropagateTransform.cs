using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Transform;

public partial class PropagateTransform : AutoSystem
{
    [ParamProvider("q")]
    public QueryParam BuildQuery(PolyWorld world) => Param.Of(world.QueryBuilder()
        .With<Position2D>().In().With<Rotation2D>().In().With<Scale2D>().In()
        .With<GlobalTransform2D>().Out()
        .With<GlobalTransform2D>().Optional().Parent().Cascade().In()
        .Cached().Build());


    [AutoRunMethod]
    public void Run(Query q)
    {
        q.Run(it => {
            while (it.Next())
            {
                if (!it.Changed())
                {
                    it.Skip();
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
