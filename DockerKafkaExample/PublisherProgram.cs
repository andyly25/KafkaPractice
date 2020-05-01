using System;
using System.Threading;
using Confluent.Kafka;

/*
 * To create Publisher you use ProducerBuilder class which expects PublishConfig
 * in the constructor
 * 
 * can build then run the code and see that message was delivered to
 * testTopic on partition 0 
 * offset for message is 3 (@3) when I ran it
 */

namespace publisher
{
    class KafkaPublishTest
    {
        // used to react to CTRL + C key press to exit the application
        static readonly AutoResetEvent _closing = new AutoResetEvent(false);

        // declare two class variables
        static IProducer<string, string> producer = null;
        // used in CreateProducer method where we use Build method
        // on ProduceBuilder to get IProducer instance
        static ProducerConfig producerConfig = null;

        static void Main(string[] args)
        {
            // call different methods
            CreateConfig();
            CreateProducer();
            //SendMessage("testTopic", "This is a test99");
            // let's edit sendmessage to send in a while loop
            int x = 0;
            while (x < 100)
            {
                SendMessage("testTopic", $"This test: {x}");
                x++;
                // this gives a small pause to see what happens when code is ran
                Thread.Sleep(200);
            }

            Console.WriteLine("Press Ctrl+C to exit");
            // some code to capture ctrl+c to exit app
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
            _closing.WaitOne();
        }

        static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit");
            _closing.Set();
        }

        // instantiate ProducerConfig and set BootstrapServer property to Kafka Broker
        static void CreateConfig()
        {
            producerConfig = new ProducerConfig
            {
                // use external listener port as we connect into Docker container from outside 
                // Docker internal network 
                BootstrapServers = "localhost:9092"
            };
        }

        static void CreateProducer()
        {
            var pb = new ProducerBuilder<string, string>(producerConfig);
            producer = pb.Build();
        }

        /* 
         * method to publish messages
         * two parameters
         * topics we want to send to
         * actual message we want to send
         */

        static async void SendMessage(string topic, string message)
        {
            // create instance of message class
            var msg = new Message<string, string>
            {
                // Key: when topic has multiple partitions, refers to value we want 
                // to used for Kafka to decide what partition to target
                // since no defined partitions, set value to null
                Key = null,
                //Value: message we want to send, set to incoming message param
                Value = message
            };

            // to publish message we call ProduceAsync method
            // returns instance of DeliveryReport class
            var delRep = await producer.ProduceAsync(topic, msg);
            var topicOffset = delRep.TopicPartitionOffset;

            Console.WriteLine($"Delivered '{delRep.Value}' to: {topicOffset}");
        }
    }
}
