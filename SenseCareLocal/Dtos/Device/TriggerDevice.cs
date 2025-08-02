public class TriggerDevice
{
    private readonly Dictionary<int, bool> _triggers = new();

    public void SetTrigger(int id) => _triggers[id] = true;

    public bool ConsumeTrigger(int id)
    {
        if (_triggers.TryGetValue(id, out var value) && value)
        {
            _triggers[id] = false;
            return true;
        }
        return false;
    }
<<<<<<< HEAD
}

=======
}
>>>>>>> 4d927fa154dfc7c4f7e63f1651371c190af17311
