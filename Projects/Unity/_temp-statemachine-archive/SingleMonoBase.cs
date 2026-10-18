using UnityEngine;

//单例模式限定器
public class SingleMonoBase<T> : MonoBehaviour where T : SingleMonoBase<T>
{
    public static T Instance { get; private set; }
    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"{name}不符合单例模式");
        }
        Instance = (T)this;
    }
    protected virtual void OnDestroy()
    {
        Instance = null;
    }
}
