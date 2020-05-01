using System;
using System.Threading;
using Confluent.Kafka;

namespace consumer
{
    /*
     * similar to producer with a configuration and a builder
     */
    class KafkaConsumer
    {
        static CancellationTokenSource cts = new CancellationTokenSource();
        static ConsumerConfig consumerConfig = null;
        static void Main(string[] args)
        {
            CreateConfig();
            CreateConsumerAndConsume();
        }

        static void CreateConfig()
        {
            consumerConfig = new ConsumerConfig
            {
                // list of brokers to conect to
                BootstrapServers = "localhost:9092",
                // name of consumer group you connect to the broker
                GroupId = "test-group",
                // tells Kafka where to start reading offsets from
                // dependant on this value to see a list of messages or consumer
                // sits and wait for messages to arrive
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        static void CreateConsumerAndConsume()
        {
            // create ConsumerBuilder based on ConsumerConfig instance
            var cb = new ConsumerBuilder<string, string>(consumerConfig);

            Console.WriteLine("Press ctrl+c to exit");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

            using(var consumer = cb.Build() )
            {
                // subscribe to the topic(s) we are interested in
                consumer.Subscribe("testTopic");

                try
                {
                    // consume in a while loop
                    while(!cts.IsCancellationRequested)
                    {
                        // returns a ConsumeResult instant used to print info to console
                        var cr = consumer.Consume(cts.Token);
                        var offset = cr.TopicPartitionOffset;
                        Console.WriteLine($"Message '{cr.Value}' at: '{offset}'.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    consumer.Close();
                }
            }
        }

        static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            Console.WriteLine("In OnExit");
            cts.Cancel();
        }
    }
}
