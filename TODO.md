# TODO

- Make Validate return a Task or whatever so that we can 'await' it.

- Design lobby and session flow and relationship.
  Is it 1 to 1? So a lobby is like 'phase 1' and the session is 'phase 2'?
  When a session ends, is everyone dropped out to 'home'?

  Is it 1 to many? So the lobby is like a persistent waiting area,
  And at the end of a session, players are returned to the lobby?
  And they can start a new session from there?

  The 1 to many would be convenient for playing multiple game sessions,
  because you don't have to re-invite everyone to a new lobby.

  Another question is...
  Is a lobby tied to a particular game, or can you change which game to play?
  Games have min/max player numbers, which would tie into how many ppl can join a lobby.
  So you could...

  - Tie a lobby to a game, and max players derives from game
  - Construct a lobby with an explicit player max, which then determines the set of games you can choose from.
  - Lobbies have no player max? (aka a high player max).
    You can choose any game, but if you have more players than the game supports,
    you have to choose which players are in the game.

- Fill in more tests.

  - Make game move
  - End a session

- Make tic-tac-toe rules.

- Split Tests into separate files... by Action?

- TestClient can just become 'client'.
  The transport mechanism can just be hidden behind the IGameServer interface.

- MAYBE - Client actions should be a single method that takes Command objects,
  instead of a method per action.

- MAYBE - Refactor so that the GameServer implementation is separate from the protocol.
  I think I would like all the various classes to be able to use the protocol...
  e.g. if an error goes wrong, the errors should be in terms of the protocol.
