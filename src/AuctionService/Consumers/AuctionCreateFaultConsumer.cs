using Contracts;
using MassTransit;

namespace AuctionService
{
    //handle for fault on auction created
    public class AuctionCreateFaultConsumer : IConsumer<Fault<AuctionCreated>>
    {
        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("----> Consuming faulty creation");

            //if exception found
            var exception = context.Message.Exceptions.First();
            // exceptionType is ArgumentException
            if (exception.ExceptionType == "System.ArgumentException")
            {
                context.Message.Message.Model="FooBar";
                await context.Publish(context.Message.Message);
            }
            else
            {
                Console.WriteLine("Not an argument exception - update error dashboard somewhere");
            }
        }
    }
}