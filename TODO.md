# TODO

- Fill in more tests.

  - Fail to Join a lobby due to game does not exist
  - Fail to Join a lobby due to lobby is full
  - Close a lobby
  - Create a session with a list of players
  - Join a session
  - End a session

- TestClient can just become 'client'.
  The transport mechanism can just be hidden behind the IGameServer interface.

- MAYBE - Refactor so that the GameServer implementation is separate from the protocol.
  I think I would like all the various classes to be able to use the protocol...
  e.g. if an error goes wrong, the errors should be in terms of the protocol.
