# VICE Binary Monitor Bridge for .NET

Implements a bridge for communication with [VICE](https://vice-emu.sourceforge.io/) [binary monitor](https://vice-emu.sourceforge.io/vice_13.html#SEC281) in .NET 5.

Tested(limited) and build against VICE 3.5.

## Quick start

Start VICE with *-binarymonitor* argument so it listens to default port 6502.

First step is to register types with .NET's standard `Microsoft.Extensions.DependencyInjection.IServiceCollection` by calling extension method `Righthand.ViceMonitor.Bridge.AddEngineServices`, like:

```csharp
var collection = new ServiceCollection();
collection.AddEngineServices();
```

After IoC is setup, `IViceBridge` has to be resolved through IoC. Optionally `ConnectionChanged` even handler can be used to track connection status (also available through `IViceBridge.IsConnected` property) and `ViceResponse` event handler can be used to receive unbound responses from VICE.

```csharp
bridge.ConnectedChanged += Bridge_ConnectedChanged;
bridge.Start();
```

After `IViceBridge.IsConnected` property becomes `true`, command can be sent to VICE and responses will flow back. Here is a ping command:

```csharp
var ping = bridge.EnqueueCommand(new PingCommand());
var response = await ping.Response;
```

## Playground sample

Playground sample is a console application that is used for testing and for sample purposes.