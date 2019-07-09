# Microservice API

[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=reliability_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)
[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=security_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)
[![SonarCloud](http://sonar.navigatorglass.com:9000/api/project_badges/measure?project=db762c49b56bd854f8e7fb1d03f7106468a27387&metric=sqale_rating)](http://sonar.navigatorglass.com:9000/dashboard?id=db762c49b56bd854f8e7fb1d03f7106468a27387)

[![Build Status](https://travis-ci.org/stuartshay/MicroService.svg?branch=develop)](https://travis-ci.org/stuartshay/MicroService)

### Purpose

The New York Application Team has been tasked with designing a microservice for there TestData System. The Application will be used by high profile clients and internal groups in the organization for Machine Learning Research and the Calculation Engine Product.

The Application will be a complete rearchitected solution using the cloud for optimal price and performance.  The API Schema has been enhanced and key performance metrics have been added. The API will implement versioning so end clients can seamlessly update to the new enhanced schema when ready.

The Business has defined the Percentile Function will use the same algorithm as MS Excel "PERCENTILE.INC" the database is serving data using float(8) and the application has defined the double data type for precision.

The Requirements for this project can be viewed at the following.

* [Business Requirements](/docfx/articles/requirements.md)
* [C# Coding Standards](/docfx/articles/csharp_coding_standards.md)

## Swagger

![](assets/swagger.png)

## Hosting Environments

### Development

Local Docker Postgres Database

```
cd docker
docker-compose -f docker-compose-local.yml -f docker-compose-metrics.yml pull
docker-compose -f docker-compose-local.yml -f docker-compose-metrics.yml up
```

AWS Postgres Development Database
```
cd docker
docker-compose -f docker-compose-development.yml pull
docker-compose -f docker-compose-development.yml up
```

Swagger API Documentation Page
```
http://<DOCKER_HOST>:5000/swagger/
```

### Docker Hub Images

 Image       |  Docker Hub | Image Size
------------ | ------------- | -------------
microservice-database | [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-database.svg)](https://hub.docker.com/r/stuartshay/microservice-database/) |[![](https://images.microbadger.com/badges/image/stuartshay/microservice-database.svg)](https://microbadger.com/images/stuartshay/microservice-database "Get your own image badge on microbadger.com") 
microservice-api-base | [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/)  | [![](https://images.microbadger.com/badges/image/stuartshay/microservice-api.svg)](https://microbadger.com/images/stuartshay/microservice-api "Get your own image badge on microbadger.com") 
microservice-api-build | [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) | [![](https://images.microbadger.com/badges/image/stuartshay/microservice-api.svg)](https://microbadger.com/images/stuartshay/microservice-api "Get your own image badge on microbadger.com")

### MyGet/NuGet Packages

 Package | Status  
------------ | -------------
MicroService.Data | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Data.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Data)
MicroService.Service | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Service.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Service)

### Jenkins Build Status

 Jenkins | Status  
------------ | -------------
Base Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-base)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-base/)
API  Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-build)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-build/)
Multi Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-multi)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-multi/)


### Build Commands

|  Build Type                 |  Linux/Mac                    | Windows                       |
| --------------------------- | ------------------------------|  -----------------------------|
| CI Build                    | ./build.sh  --target=CI-Build | .\build.ps1 --target=CI-Build |
| SonarQube Testing           | ./build.sh  --target=sonar    | .\build.ps1 --target=sonar    |

**docfx**

```powershell
docfx docfx/docfx.json
docfx docfx/docfx.json -p 9090 --serve

http://localhost:9090
```

### Reference

```
Percentile Calculation
https://stackoverflow.com/questions/8137391/percentile-calculation
```
