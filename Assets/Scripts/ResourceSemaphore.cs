using System;
using System.Collections.Generic;

public abstract class ResourceSemaphore<T> where T : ResourceSemaphore<T>
{
    private List<(Action<T> action, int resources)> _waiting;

    private int _resourceCount;

    public int ResourceCount
    {
        get => _resourceCount;
        set => _resourceCount = value;
    }

    private int _granted;

    public ResourceSemaphore(int resourceCount)
    {
        ResourceCount = resourceCount;
        _granted = 0;
        _waiting = new List<(Action<T> action, int resources)>();
    }

    public void WaitOne(Action<T> onGetResource)
    {
        Wait(onGetResource, 1);
    }

    public void Wait(Action<T> onGetResource, int required)
    {
        if (_resourceCount - _granted >= required)
        {
            _granted += required;
            onGetResource?.Invoke(this as T);
        }
        else
        {
            _waiting.Add((onGetResource, required));
        }
    }

    public void Release(int howMany)
    {
        if(_granted - howMany < 0)
            throw new ArgumentException($"Over release of semaphore {GetType().Name}");
        _granted -= howMany;
        if (_isInvoking)
        {
            _reInvoke = true;
        }
        else
        {
            _isInvoking = true;
            InvokePending();
            _isInvoking = false;
        }
    }

    private bool _isInvoking;
    private bool _reInvoke;

    private void InvokePending()
    {
        for (int i = _waiting.Count - 1; i >= 0; i--)
        {
            if (_waiting[i].resources <= _resourceCount - _granted)
            {
                _granted += _waiting[i].resources;
                _waiting[i].action?.Invoke(this as T);
                _waiting.RemoveAt(i);
                if (_reInvoke)
                {
                    break;
                }
            }
        }

        if (_reInvoke)
        {
            _reInvoke = false;
            InvokePending();
        }
    }
}

public class AnimationControlSemaphore : ResourceSemaphore<AnimationControlSemaphore>
{
    public AnimationControlSemaphore(int resources) : base(resources) {}   
}
