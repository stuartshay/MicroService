# C# Task
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