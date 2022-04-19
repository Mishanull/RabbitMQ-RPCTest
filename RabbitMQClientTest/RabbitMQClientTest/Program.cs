// See https://aka.ms/new-console-template for more information

using RabbitMQClientTest;
    


            
do
{
    var rpcClient = new RpcClient("q.test");
    Console.WriteLine(" [x] Requesting...");
    var response = rpcClient.Call("Hello to server!");

    Console.WriteLine(" [.] Got '{0}'", response);
    rpcClient.Close();

    Console.WriteLine("Do it again? Y/n");
} while (Char.ToUpper(Console.ReadKey().KeyChar) == 'Y');