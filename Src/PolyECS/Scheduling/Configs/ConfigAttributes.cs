using PolyECS.Systems;

namespace PolyECS.Scheduling.Configs;

public abstract class SystemConfigAttribute : Attribute
{
    public abstract IIntoSystemConfigs Apply(IIntoSystemConfigs configs);
}

public abstract class EnumSetReferenceAttribute<T> : SystemConfigAttribute where T : struct, Enum
{
    public EnumSetReferenceAttribute(T set)
    {
        ReferenceSet = new EnumSystemSet<T>(set);
    }

    protected IIntoSystemSet ReferenceSet;
}

public abstract class GenericSetReferenceAttribute<T> : SystemConfigAttribute where T : RunSystem
{
    public GenericSetReferenceAttribute()
    {
        ReferenceSet = new SystemTypeSet<T>();
    }

    protected IIntoSystemSet ReferenceSet;
}

public class BeforeSystemAttribute<T> : GenericSetReferenceAttribute<T> where T : RunSystem
{
    public override IIntoSystemConfigs Apply(IIntoSystemConfigs configs)
    {
        return configs.Before(ReferenceSet);
    }
}

public class BeforeAttribute<TEnum> : EnumSetReferenceAttribute<TEnum> where TEnum : struct, Enum
{
    public override IIntoSystemConfigs Apply(IIntoSystemConfigs configs)
    {
        return configs.Before(ReferenceSet);
    }

    public BeforeAttribute(TEnum set) : base(set) { }
}

public class AfterSystemAttribute<T> : GenericSetReferenceAttribute<T> where T : RunSystem
{
    public override IIntoSystemConfigs Apply(IIntoSystemConfigs configs)
    {
        return configs.After(ReferenceSet);
    }
}

public class AfterAttribute<TEnum> : EnumSetReferenceAttribute<TEnum> where TEnum : struct, Enum
{
    public override IIntoSystemConfigs Apply(IIntoSystemConfigs configs)
    {
        return configs.After(ReferenceSet);
    }

    public AfterAttribute(TEnum set) : base(set) { }
}

public class InSetAttribute<T>(T set) : EnumSetReferenceAttribute<T>(set)
    where T : struct, Enum
{
    public override IIntoSystemConfigs Apply(IIntoSystemConfigs configs)
    {
        return configs.InSet(ReferenceSet);
    }
}
