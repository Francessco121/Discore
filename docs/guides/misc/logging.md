# Logging
Since Discore consists of many internal tasks, a logging system was created to keep track of everything happening and assist in debugging. While the [Discore logger](xref:Discore.DiscoreLogger) is not required to be used in an application, it can assist with tracking down bugs in Discore. If you suspect a bug is originating from Discore and not your application, these logs should be very useful in determining what is happening.

Additionally, if you are having a difficult time getting your application connected to Discord, these logs may point you in the right direction.

## Hooking Into The Logger
Every time a message is logged in Discore, the event [`DiscoreLogger.OnLog`](xref:Discore.DiscoreLogger.OnLog) is fired.

This event is fired with a few details:
- The [`DiscoreLogger`](xref:Discore.DiscoreLogger) that fired the event (available through the `sender` parameter).
- The contents of the message that was logged.
- The severity of message.
- The `DateTime` the message was logged.

## Log Types
Discore breaks down message logs into four types:
- Debug (e.g. connection handshakes)
- Info (e.g. important events such as a Gateway reconnection)
- Warning (e.g. unexpected non-fatal events)
- Error (e.g. fatal events such as abnormal socket disconnections)

## Filtering
Log message types can be disabled at a global level via [`DiscoreLogger.MinimumLevel`](xref:Discore.DiscoreLogger.MinimumLevel). This defaults to [`DiscoreLogLevel.Info`](xref:Discore.DiscoreLogLevel.Info).

Verbose messages are filtered out by default for performance reasons and generally should not be enabled in a release build. Voice connections in particular generate a decent amount of verbose log messages.
