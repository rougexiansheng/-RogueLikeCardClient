
public abstract class OriginDefineBase<TTable>
{
    public int id;
    public abstract TTable ParseData();

    public virtual int GetGroupId()
    {
        return default;
    }
}
