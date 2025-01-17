using PolyECS.Systems;

namespace PolyECS.Scheduling.Configs;

public abstract class SystemConfigAttribute : Attribute
{
    public abstract IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs);
}

public abstract class EnumSetReferenceAttribute<T> : SystemConfigAttribute where T : struct, Enum
{
    protected IIntoSystemSet ReferenceSet;

    public EnumSetReferenceAttribute(T set) => ReferenceSet = new EnumSystemSet<T>(set);
}

public abstract class GenericSetReferenceAttribute<T> : SystemConfigAttribute where T : ISystem
{
    protected IIntoSystemSet ReferenceSet;

    public GenericSetReferenceAttribute() => ReferenceSet = new SystemTypeSet<T>();
}

public class BeforeSystemAttribute<T> : GenericSetReferenceAttribute<T> where T : ISystem
{
    public override IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs) => configs.Before(ReferenceSet);
}

public class BeforeAttribute<TEnum> : EnumSetReferenceAttribute<TEnum> where TEnum : struct, Enum
{
    public BeforeAttribute(TEnum set) : base(set) { }

    public override IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs) => configs.Before(ReferenceSet);
}

public class AfterSystemAttribute<T> : GenericSetReferenceAttribute<T> where T : ISystem
{
    public override IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs) => configs.After(ReferenceSet);
}

public class AfterAttribute<TEnum> : EnumSetReferenceAttribute<TEnum> where TEnum : struct, Enum
{
    public AfterAttribute(TEnum set) : base(set) { }

    public override IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs) => configs.After(ReferenceSet);
}

public class InSetAttribute<T>(T set) : EnumSetReferenceAttribute<T>(set)
    where T : struct, Enum
{
    public override IIntoNodeConfigs<ISystem> Apply(IIntoNodeConfigs<ISystem> configs) => configs.InSet(ReferenceSet);
}
