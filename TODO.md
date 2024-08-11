# TODO

- Finish implementing ServiceTests for new Room-based model.

- Does 'IGameClient' need to subscribable?
  So that UI can update in response to events?

- Make Validate return a Task or whatever so that we can 'await' it?

- Make tic-tac-toe rules.

  - Rename existing 'Game' model... GameDefinition? GameMetadata?
  - Need Game interface in Domain
  - Implement Game interface in 'Games' for TicTacToe

- Split Tests into separate files... by Action?

- TestClient can just become 'client'.
  The transport mechanism can just be hidden behind the IGameServer interface.

- MAYBE - Client actions should be a single method that takes Command objects,
  instead of a method per action.

- MAYBE - Refactor so that the GameServer implementation is separate from the protocol.
  I think I would like all the various classes to be able to use the protocol...
  e.g. if an error goes wrong, the errors should be in terms of the protocol.
