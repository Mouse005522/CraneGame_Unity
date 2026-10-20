namespace Kun.Tool
{
    public abstract class EasyMediatNotificationHandler<T, TRes> : EasyMediatNotificationHandler
    {
        public abstract TRes OnReceive (T data);
    }

    public abstract class EasyMediatNotificationHandler<T> : EasyMediatNotificationHandler
    {
        public abstract void OnReceive (T data);
    }

    public abstract class EasyMediatNotificationHandler
    {

    }
}
