using System.Collections.Generic;
using PolyECS.Systems;

namespace PolyECS.Tests.Systems;

public class AccessTest
{
    [Fact]
    public void ReadAllAccessConflicts()
    {
        // read all / single write
        var a = new Access<int>().AddWrite(0);
        var b = new Access<int>().ReadAll();
        a.IsCompatible(b).Should().BeFalse();
        b.IsCompatible(a).Should().BeFalse();

        // read all / read all
        a = new Access<int>().ReadAll();
        a.IsCompatible(b).Should().BeTrue();
        b.IsCompatible(a).Should().BeTrue();
    }

    [Fact]
    public void AccessGetConflicts()
    {
        var a = new Access<int>().AddRead(0).AddRead(1);
        var b = new Access<int>().AddRead(0).AddWrite(1);
        a.GetConflicts(b).Should().BeEquivalentTo([1]);

        var accessC = new Access<int>().AddWrite(0).AddWrite(1);
        a.GetConflicts(accessC).Should().BeEquivalentTo([0, 1]);
        b.GetConflicts(accessC).Should().BeEquivalentTo([0, 1]);

        var accessD = new Access<int>().AddRead(0);
        accessD.GetConflicts(a).Should().BeEmpty();
        accessD.GetConflicts(b).Should().BeEmpty();
        accessD.GetConflicts(accessC).Should().BeEquivalentTo([0]);
    }

    [Fact]
    public void FilteredCombinedAccess()
    {
        var a = new FilteredAccessSet<int>();
        a.AddUnfilteredRead(1);
        var b = new FilteredAccess<int>();
        b.AddWrite(1);
        a.GetConflictsSingle(b).Should().BeEquivalentTo([1]);
    }

    [Fact]
    public void FilteredAccessExtend()
    {
        var a = new FilteredAccess<int>()
            .AddRead(0)
            .AddRead(1)
            .AndWith(2);

        var b = new FilteredAccess<int>()
            .AddRead(0)
            .AddWrite(3)
            .AndWithout(4);

        a.Extend(b);

        var expected = new FilteredAccess<int>()
            .AddRead(0)
            .AddRead(1)
            .AndWith(2)
            .AddWrite(3)
            .AndWithout(4);

        a.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void FilteredAccessExtendOr()
    {
        // Exclusive access to `(&mut A, &mut B)`.
        var a = new FilteredAccess<int>()
            .AddWrite(0)
            .AddWrite(1);

        // Filter by With<C>
        var b = new FilteredAccess<int>()
            .AndWith(2);

        // Filter by (With<D>, Without<E>)
        var c = new FilteredAccess<int>()
            .AndWith(3)
            .AndWithout(4);

        // Turns b into Or<With<C>, (With<D>, Without<E>)
        b.AppendOr(c);
        // Applies the filters to the initial query, which crresponds to the FilteredAccess' repr of Query(ref A, ref B), Or<With<c>, (With<D>, Without<E>))
        a.Extend(b);

        var expected = new FilteredAccess<int>().AddWrite(0).AddWrite(1);
        expected.FilterSets = new List<AccessFilters<int>>
        {
            new ()
            {
                With = new HashSet<int>([0, 1, 2])
            },
            new ()
            {
                With = new HashSet<int>([0, 1, 3]),
                Without = new HashSet<int>([4])
            }
        };

        a.Should().BeEquivalentTo(expected);
    }
}
