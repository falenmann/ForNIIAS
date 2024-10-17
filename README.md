
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


Parks table
Stores information about parks.

Id - unique identifier of the park.
Name - name of the park.
AsuNumber - ACS (Automated Control System) number.
Type - type of park.
Direction - direction of travel.
Paths table
Stores information about paths in the park.

Id - unique identifier of the path.
AsuNumber - ACS number of the path.
IdPark - identifier of the park to which the path belongs.
Epcs table
Stores information about rolling stock units (cars).

Id - unique identifier of the unit.
Number - inventory number of the unit.
Type - type of rolling stock (for example, type of car).
EpcEvents table
Stores events associated with rolling stock units.

Time - time of the event.
IdPath - identifier of the path to which the event is associated.
Type - type of event (for example, arrival, departure, etc.).
NumberInOrder - number in the sequence of events.
IdEpc - rolling stock unit identifier.
EventArrivals table
Stores information about train arrival events.

Time - arrival time.
IdPath - track identifier where the train arrived.
TrainNumber - train number.
TrainIndex - train index.
EventDepartures table
Stores information about train departure events.

Time - departure time.
IdPath - track identifier where the train departed.
TrainNumber - train number.
TrainIndex - train index.
EventAdds table
Stores information about adding cars to the track.

Time - adding time.
IdPath - track identifier where the car is added.
Direction - direction of travel.
EventSubs table
Stores information about removing cars from the track.

Time - removing time.
IdPath - track identifier where the car is removed.
Direction - direction of travel.



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


