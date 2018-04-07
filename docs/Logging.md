[[← back]](./README.md)

# Logging

Since Discore consists of many isolated internal tasks, a logging system was created to keep track of everything happening. While the Discore logger is not required to be used in an application, it may assist with:
- **Tracking down bugs occurring in Discore**: If you suspect a bug is originating from Discore and not your application, these logs should be very useful in determining what is happening.
- **Extending your own logging system**: It may be useful to include events occurring internally in Discore to help keep track of what your application is doing.

## Hooking Into The Logger
Every time a message is logged in Discore, the event `DiscoreLogger.OnLog` is fired.

This event is fired with a few details:
- The `DiscoreLogger` that fired the event (available through the `sender` parameter).
- The message that was logged.
- The type of message.
- The `DateTime` the message was logged.

## Log Types
Discore breaks down message logs into four types:
- Verbose (e.g. connection handshakes)
- Info (e.g. important events such as a gateway re-connection)
- Warning (e.g. unexpected non-fatal events)
- Error (e.g. fatal events such as abnormal socket disconnections)

## Filtering
Log messages can be disabled at a global level via `DiscoreLogger.MinimumLevel`. Unless Discore is compiled with the DEBUG preprocessor, this defaults to `DiscoreLogType.Info` (otherwise `DiscoreLogType.Verbose`). Verbose messages are filtered out by default for performance reasons. For example, a bot that has voice chat support serving hundreds of guilds does not need the verbose messages created from voice sockets connecting and disconnecting constantly.

## Extending The Logger
If you wish to use the Discore logger as your application's logger, a `DiscoreLogger` can be created with a prefix. Each logger is meant for a certain area of the application, with the prefix being a short description of that area.

This example creates a new logger to be used in the message handling procedure of an application:
```csharp
DiscoreLogger customLogger = new DiscoreLogger("Message Handling");
customLogger.LogInfo("Hello!"); // This would generate the message "[Message Handling] Hello!"
```


