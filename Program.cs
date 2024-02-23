using System;
using System.Text;
using NATS.Client;


string natsUrl = Environment.GetEnvironmentVariable("NATS_URL");
if (natsUrl == null)
{
	natsUrl = "nats://127.0.0.1:32000";
}


Options opts = ConnectionFactory.GetDefaultOptions();
opts.Url = natsUrl;


ConnectionFactory cf = new ConnectionFactory();
IConnection c = cf.CreateConnection(opts);



EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
{
	Msg m = args.Message;
	string text = Encoding.UTF8.GetString(m.Data);
	Console.WriteLine($"Async handler received the message '{text}' from subject '{m.Subject}'");
};


c.Publish("greet.joe", Encoding.UTF8.GetBytes("hello joe 1"));


IAsyncSubscription subAsync = c.SubscribeAsync("greet.*", handler);


ISyncSubscription subSync = c.SubscribeSync("greet.pam");


c.Publish("greet.pam", Encoding.UTF8.GetBytes("hello pam 1"));
c.Publish("greet.joe", Encoding.UTF8.GetBytes("hello joe 2"));


try
{
	Msg m = subSync.NextMessage(1000);
	string text = Encoding.UTF8.GetString(m.Data);
	Console.WriteLine($"Sync subscription received the message '{text}' from subject '{m.Subject}'");
	m = subSync.NextMessage(100);
}

catch (NATSTimeoutException)
{
	Console.WriteLine($"Sync subscription no messages currently available");
}


c.Drain();