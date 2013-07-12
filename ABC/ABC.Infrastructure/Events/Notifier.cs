using Microsoft.AspNet.SignalR;
using System;


namespace ABC.Infrastructure.Events
{
    public class Notifier
    {
        static readonly IPersistentConnectionContext Context = GlobalHost.ConnectionManager.GetConnectionContext<EventDispatcher>();

        public static void Subscribe( Guid connectionId, Guid groupId )
        {
            Context.Groups.Add( connectionId.ToString(), groupId.ToString() );

        }

        public static void Unsubscribe( Guid connectionId, Guid groupId )
        {
            Context.Groups.Remove( connectionId.ToString(), groupId.ToString() );
        }

        public static void Subscribe( Guid connectionId, string groupName )
        {
            Context.Groups.Add( connectionId.ToString(), groupName );
        }

        public static void Unsubscribe( Guid connectionId, string groupName )
        {
            Context.Groups.Remove( connectionId.ToString(), groupName );
        }

        public static void NotifyGroup( Guid groupId, NotificationType type, object obj )
        {
            Context.Groups.Send( groupId.ToString(), ConstructEvent( type, obj ) );
        }

        public static void NotifyGroup( string groupName, NotificationType type, object obj )
        {
            Context.Groups.Send( groupName, ConstructEvent( type, obj ) );
        }

        public static void NotifyAll( NotificationType type, object obj )
        {
            Context.Connection.Broadcast( ConstructEvent( type, obj ) );
        }

        public static object ConstructEvent( NotificationType type, object obj )
        {
            return new { Event = type.ToString(), Data = obj };
        }
    }
}