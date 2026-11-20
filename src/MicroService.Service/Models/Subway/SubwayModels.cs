using System.Collections.Generic;

namespace MicroService.Service.Models.Subway
{
    /// <summary>
    /// A subway service (route) with the MTA's colors for its bullet.
    /// </summary>
    /// <param name="Service">The service code: 1, A, SIR, 5 Peak, SF, SR, ST ...</param>
    /// <param name="Name">The MTA's service name, e.g. "Broadway / 7AV Local".</param>
    /// <param name="Color">The line color, e.g. #EE352E.</param>
    /// <param name="TextColor">The color of text on the line color.</param>
    public sealed record SubwayRoute(string Service, string Name, string Color, string TextColor);

    /// <summary>
    /// The stations near each requested point, and every route that serves them.
    /// </summary>
    /// <param name="Points">One entry per requested point, in request order.</param>
    /// <param name="Services">The union of the stations' daytime routes, 1-7 then letters.</param>
    public sealed record SubwayStationsNear(IReadOnlyList<SubwayPointStations> Points, IReadOnlyList<string> Services);

    /// <summary>
    /// The stations within the radius of one point, nearest first.
    /// </summary>
    public sealed record SubwayPointStations(double Latitude, double Longitude, IReadOnlyList<SubwayStationDistance> Stations);

    /// <summary>
    /// A station from the MTA station list and its distance from the requested point.
    /// </summary>
    /// <param name="StationId">MTA Station ID.</param>
    /// <param name="ComplexId">MTA Complex ID: stations in one complex are connected.</param>
    /// <param name="GtfsStopId">GTFS stop id.</param>
    /// <param name="StopName">Station name, e.g. "Times Sq-42 St".</param>
    /// <param name="Line">The line the station is on, e.g. "Broadway - 7Av".</param>
    /// <param name="Borough">MTA borough code: M, Bx, Bk, Q, SI.</param>
    /// <param name="Routes">Daytime routes, 1-7 then letters. S is a shuttle.</param>
    /// <param name="Structure">Subway, Elevated, Open Cut, Viaduct, At Grade or Embankment.</param>
    /// <param name="Latitude">GTFS latitude.</param>
    /// <param name="Longitude">GTFS longitude.</param>
    /// <param name="Distance">Distance from the point in metres.</param>
    public sealed record SubwayStationDistance(
        int StationId,
        int ComplexId,
        string GtfsStopId,
        string StopName,
        string Line,
        string Borough,
        IReadOnlyList<string> Routes,
        string Structure,
        double Latitude,
        double Longitude,
        double Distance);
}
