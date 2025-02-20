using Flecs.NET.Core;
using PolyECS;
using PolyECS.Systems;

namespace PolyGame.Graphics.Sprites;

public partial class PropagateZIndex : AutoSystem
{
    public void Run(TQuery<ZIndex, GlobalZIndex, GlobalZIndex, (In<Term0>, Out<Term1>, Cascade<Parent<Optional<In<Term2>>>>, Cached)> q)
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
                var index = it.Field<ZIndex>(0);
                var globalIndex = it.Field<GlobalZIndex>(1);
                var parentIndex = it.Field<GlobalZIndex>(2);
                var hasParent = it.IsSet(2);

                foreach (var i in it)
                {
                    globalIndex[i].Value = index[i].Value;

                    if (hasParent && index[i].Relative)
                    {
                        globalIndex[i].Value = parentIndex[i].Value + index[i].Value;
                    }
                }
            }
        });
    }
}
