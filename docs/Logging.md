[[← back]](./README.md)

# Logging

Since Discore consists of many isolated internal tasks, a logging system was created to keep track of everything happening. While the Discore logger is not required to be used in an application, it can assist with tracking down bugs in Discore. If you suspect a bug is originating from Discore and not your application, these logs should be very useful in determining what is happening.

## Hooking Into The Logger
Every time a message is logged in Discore, the event `DiscoreLogger.OnLog` is fired.

This event is fired with a few details:
- The `DiscoreLogger` that fired the event (available through the `sender` parameter).
- The contents of the message that was logged.
- The severity of message.
- The `DateTime` the message was logged.

## Log Types
Discore breaks down message logs into four types:
- Debug (e.g. connection handshakes)
- Info (e.g. important events such as a gateway re-connection)
- Warning (e.g. unexpected non-fatal events)
- Error (e.g. fatal events such as abnormal socket disconnections)

## Filtering
Log messages can be disabled at a global level via `DiscoreLogger.MinimumLevel`. This defaults to `DiscoreLogLevel.Info`.

Verbose messages are filtered out by default for performance reasons. For example, a bot that has voice chat support serving hundreds of guilds does not need the verbose messages created from voice sockets connecting and disconnecting constantly.
