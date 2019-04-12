# C# Task

[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=alert_status)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)
[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=reliability_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)
[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=security_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)
[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=sqale_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)

 Image       |  Docker Hub
------------ | -------------
microservice-database | [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-database.svg)](https://hub.docker.com/r/stuartshay/microservice-database/)
microservice-api-base | [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/)

 Jenkins | Status  
------------ | -------------
Base Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-base)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-base/)
API  Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-build)](https://jenkins.navigatorglass.com/job/NavigatorAPI/job/microservice-api-build/)

 Package | Status  
------------ | -------------
MicroService.Data | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Data.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Data)
MicroService.Service | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Service.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Service)

##

### Myget Package Deployment

Windows

```powershell
  $env:mygetApiKey = "adab4634-8ddb-4789-ae92-6461295ac69c"
  .\build.ps1 -target push-myget
```

Linux
 
```bash
 export mygetApiKey="adab4634-8ddb-4789-ae92-6461295ac69c"
./build.sh --target=push-myget
```

### DocFX

#### Prerequisites:

```
choco install docfx
```

#### Build and Serve Website

```
docfx docfx/docfx.json
docfx docfx/docfx.json --serve
```

```
http://localhost:8080
```

#### Deployment 
```powershell
 .\build.ps1 -target Generate-Docs
```










Create a microservice and a client consumer of the microservice.

The requirements of the service are as follows:

* Written in C# using .Net Core Web API
* Read data from a PostgreSQL database (that is provided)
* Compute the 99.5th percentile of the dataset (in C#) and return the result
  * **Do not** sort the data in SQL - this is a C# task.
  * The calculated percentile result should use the same algorithm as MS Excel "PERCENTILE.INC", the answer for this dataset is : 9949.9563797144219



###Notes: 
* The dataset will always return 1,000,000 unsorted values
* The PostgreSQL database connection details are:
  * Server: testvar.postgres.database.azure.com
  * User: risky@testvar
  * Password: c7CsdGuoWY%VryALXk
  * **Note** : You will need to provide us with any IP address you are using to access the database. Email : Nick.Francis@riskfirst.com
* To retrieve the values execute: `select * from public.get_data();`
* The database is read-only
  * You cannot add stored procedures/functions/tables etc.



Your solution will be assessed on the following:

* Implementation of the requirements
* Performance
* Code quality
* Testing style and quality
* Scaleability


Take as long as you feel is necessary to complete the task to fulfil the requirements.
But as guidance, aim to spend no more than 3-4 hours.  

Once complete, please commit your solution to GitHub and provide a link. The code should be of production quality.
Please document any assumptions and design decisions in a README file.
