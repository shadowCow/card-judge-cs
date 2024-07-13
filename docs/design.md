# Design

This is a private-first turn-based and simultaneous-round-based multiplayer game system.

# Terminology

## Private-first

This system is primarily intended to be used by people who know each other outside the context of this system and want to play games virtually with each other.

A `Game Session` can be created by any user, but a `Game Session` can only be joined by invitation. A `Game Session` is not discoverable; the `Game Session Host` must convey the invitation to each user that the `Game Session Host` wishes to invite. A user who wishes to join a `Game Session` must provide the appropriate secret to be admitted.

Other mechanisms for creating and joining games may be supported, but the focus is on the private, invitation-only, gaming experience.

## Turn-based

In a `Turn-based` game, players perform actions in some defined order. That is, if it is currently Alice's turn, Bob may not perform any action until Alice completes her turn.

## Simultaneous-round-based

In a `Simultaneous-round-based` game, players all take their turns at the same time. Players may perform their legal actions without waiting for other players to perform theirs, until the round is over.

## Game

A `Game` is the set of rules that defines the gaming experience, from which an individual `Game Session` can be created. Chess is a `Game`; when Alice and Bob play Chess together, they are not playing a `Game`, but rather a `Game Session`, using the rules of the Chess `Game`.

The rules include:

- The virtual world definition. E.g. in a board game this may include the board and its layout, pieces, player tableaus, player card hands. In a card game like poker, this would include the pot, the shared card table, individual player card tableaus and hands. In a 3d world, this would be the 3d world.
- The `Game State`. The current positions, values, etc of any aspect of the game in the virtual world.
- The set of `Game Actions`s. The legal actions that a player can take on his or her turn.
- The `Game Transition Function`. When a player performs a `Game Action` in a particular `Game State`, the `Game Transition Function` determines the next state of the `Game Session`.

## Game Session

A `Game Session` is the active playing of a `Game` by a set of players.

Players choose which `Game` to play, and create a `Game Session` to play according to the rules of the chosen `Game`.

# System Architecture

A `Game Server` tracks all game sessions and their state. It is the source of truth, and is a trusted system component.

A `Game Client` connects to a `Game Server`. Many `Game Client`s may connect to a single `Game Server`. Multiple `Game Client`s that want to participate in the same `Game Session` must connect to the same `Game Server`.

Each `Game Client` acts on behalf of a single player, who is identified by their `Player Id`.

The system is `Event Driven`. `Game State` is `Event Sourced` - we can replay the history of `Game Action`s to arrive at a particular `Game State`. A `Game Server` publishes events to the `Game Client`s. A `Game Client` applies those events to its local copy of the game rules and `Game State` to get the current `Game State`. Since a `Game Client` is not trusted, it sends `Command`s to the server which may be rejected to prevent illegal actions by a bad actor.

> Note: In an Event Driven system, an `Event` represents a historical record. It has already happened, and cannot be undone. A `Command` represents a request to do something. It may be denied. A `Command` must always be responded to with a response which can either be a success or a failure.

The communication between a `Game Client` and a `Game Server` is bi-directional. Some events are produced by a `Game Client`, and consumed by a `Game Server`. Some events are produced by the `Game Server`, and are consumed by the connected `Game Client`s. The details of the communication channel implementation are not specified here. They may be implemented via a client-side pull system, such as http requests, or via a bi-directional push system, such as websocket. The implementation may choose whichever is most appropriate for its performance goals, and may support more than one mechanism.
