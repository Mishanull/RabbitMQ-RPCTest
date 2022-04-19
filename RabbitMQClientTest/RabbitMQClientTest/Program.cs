// See https://aka.ms/new-console-template for more information

using RabbitMQClientTest;
    


            
do
{
    var rpcClient = new RpcClient("prison.users");
    Console.WriteLine(" [x] Requesting message");
    var response = rpcClient.Call("message");

    Console.WriteLine(" [.] Got '{0}'", response);
    rpcClient.Close();

    Console.WriteLine("Do it again? Y/n");
} while (Char.ToUpper(Console.ReadKey().KeyChar) == 'Y');