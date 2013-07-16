using Microsoft.AspNet.SignalR;
using System;


namespace ABC.Infrastructure.Events
{
    public class Notifier
    {
        static readonly IPersistentConnectionContext Context = GlobalHost.ConnectionManager.GetConnectionContext<EventDispatcher>();

        public static void NotifyAll(NotificationType type, object obj)
        {
            var output = ConstructEvent(type, obj);
            Context.Connection.Broadcast(output);
        }

        protected static object ConstructEvent(NotificationType type, object obj)
        {
            var notevent = new { Event = type.ToString(), Data = obj };
            return notevent;
        }
    }
}