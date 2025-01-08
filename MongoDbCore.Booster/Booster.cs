namespace MongoDbCore.Booster;

public class Booster<TDbContext, TEntity>
    where TDbContext : MongoDbContext
    where TEntity : BaseEntity
{
    private readonly object _lock = new();
    private List<TEntity> _list = new();
    private Collection<TEntity> _collection = null!;

    public void Initialize(TDbContext dbContext)
    {
        _collection = new Collection<TEntity>(dbContext);
        lock (_lock)
        {
            _list = _collection.ToList();
        }
    }

    public List<TEntity> GetAll()
    {
        lock (_lock)
        {
            return _list.ToList(); // Return a copy to avoid thread contention
        }
    }

    public TEntity? Get(string id)
    {
        lock (_lock)
        {
            return _list.FirstOrDefault(x => x.Id == id);
        }
    }

    public TEntity Add(TEntity entity)
    {
        lock (_lock)
        {
            entity = _collection.Add(entity);
            _list.Add(entity);
            return entity;
        }
    }

    public TEntity Update(TEntity entity)
    {
        lock (_lock)
        {
            var updatedEntity = _collection.Update(entity);
            var index = _list.FindIndex(x => x.Id == entity.Id);
            if (index >= 0)
            {
                _list[index] = updatedEntity;
            }
            return updatedEntity;
        }
    }

    public void Delete(string id)
    {
        lock (_lock)
        {
            var item = Get(id);
            if (item != null)
            {
                _collection.Delete(item);
                _list.Remove(item);
            }
        }
    }
}
