
Installation and configuration
Requirements
.NET 6.0 or newer
PostgreSQL

Before starting, you need to configure the connection to the PostgreSQL database. Connection data in appsettings.json specify your connection data:

"ConnectionStrings": {
  "PostgresConnection": "Host=....;Database=....;Username=....;Password=....;"
}

Database Setup
Install PostgreSQL.
Create the required database schema using the PostgresDbContext migrations or manually create the tables as per the models defined in the project:
Parks
Paths
Epcs
EpcEvents
EventArrivals
EventDepartures
EventAdds
EventSubs

To work, the database has the following structure and keys:

![image](https://github.com/user-attachments/assets/598b2384-9b8b-4e1d-828c-8e8736f184f3)



*Parks table. Stores information about parks.*

·Id - unique park identifier.

·Name - park name.

·AsuNumber - ACS (Automated Control System) number.

·Type - park type.

·Direction - direction of movement.



*IPaths table Stores information about paths in the park.*

·Id - unique path identifier.

·AsuNumber - ASU number of the path.

·IdPark - identifier of the park to which the path belongs.



*Table Epcs Stores information about rolling stock units (cars).*

·Id - unique identifier of the unit.

·Number - inventory number of the unit.

·Type - type of rolling stock (for example, type of car).



*Table EpcEvents Stores events related to rolling stock units.*

·Time - event time.

·IdPath - path identifier to which the event is related.

·Type - event type (e.g. arrival, departure, etc.).

·NumberInOrder - number in the event sequence.

·IdEpc - rolling stock unit identifier.



*EventArrivals table Stores information about train arrival events.*

·Time - arrival time.

·IdPath - identifier of the path where the train arrived.

·TrainNumber - train number.

·TrainIndex - train index.



*EventDepartures table Stores information about train departure events.*

·Time - departure time.

·IdPath - identifier of the track from which the train departed.

·TrainNumber - train number.

·TrainIndex - train index.



*EventAdds table Stores information about adding cars to the track.*

·Time - time of adding.

·IdPath - identifier of the track to which the car is added.

·Direction - direction of movement.



*EventSubs table Stores information about the removal of cars from the track.*

·Time - time of removal.

·IdPath - identifier of the track from which the car is removed.

·Direction - direction of movement.




To work in the client, the following is specified:
var channel = GrpcChannel.ForAddress("http://localhost:5012"...

You may need to change the data for connecting to the grpc server

Client Usage
Input the desired time period (start and end times) in the UTC format: yyyy-MM-dd HH:mm:ss.
Press the button Получить информацию о вагонах to query the gRPC server.
The results will be displayed in the table showing inventory number, arrival time, and departure time.


gRPC API
The gRPC service exposes the following API:

Method: GetWagons
Request: WagonRequest

StartTime (string): The start of the time period (format: yyyy-MM-dd HH:mm:ss).
EndTime (string): The end of the time period (format: yyyy-MM-dd HH:mm:ss).
Response: WagonResponse

Wagons (repeated Wagon):
InventoryNumber (string): The wagon's inventory number.
ArrivalTime (string): Arrival time (format: yyyy-MM-dd HH:mm:ss).
DepartureTime (string): Departure time (format: yyyy-MM-dd HH:mm:ss)

Troubleshooting
Invalid Date Format
Ensure that the input dates are in the correct format (yyyy-MM-dd HH:mm:ss) when querying from the client.


