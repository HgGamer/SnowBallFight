// THIS FILE IS AUTOMATICALLY GENERATED BY SPACETIMEDB. EDITS TO THIS FILE
// WILL NOT BE SAVED. MODIFY TABLES IN YOUR MODULE SOURCE CODE INSTEAD.

#nullable enable

using System;
using SpacetimeDB.ClientApi;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SpacetimeDB.Types
{
    public sealed partial class RemoteReducers : RemoteBase
    {
        public delegate void ThrowSnowBallHandler(ReducerEventContext ctx, uint playerId, DbVector2 position);
        public event ThrowSnowBallHandler? OnThrowSnowBall;

        public void ThrowSnowBall(uint playerId, DbVector2 position)
        {
            conn.InternalCallReducer(new Reducer.ThrowSnowBall(playerId, position), this.SetCallReducerFlags.ThrowSnowBallFlags);
        }

        public bool InvokeThrowSnowBall(ReducerEventContext ctx, Reducer.ThrowSnowBall args)
        {
            if (OnThrowSnowBall == null) return false;
            OnThrowSnowBall(
                ctx,
                args.PlayerId,
                args.Position
            );
            return true;
        }
    }

    public abstract partial class Reducer
    {
        [SpacetimeDB.Type]
        [DataContract]
        public sealed partial class ThrowSnowBall : Reducer, IReducerArgs
        {
            [DataMember(Name = "player_id")]
            public uint PlayerId;
            [DataMember(Name = "position")]
            public DbVector2 Position;

            public ThrowSnowBall(
                uint PlayerId,
                DbVector2 Position
            )
            {
                this.PlayerId = PlayerId;
                this.Position = Position;
            }

            public ThrowSnowBall()
            {
                this.Position = new();
            }

            string IReducerArgs.ReducerName => "ThrowSnowBall";
        }
    }

    public sealed partial class SetReducerFlags
    {
        internal CallReducerFlags ThrowSnowBallFlags;
        public void ThrowSnowBall(CallReducerFlags flags) => ThrowSnowBallFlags = flags;
    }
}
