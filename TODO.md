# TODO

- Update the tests to include the concept of one or more 'client' instances. The system is a server and multiple clients, and is event driven and agnostic to the details of the communication channel. The tests should reflect all of that. The tests need...
  - A way to create a 'client'
  - A way to create a 'server'
  - A way for a 'client' to send messages to a 'server'
  - A way to validate that a 'client' has received a message from a 'server'
  - ?? - A way to validate a client state
