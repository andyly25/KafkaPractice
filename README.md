# setting up Confluent with docker
Note, not full project code is posted here, just all of the main files that I had made changes in. Also note this is assuming you are running code mainly on Windows with Command Prompt

## installing docker desktop
- make sure you have latest Windows 10 update (can take hours)
- if not windows 10 Pro or enterprise have to cheat a little bit
- check if you have everything needed on windows 10 to have it working (my windows 10 home did)
- follow instructions here
	- https://itnext.io/install-docker-on-windows-10-home-d8e621997c1d

## install Confluent Platform
- images for individual components of Confluent platform is on Docker hub
	- use Docker Compose file that Confluent made
	- https://github.com/confluentinc/cp-docker-images
	- following tutorial clone repo and cd into cp-docker-images and check out branch for `5.2.1-post: git checkout 5.2.1-post`
	- in examples I used cp-all-in-one as sample which has a docker-compose.yml which contains these components
		- Zookeeper
		- kafka broker
		- schema registry
		- kafka connect
		- control center
		- KSQL Server
		- KSQL CLI
		- KSQL datagen
			- included for test purposes, should not used for production
		- Rest proxy
	- remember to increase memory for Docker at least 8gb
	- in docker-compose.yml
		- broker section
			- ports that Kafka broker listens on
				- typically 9092 in non-docker Kafka installation
				- Docker environment have Docker internal network and external (e.g. host machine for Docker containers)
					- why there is 29092 and 9092 for 2 listeners internal and external
	- cd into cp-all-in-one and run command
		- `docker-compose up -d --build`
			- pulls different images for Confluent Platform
			- `--build` flag builds Kafka Connect image together with datagen connector
			- creates containers and starts it up
			- `docker-compose` command starts pulling them from registry
		- to check if all containers are running use
			- `docker-compose ps` listing all containers related to images in docker-compose.yml file
		- can check if containers is up by using logs as well
			- e.g. `docker logs broker | Select-String -Pattern 'Started'`
			- can use grep and etc

## Control Center
- web UI for managing and monintoring Kafka
	- data streams
	- system health
	- config of Kafka Connect
- From Ports from the yml file, it is exposed to port 9021
	- view in `localhost:9021`
- can create Topics on left side of the page
- use command line tools
	- kafka-console-consumer
		- reads data from Kafka topic and writes data to standard outpu
	- kafka-console-producer
		- reads from standard output and writes tp kafka topic
	- to go into bash shell of Kafka broker container
		- `docker exec -it broker bash`
		- cd into /usr/bin directory with Kafka command tools
		- can open two command windows with it and execute
			- `./kafka-console-consumer --bootstrap-server broker:29092 --topic topicNameHere`
			- `./kafka-console-producer --broker-list broker:29092 --topic testTopic`
				- now you can enter in strings and the consumer will read data from broker topic and write to output

## .net code 
- create console apps for producer and consumer
- make sure to install nuget package for kafka.confluent
- then just read the comments for the example I made

---

# Setting up confluent without Docker
- used this tutorial, but this tutorial is old and missed a lot of steps
	- https://dotnetcorecentral.com/blog/asp-net-core-streaming-application-using-kafka-part-1/
	- in the end i skipped doing the .NET portion, but everything before that works after my modifications
- note to add environment variables
	- right click This PC -> Advanced system settings -> Environment Variables
- need to install JRE 8 
	- http://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html
	- Windows x64 Offline version
	- create User variable `JAVA_HOME` with path to the JRE
	- then in System variable there is a `Path`
		- append `%JAVA_HOME%\bin` at the end
			- different versions is differnt, if you see a long line, you need semicolon to end it
	- Big error that will pop up is that within **Bin** folder you need to create a `Server` folder with all of the copied contents of `Client` folder.
		- will cause an error later that says cant find server or something when trying to run kafka
- need to install Zookeeper 
	- http://zookeeper.apache.org/releases.html
	- remember to extract files, i placed zookeeper-x.x.xx into C:
	- inside config folder change zoo_sample.cfg into zoo.cfg
		- modify the file's dataDir to equal to e.g. `C:/zookeeper-3.6.0/data`
	- then similar to Java in **System Variable** create a ZOOKEEPER_HOME with path to installation folder
	- in same area appeand to end of `Path` %ZOOKEEPER_HOME%\bin
- need to install Kafka
	- http://kafka.apache.org/downloads.html
		- install the recommended Scala version
		- extract and place in C: as well
		- edit server.properties file so that `log.dir` uses `/kafka_logs` instead of default folder

## Running Kafka
- navigate to “C:\zookeeper-3.4.10\bin”
- the command `zksrver` will not work if not on command prompt, should work if set everything right
	- use `./zkServer.sh` to see list of commands you can do with it if on git bash or terminal
	- probably want to use `start`
- open up another command prompt to enter command to start kafka
	- “.\bin\windows\kafka-server-start.bat .\config\server.properties”
	- issue that can pop up....
		- java.lang.OutOfMemoryError: Java heap space while running kafka
		- one solution is to go to bin\windows and edit the kafka-server-start script to also be 512M instead of 1G

## Creating Kafka topic
- **NOTE** --zookeeper vs --bootstrap-server
	- older version of Kafka (v 0.9.0). Kafka use to store data on Kafka server and all offset related information like (current partition offsets) were stored in zookeeper
		- So for a consumer to run it requires both data and meta data.So for getting meta data, it has to call zookeeper
	- In new versions of Kafka i.e ( v 0.10.0 - above). It stores all topic metadata information(total partitions and their current offsets) in the `__consumer_offset` topic on the same Kafka server
		- So now only Kafka broker only need to communicate with zookeeper and consumer gets all data and metadata from kafkabroker itself so it now no longer need to communicate with zookeeper
- to create topic navigate to `c:\kafka\kafka_x.xx-x.0.0\bin\windows`
	- now to create topic with command
		- `kafka-topics.bat --create --zookeeper localhost:2181 --replication-factor 1 --partitions 1 --topic test`

## creating a producer and consumer to test
- command for starting producer type
	- `kafka-console-producer.bat --broker-list localhost:9092 --topic test`
- to create consumer type
	- kafka before version 2.0
		- `kafka-console-consumer.bat --zookeeper localhost:2181 --topic test`
	- kafka after version 2.0
		- `kafka-console-consumer.bat -bootstrap-server localhost:9092 -topic test2`
			- can add `-from-beginning` to show all of the things typed into the topic
			- finally works
		- now you can type text into producer and something will appear in consumer

## .NET code
- it was too out of date so I will have to look at another more recent one


---

# Kafka implementation in .Net core for microservice
- same steps as before to have 7zip, JRE8, kafka and Zookeeper installed
	- refer to previous instructions above
- create a topic
- test to see if works by running a consumer and producer after starting up zookeeper and kafka
	- you will need to have a consumer running to test .Net code later

## .NET code
- NOTE! you'll run into errors if your .Net Core is not 3.0+ like me
	- also needs visual studios 2019
- to start off make a new project
	- Visual C# -> Web -> ASP.NET Core web application
	- then asks you what type to make, I chose API
- right click and edit project file which opens the .csproj
	- can add this line for Confluent.Kafka then reboot
	- `<PackageReference Include="Confluent.Kafka" Version="1.4.0" />`
	- ignore above, it will cause an error, just go to manage nuget packages and install from there or wrap above between `<ItemGroup></ItemGroup>`
- Head into appsettings.json and add this above "AllowedHosts"
	- 
```csharp
"producer": {
    "bootstrapservers": "localhost:9092"
},
```
- now head to startup.cs file and add inside ConfigureServices
```csharp
var producerConfig = new ProducerConfig();
Configuration.Bind("producer", producerConfig);

services.AddSingleton<ProducerConfig>(producerConfig);
```
- next step is to create our producer controller
	- in controller folder add new controller and name it Producer
	- contents just refer to my file
- now to test I will use Postman
	- make a POST method to `https://localhost:44355/api/Producer/send?topic=topicYouMade`
	- Make sure you add content in Body, pick raw, and text changed to JSON (I was wondering why wasn't working for a while)
	- with contents of `{"Id":1, "Name":"testName"}`
	- now you'll see something popped up in your command prompt for consumer
- creating Consumer .Net code
	- create console app for consumer
	- main code used will be in main and after running the code, if you do the same POST like we did to test consumer, will be shown